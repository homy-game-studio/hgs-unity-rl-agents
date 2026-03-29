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
    [SerializeField] float steerSmoothing = 1f;

    [Header("Values")]
    [SerializeField] float antiRollForce = 8000f;
    [SerializeField] float acelerationForce = 1000f;
    [SerializeField] float breakingForce = 3000f;

    public float steer;
    public float aceleration;
    public float breaking;

    public float ForwardVelocity => transform.InverseTransformDirection(rb.linearVelocity).z;

    Vector3 _startPosition;
    Quaternion _startRotation;

    private void Awake()
    {
        rb.centerOfMass = centerOfMass.localPosition;
        _startPosition = rb.position;
        _startRotation = rb.rotation;
    }

    public void Respawn()
    {
        rb.position = _startPosition;
        rb.rotation = _startRotation;
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
        if (breaking > 0) return;

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
        float force = Mathf.Clamp01(breaking) * breakingForce;
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
