using System;
using HGS.RLAgents;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Sensors;
using UnityEngine;


public class TruckAgent : Agent
{
    [Header("Presentation")]
    [SerializeField] MeshRenderer[] bodyParts;

    [Header("Flags")]
    [SerializeField] bool heuristic = false;

    [Header("Physics")]
    [SerializeField] TruckPhysics truckPhysics;
    [SerializeField] TruckTraillerPhysics trailerPhysics;

    [Header("Sensors")]
    [SerializeField] CollisionSensor collisionSensor;
    [SerializeField] RaySensor truckRaySensor;
    [SerializeField] RaySensor trailerRaySensor;
    [SerializeField] Transform parkingZone;
    [SerializeField] float maxDistanceToParkingZone = 20f;
    [SerializeField] float maxObservedSpeed = 10f;

    [Header("Debug (physics test)")]
    [Tooltip("Ignores the network: every agent applies debugOutput (x = throttle, y = brake, z = steer)")]
    [SerializeField] bool debugFixedOutput = false;
    [SerializeField] Vector3 debugOutput = new Vector3(-0.5f, 0f, 0f);

    [Header("Demonstration (DemonstrationRecorder)")]
    [Tooltip("Meters of random offset from the default spawn at every new recording, so the demonstrations cover different starts")]
    [SerializeField] float demoSpawnJitterPosition = 2f;
    [Tooltip("Degrees of random yaw around the default spawn at every new recording")]
    [SerializeField] float demoSpawnJitterYaw = 15f;

    [Header("Actions")]
    [SerializeField, Range(0f, 0.95f)] float brakeDeadZone = 0.1f;

    // Non-ray values in CollectObservations. Model.inputSize = rays of both sensors + this.
    const int ScalarObservationCount = 10;
    bool _inputSizeChecked;

    public float FowardVelocity => truckPhysics.ForwardVelocity;
    public float MaxObservedSpeed => maxObservedSpeed;
    public float MaxDistanceToParkingZone => maxDistanceToParkingZone;
    // Planar (XZ) distance: the parking zone sits lower than the trailer, and that height gap must not count
    public float NormalizedDistanceToParkingZone
    {
        get
        {
            var delta = trailerPhysics.CenterOfMassPosition - parkingZone.position;
            delta.y = 0f;
            return delta.magnitude / maxDistanceToParkingZone;
        }
    }
    // Headings are compared on the ground plane: pitch and roll (suspension, ramps) must not count
    // as misalignment. The zone's forward is the exit direction, so parked = trailer facing it.
    static Vector3 Flat(Vector3 direction)
    {
        direction.y = 0f;
        return direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector3.zero;
    }

    Vector3 TrailerHeading => Flat(trailerPhysics.transform.forward);
    Vector3 TruckHeading => Flat(transform.forward);

    // Signed: positive = the trailer must turn right to face the zone's forward. A cosine (below)
    // loses which way to turn, and a bearing to the zone is unstable when the trailer is almost on it.
    public float HeadingErrorToParkingZone => Vector3.SignedAngle(TrailerHeading, Flat(parkingZone.forward), Vector3.up);
    public float AlignmentToParkingZone => Vector3.Dot(TrailerHeading, Flat(parkingZone.forward));
    public float AlignmentToTrailer => Vector3.Dot(TruckHeading, TrailerHeading);
    public float AngleToTrailer => Vector3.SignedAngle(TruckHeading, TrailerHeading, Vector3.up);

    public bool IsCollidedWithMap { get; private set; } = false;

    public TruckPhysics Truck => truckPhysics;
    public TruckTraillerPhysics Trailer => trailerPhysics;

    // Physics test scene: the network and the keyboard are ignored, whoever sets this calls Drive()
    public bool ExternalControl { get; set; }

    public void Drive(float throttle, float brake, float steer)
    {
        truckPhysics.aceleration = throttle;
        truckPhysics.breaking = brake;
        truckPhysics.steer = steer;
    }

    // Argument: speed of the impact (m/s)
    public Action<float> onCollideWithMapEvt;

    // Default spawn pose of the truck (trailer follows it as one rigid body)
    public Vector3 DefaultSpawnPosition => truckPhysics.StartPosition;
    public Quaternion DefaultSpawnRotation => truckPhysics.StartRotation;

    public float StartNormalizedDistanceToParkingZone { get; private set; }
    public float StartAlignmentToParkingZone { get; private set; }
    public float StartAlignmentToTrailer { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        RefreshStartValues();
        collisionSensor.onCollisionEnterEvent += HandleCollision;
    }

    private void OnDestroy()
    {
        if (collisionSensor != null) collisionSensor.onCollisionEnterEvent -= HandleCollision;
    }

    protected override void Update()
    {
        base.Update();
        if (heuristic && !ExternalControl)
        {
            var breakVal = Input.GetKey(KeyCode.Space) ? 0.5f : 0f;
            truckPhysics.breaking = breakVal;

            truckPhysics.aceleration = Input.GetAxis("Vertical");
            truckPhysics.steer = Input.GetAxis("Horizontal");

            if (Input.GetKeyUp(KeyCode.P))
            {
                Stop();
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                Respawn();
            }
        }
    }

    public override void SetGenome(int id, Genome genome)
    {
        base.SetGenome(id, genome);
        var color = new Color(
            Mathf.Clamp01((genome.GetGene(0) + 1f) / 2f),
            Mathf.Clamp01((genome.GetGene(1) + 1f) / 2f),
            Mathf.Clamp01((genome.GetGene(2) + 1f) / 2f)
        );

        for (int i = 0; i < bodyParts.Length; i++)
        {
            bodyParts[i].material.color = color;
        }
    }

    protected override float[] CollectObservations()
    {
        truckRaySensor.Sense();
        trailerRaySensor.Sense();

        var observations = new float[truckRaySensor.Infos.Length + trailerRaySensor.Infos.Length + ScalarObservationCount];
        int index = 0;

        if (!_inputSizeChecked)
        {
            _inputSizeChecked = true;
            if (model.inputSize != observations.Length)
            {
                Debug.LogError($"Model '{model.id}' inputSize is {model.inputSize} but the agent collects {observations.Length} observations (rays + {ScalarObservationCount}).");
            }
        }

        // Distances are compressed (signed square root): what matters most, e.g. when to brake,
        // happens at small normalized distances, which would otherwise be almost invisible
        for (int i = 0; i < truckRaySensor.Infos.Length; i++)
            observations[index++] = Compress(truckRaySensor.Infos[i].distance);

        for (int i = 0; i < trailerRaySensor.Infos.Length; i++)
            observations[index++] = Compress(trailerRaySensor.Infos[i].distance);

        observations[index++] = Mathf.Clamp(truckPhysics.ForwardVelocity / maxObservedSpeed, -1f, 1f);
        observations[index++] = Compress(Mathf.Clamp01(NormalizedDistanceToParkingZone));
        observations[index++] = AlignmentToParkingZone;
        // Signed angles keep the side information (left/right) that a dot product loses
        observations[index++] = HeadingErrorToParkingZone / 180f;
        observations[index++] = AngleToTrailer / 180f;
        observations[index++] = AlignmentToTrailer;

        // Where the zone is, seen from the trailer (x = right, z = ahead)
        var trailerCenter = trailerPhysics.CenterOfMassPosition;
        var zoneInTrailer = (trailerPhysics.transform.InverseTransformPoint(parkingZone.position)
                             - trailerPhysics.transform.InverseTransformPoint(trailerCenter)) / maxDistanceToParkingZone;
        observations[index++] = Compress(Mathf.Clamp(zoneInTrailer.x, -1f, 1f));
        observations[index++] = Compress(Mathf.Clamp(zoneInTrailer.z, -1f, 1f));

        // Where the trailer is, seen from the zone (x = lateral offset, z = depth along the slot axis)
        var trailerInZone = parkingZone.InverseTransformPoint(trailerCenter) / maxDistanceToParkingZone;
        observations[index++] = Compress(Mathf.Clamp(trailerInZone.x, -1f, 1f));
        observations[index++] = Compress(Mathf.Clamp(trailerInZone.z, -1f, 1f));

        return observations;
    }

    // Signed square root: keeps the sign and stretches small magnitudes (0.04 -> 0.2)
    static float Compress(float value) => Mathf.Sign(value) * Mathf.Sqrt(Mathf.Abs(value));

    protected override void EvaluateOutput(float[] output)
    {
        if (heuristic || ExternalControl) return;

        // Physics test: ignores the network and applies the same outputs to every agent
        if (debugFixedOutput) output = new[] { debugOutput.x, debugOutput.y, debugOutput.z };

        truckPhysics.aceleration = output[0];
        // Output is tanh: values under the dead zone mean "do not brake"
        truckPhysics.breaking = Mathf.Clamp01((output[1] - brakeDeadZone) / (1f - brakeDeadZone));
        truckPhysics.steer = output[2];
    }

    private void HandleCollision(Collision collision)
    {
        if (IsCollidedWithMap) return;
        if (!collision.gameObject.CompareTag("Map")) return;

        // The rigidbody velocity is already changed by the impact here (and Stop() zeroes it),
        // so use the relative velocity of the contact, which is the speed before the impact
        var impactSpeed = collision.relativeVelocity.magnitude;

        Stop();
        IsCollidedWithMap = true;
        onCollideWithMapEvt?.Invoke(impactSpeed);
    }

    public override void Stop()
    {
        truckPhysics.Stop();
        trailerPhysics.Stop();
    }

    public override void Respawn() => RespawnAt(DefaultSpawnPosition, DefaultSpawnRotation);

    // Same space as the network output: the brake goes back through the dead zone mapping of EvaluateOutput
    public override float[] GetAppliedAction()
    {
        var brake = truckPhysics.breaking;
        var brakeOutput = brake > 0f ? brakeDeadZone + brake * (1f - brakeDeadZone) : 0f;
        return new[] { truckPhysics.aceleration, brakeOutput, truckPhysics.steer };
    }

    public override void RespawnForDemonstration()
    {
        var jitter = UnityEngine.Random.insideUnitCircle * demoSpawnJitterPosition;
        var position = DefaultSpawnPosition + new Vector3(jitter.x, 0f, jitter.y);
        var rotation = DefaultSpawnRotation * Quaternion.Euler(0f, UnityEngine.Random.Range(-demoSpawnJitterYaw, demoSpawnJitterYaw), 0f);
        RespawnAt(position, rotation);
    }

    // Places the truck at the given pose and the trailer with the same rigid transform
    public void RespawnAt(Vector3 truckPosition, Quaternion truckRotation)
    {
        truckPhysics.Stop();
        trailerPhysics.Stop();

        var truckStart = truckPhysics.StartPosition;
        var deltaRotation = truckRotation * Quaternion.Inverse(truckPhysics.StartRotation);

        truckPhysics.Respawn(truckPosition, truckRotation);
        trailerPhysics.Respawn(
            truckPosition + deltaRotation * (trailerPhysics.StartPosition - truckStart),
            deltaRotation * trailerPhysics.StartRotation);

        IsCollidedWithMap = false;
        RefreshStartValues();
    }

    void RefreshStartValues()
    {
        Physics.SyncTransforms();
        StartNormalizedDistanceToParkingZone = NormalizedDistanceToParkingZone;
        StartAlignmentToParkingZone = AlignmentToParkingZone;
        StartAlignmentToTrailer = AlignmentToTrailer;
    }
}
