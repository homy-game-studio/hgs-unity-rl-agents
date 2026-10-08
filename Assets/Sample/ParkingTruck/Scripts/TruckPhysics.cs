using UnityEngine;

public class TruckPhysics : MonoBehaviour
{
    [Header("Referencies")]
    [SerializeField] Rigidbody rb;
    [SerializeField] WheelCollider[] wheelColliders;
    [SerializeField] Transform[] wheelTransforms;
    [SerializeField] Vector3 angleCorrection;
    [SerializeField] bool syncVisualWheels = true;

    [Header("Center Of Mass")]
    [SerializeField] Transform centerOfMass;

    [Header("Max Values")]
    [SerializeField] float maxSteerAngle = 30f;

    [Header("Smoothing")]
    [SerializeField] float steerSmoothing = 180f; // degrees per second

    [Header("Values")]
    [SerializeField] float antiRollForce = 8000f;
    [SerializeField] float acelerationForce = 1000f;
    [SerializeField] float breakingForce = 3000f;

    [Header("Idle brake")]
    [Tooltip("Holds the truck when the throttle is released, so stopping does not need a separate brake output")]
    [SerializeField] bool idleBrake = true;
    [SerializeField] float idleBrakeThrottle = 0.1f;
    [Range(0f, 1f)]
    [SerializeField] float idleBrakeFraction = 0.5f;

    public float steer;
    public float aceleration;
    public float breaking;

    public float ForwardVelocity => transform.InverseTransformDirection(rb.linearVelocity).z;

    Vector3 _startPosition;
    Quaternion _startRotation;

    public Vector3 StartPosition => _startPosition;
    public Quaternion StartRotation => _startRotation;

    private void Awake()
    {
        rb.centerOfMass = centerOfMass.localPosition;
        _startPosition = rb.position;
        _startRotation = rb.rotation;
    }

    public void Respawn() => Respawn(_startPosition, _startRotation);

    public void Respawn(Vector3 position, Quaternion rotation)
    {
        rb.position = position;
        rb.rotation = rotation;

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].motorTorque = 0f;
            wheelColliders[i].brakeTorque = 0f;
            wheelColliders[i].steerAngle = 0f;
        }

        Physics.SyncTransforms();
    }

    // Update is called once per frame
    void Update()
    {
        if (syncVisualWheels) SyncWheels();
    }

    private void FixedUpdate()
    {
        Steer(Time.fixedDeltaTime);
        Acelerate();
        Break();
        ApplyTractionControl();
        ApplyAntiRoll(wheelColliders[0], wheelColliders[2]);
        ApplyAntiRoll(wheelColliders[1], wheelColliders[3]);
    }

    void SyncWheels()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;
            wheelColliders[i].GetWorldPose(out pos, out rot);

            // Corrige a rotacao
            rot *= Quaternion.Euler(angleCorrection);

            wheelTransforms[i].position = pos;
            wheelTransforms[i].rotation = rot;
        }
    }

    void Acelerate()
    {
        if (breaking > 0)
        {
            // Braking cuts the engine: do not keep the last torque applied
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                wheelColliders[i].motorTorque = 0f;
            }
            return;
        }

        float force = Mathf.Clamp(aceleration, -1f, 1f) * acelerationForce;

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].motorTorque = force;
        }
    }

    void ApplyTractionControl()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelHit hit;
            wheelColliders[i].GetGroundHit(out hit);
            if (hit.forwardSlip > 0.5f)
            {
                wheelColliders[i].motorTorque = 0;
            }
        }
    }

    void ApplyAntiRoll(WheelCollider left, WheelCollider right)
    {
        WheelHit hit;
        float travelLeft = 1.0f;
        float travelRight = 1.0f;

        bool groundedLeft = left.GetGroundHit(out hit);
        if (groundedLeft)
            travelLeft = (-left.transform.InverseTransformPoint(hit.point).y - left.radius) / left.suspensionDistance;

        bool groundedRight = right.GetGroundHit(out hit);
        if (groundedRight)
            travelRight = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;

        float force = (travelLeft - travelRight) * antiRollForce;

        if (groundedLeft)
            rb.AddForceAtPosition(left.transform.up * -force, left.transform.position);

        if (groundedRight)
            rb.AddForceAtPosition(right.transform.up * force, right.transform.position);
    }

    void Break()
    {
        float brake = Mathf.Clamp01(breaking);

        // Fades out as the throttle grows (full at 0, none at the threshold), so a small throttle
        // can still creep forward instead of hitting a sudden wall of brake
        if (idleBrake && idleBrakeThrottle > 0f)
        {
            float idle = 1f - Mathf.Clamp01(Mathf.Abs(aceleration) / idleBrakeThrottle);
            brake = Mathf.Max(brake, idleBrakeFraction * idle);
        }

        float force = brake * breakingForce;
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].brakeTorque = force;
        }
    }

    void Steer(float dt)
    {
        float steerAngle = Mathf.Clamp(steer, -1, 1) * this.maxSteerAngle;
        wheelColliders[0].steerAngle = Mathf.MoveTowards(wheelColliders[0].steerAngle, steerAngle, steerSmoothing * dt);
        wheelColliders[2].steerAngle = Mathf.MoveTowards(wheelColliders[2].steerAngle, steerAngle, steerSmoothing * dt);
    }

    public void Stop()
    {
        aceleration = 0;
        breaking = 1;
        steer = 0;
        wheelColliders[0].steerAngle = 0;
        wheelColliders[2].steerAngle = 0;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
