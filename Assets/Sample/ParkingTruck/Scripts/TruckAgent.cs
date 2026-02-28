using System.Runtime.CompilerServices;
using HGS.RLAgents;
using HGS.RLAgents.Evolution;
using HGS.RLAgents.Sensors;
using UnityEngine;

public class TruckAgent : Agent
{
    [Header("Truck Parts")]
    [SerializeField] Transform frontLeftWheel;
    [SerializeField] Transform frontRightWheel;
    [SerializeField] Transform backLeftWheel;
    [SerializeField] Transform backRightWheel;
    [SerializeField] Transform cabin;
    [SerializeField] Transform trailer;

    [Header("Skin")]
    [SerializeField] SpriteRenderer cabinSprite;
    [SerializeField] SpriteRenderer trailerSprite;

    [Header("Sensors")]
    [SerializeField] RaySensor2D cabinSensor;
    [SerializeField] RaySensor2D trailerSensor;
    [SerializeField] IntersectionSensor2D intersectionSensor2D;
    [SerializeField] CollisionSensor2D collisionSensor2D;
    [SerializeField] Transform parkingZone;

    [Header("Movement Settings")]
    [SerializeField] float steerSpeed = 5f;
    [SerializeField] float maxSteerAngle = 30f;
    [SerializeField] float engineForce = 1.0f;
    [SerializeField] public float maxForwardSpeed = 10f;
    [SerializeField] float rollingResistance = 1.5f;
    [SerializeField] float lateralGrip = 8f;

    Rigidbody2D _cabinRb;
    Rigidbody2D _trailerRb;
    private float _currentSteer = 0f;

    Vector3 _spawnCabinPos;
    Vector3 _spawnTrailerPos;
    Quaternion _spawnCabinRot;
    Quaternion _spawnTrailerRot;

    public bool debug = false;
    public float SteerInput { get; set; }
    public float ThrottleInput { get; set; }
    public float ForwardSpeed => Vector2.Dot(_cabinRb.linearVelocity, cabin.right);
    public bool IsCollidedWithMap { get; private set; }
    public float ParkingPercent { get; private set; }
    public float TrailerAlignmentToParking => Vector2.Dot(trailer.right, parkingZone.right);
    public float AngleToParkingZone
    {
        get
        {
            float angle = Vector2.SignedAngle(trailer.right, parkingZone.right);
            return Mathf.Abs(angle) / 180f;
        }
    }
    public float AngleBetweenCabinAndTrailer
    {
        get
        {
            float angle = Vector2.SignedAngle(cabin.right, trailer.right);
            return Mathf.Abs(angle) / 180f;
        }
    }

    public bool IsNextToParkingZone =>
        Vector2.Distance(trailer.position, parkingZone.position) < 2f;

    public Vector2 DirectionToParkingZone =>
        (parkingZone.position - trailer.position).normalized;

    protected override void Awake()
    {
        base.Awake();
        _cabinRb = cabin.GetComponent<Rigidbody2D>();
        _trailerRb = trailer.GetComponent<Rigidbody2D>();
        _spawnCabinPos = cabin.position;
        _spawnTrailerPos = trailer.position;
        _spawnCabinRot = cabin.rotation;
        _spawnTrailerRot = trailer.rotation;
        collisionSensor2D.onCollisionEnter2DEvent += OnDetectCollision;
    }

    protected override void Update()
    {
        base.Update();
        //SteerInput = -Input.GetAxis("Horizontal");
        //ThrottleInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        ApplyThrottle();
        ApplyRollingResistance();
        ApplyLateralGrip(_cabinRb, cabin);
        ApplyLateralGrip(_trailerRb, trailer);
        ApplySteering();
    }

    public override void SetGenome(int id, Genome genome)
    {
        base.SetGenome(id, genome);
        var color = new Color(
            (genome.GetGene(0) + 1f) / 2f,
            (genome.GetGene(1) + 1f) / 2f,
            (genome.GetGene(2) + 1f) / 2f
        );
        cabinSprite.color = color;
        trailerSprite.color = color * 0.8f;
    }

    private void ApplyThrottle()
    {
        if (ThrottleInput == 0f) return;

        // Limite de velocidade real
        if (ForwardSpeed >= maxForwardSpeed)
            return;

        Vector2 force = cabin.right * engineForce * ThrottleInput;
        _cabinRb.AddForce(force, ForceMode2D.Force);
    }

    private void ApplyRollingResistance()
    {
        Vector2 velocity = _cabinRb.linearVelocity;
        Vector2 resistance = -velocity * rollingResistance;
        _cabinRb.AddForce(resistance, ForceMode2D.Force);
    }

    private void ApplyLateralGrip(Rigidbody2D rb, Transform part)
    {
        Vector2 velocity = rb.linearVelocity;

        Vector2 forward = part.right;
        Vector2 right = part.up;

        float forwardSpeed = Vector2.Dot(velocity, forward);
        float lateralSpeed = Vector2.Dot(velocity, right);

        // Remove parte da velocidade lateral (simula pneu)
        Vector2 lateralCorrection =
            -right * lateralSpeed * lateralGrip;

        rb.AddForce(lateralCorrection, ForceMode2D.Force);
    }

    private void ApplySteering()
    {
        // Suavização do steer
        float targetSteer = SteerInput * maxSteerAngle;
        _currentSteer = Mathf.MoveTowards(
            _currentSteer,
            targetSteer,
            steerSpeed * Time.fixedDeltaTime
        );

        float steerRad = _currentSteer * Mathf.Deg2Rad;
        float wheelBase = Vector2.Distance(frontLeftWheel.position, backLeftWheel.position);
        float angularVelocity = 0f;

        if (Mathf.Abs(steerRad) > 0.001f)
        {
            float radius = wheelBase / Mathf.Tan(steerRad);
            Vector2 velocity = _cabinRb.linearVelocity;
            float forwardSpeed = Vector2.Dot(velocity, cabin.right);
            angularVelocity = ForwardSpeed / radius;
        }

        // Movimento angular
        float targetRot =
            _cabinRb.rotation +
            angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime;

        _cabinRb.MoveRotation(targetRot);
        frontLeftWheel.localRotation = Quaternion.Euler(0, 0, _currentSteer);
        frontRightWheel.localRotation = Quaternion.Euler(0, 0, _currentSteer);
    }

    protected override float[] CollectObservations()
    {
        cabinSensor.Sense();
        trailerSensor.Sense();
        intersectionSensor2D.Sense();

        ParkingPercent = intersectionSensor2D.Info.percentage;

        return new float[] {
            cabinSensor.Infos[0].distance,
            cabinSensor.Infos[1].distance,
            cabinSensor.Infos[2].distance,
            trailerSensor.Infos[0].distance,
            trailerSensor.Infos[1].distance,
            trailerSensor.Infos[2].distance,

            ForwardSpeed/maxForwardSpeed,
            _currentSteer/maxSteerAngle,

            DirectionToParkingZone.x,
            DirectionToParkingZone.y,

            TrailerAlignmentToParking,
            AngleToParkingZone,
            AngleBetweenCabinAndTrailer,
            ParkingPercent,
        };
    }

    protected override void EvaluateOutput(float[] output)
    {
        ThrottleInput = Mathf.Clamp(output[0], -1f, 1f);
        SteerInput = Mathf.Clamp(output[1], -1f, 1f);
    }

    public override void Stop()
    {
        SteerInput = 0;
        ThrottleInput = 0;
        _cabinRb.linearVelocity = Vector2.zero;
        _trailerRb.linearVelocity = Vector2.zero;
        _cabinRb.angularVelocity = 0;
        _trailerRb.angularVelocity = 0;
        _currentSteer = 0;
        _cabinRb.simulated = false;
        _trailerRb.simulated = false;
    }

    public override void Respawn()
    {
        cabin.position = _spawnCabinPos;
        cabin.rotation = _spawnCabinRot;
        trailer.position = _spawnTrailerPos;
        trailer.rotation = _spawnTrailerRot;
        IsCollidedWithMap = false;
        _cabinRb.simulated = true;
        _trailerRb.simulated = true;
        ParkingPercent = 0;
    }

    private void OnDetectCollision(Collision2D collision2D)
    {
        if (collision2D.gameObject.CompareTag("Map"))
        {
            IsCollidedWithMap = true;
        }
    }

    private void OnGUI()
    {
        if (!debug) return;

        // font size
        GUI.skin.label.fontSize = 28;

        GUI.Label(new Rect(10, 10, 500, 50), $"Speed: {ForwardSpeed:F2}");
        GUI.Label(new Rect(10, 50, 500, 50), $"Steer: {_currentSteer:F2}");
        GUI.Label(new Rect(10, 100, 500, 50), $"Parking Percent: {ParkingPercent:P2}");
        GUI.Label(new Rect(10, 200, 500, 50), $"Trailer Align: {TrailerAlignmentToParking:P2}");
        GUI.Label(new Rect(10, 250, 500, 50), $"Angle to Parking: {AngleToParkingZone:P2}");
        GUI.Label(new Rect(10, 300, 500, 50), $"Angle Cabin-Trailer: {AngleBetweenCabinAndTrailer:P2}");
        // Reward
        GUI.Label(new Rect(10, 350, 500, 50), $"Reward: {fitness:F2}");
    }
}