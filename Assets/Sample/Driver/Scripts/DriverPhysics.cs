using UnityEngine;

public class DriverPhysics : MonoBehaviour
{
    [Header("Referencies")]
    [SerializeField] Rigidbody rb;
    [SerializeField] WheelCollider[] wheelColliders;
    [SerializeField] Transform[] wheelTransforms;
    [SerializeField] Vector3 angleCorrection;
    [SerializeField] bool syncVisualWheels = true;

    [Header("Max Values")]
    [SerializeField] float maxSteerAngle = 30f;

    [Header("Smoothing")]
    [SerializeField] float steerSmoothing = 1f;

    [Header("Values")]
    [SerializeField] float acelerationForce = 1000f;
    [SerializeField] float breakingForce = 3000f;

    public float steer;
    public float aceleration;
    public float breaking;

    public float ForwardVelocity => rb.linearVelocity.sqrMagnitude;

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

        float force = Mathf.Clamp01(aceleration) * acelerationForce;

        wheelColliders[1].motorTorque = force;
        wheelColliders[3].motorTorque = force;
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
