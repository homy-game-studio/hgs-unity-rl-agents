using UnityEngine;

public class TruckTraillerPhysics : MonoBehaviour
{
    [Header("Referencies")]
    [SerializeField] Rigidbody rb;
    [SerializeField] WheelCollider[] wheelColliders;
    [SerializeField] Transform[] wheelTransforms;
    [SerializeField] Vector3 angleCorrection;
    [SerializeField] bool syncVisualWheels = true;
    [SerializeField] float breakingForce;

    [Header("Center Of Mass")]
    [SerializeField] Transform centerOfMass;

    // Reference point of the trailer for measuring against the parking zone: the transform
    // pivot is not the middle of the trailer, so measuring from it favors entering nose first
    public Vector3 CenterOfMassPosition => centerOfMass != null ? centerOfMass.position : transform.position;

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

        // Release the trailer brakes applied by Stop()
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].motorTorque = 0f;
            wheelColliders[i].brakeTorque = 0f;
        }

        Physics.SyncTransforms();
    }

    // Update is called once per frame
    void Update()
    {
        if (syncVisualWheels) SyncWheels();
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

    public void Stop()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            wheelColliders[i].brakeTorque = breakingForce;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
