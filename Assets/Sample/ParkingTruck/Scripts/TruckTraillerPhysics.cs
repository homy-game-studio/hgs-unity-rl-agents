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
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
