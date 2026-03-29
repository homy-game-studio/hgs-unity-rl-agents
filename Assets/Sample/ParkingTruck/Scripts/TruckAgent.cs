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
    [SerializeField] RaySensor truckRaySensor;
    [SerializeField] RaySensor trailerRaySensor;
    [SerializeField] Transform parkingZone;
    [SerializeField] float maxDistanceToParkingZone = 20f;

    public float NormalizedDistanceToParkingZone => Vector3.Distance(trailerPhysics.transform.position, parkingZone.position) / maxDistanceToParkingZone;
    public float AngleToParkingZone => Vector3.SignedAngle(trailerPhysics.transform.forward, (parkingZone.position - trailerPhysics.transform.position).normalized, Vector3.up);
    public float AlignmentToParkingZone => Vector3.Dot(trailerPhysics.transform.forward, parkingZone.forward);
    public float AlignmentToTrailer => Vector3.Dot(transform.forward, trailerPhysics.transform.forward);
    public float AngleToTrailer => Vector3.SignedAngle(transform.forward, trailerPhysics.transform.forward, Vector3.up);

    public bool IsCollidedWithMap { get; private set; } = false;
    public Action onCollideWithMapEvt;

    public float StartNormalizedDistanceToParkingZone { get; private set; }
    public float StartAlignmentToParkingZone { get; private set; }
    public float StartAlignmentToTrailer { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        StartNormalizedDistanceToParkingZone = NormalizedDistanceToParkingZone;
        StartAlignmentToParkingZone = AlignmentToParkingZone;
        StartAlignmentToTrailer = AlignmentToTrailer;
    }


    protected override void Update()
    {
        base.Update();
        if (heuristic)
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
            (genome.GetGene(0) + 1f) / 2f,
            (genome.GetGene(1) + 1f) / 2f,
            (genome.GetGene(2) + 1f) / 2f
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

        return new float[] {
                truckRaySensor.Infos[0].distance,
                truckRaySensor.Infos[1].distance,
                truckRaySensor.Infos[2].distance,
                truckRaySensor.Infos[3].distance,
                truckRaySensor.Infos[4].distance,
                truckRaySensor.Infos[5].distance,
                truckRaySensor.Infos[6].distance,
                truckRaySensor.Infos[7].distance,
                truckRaySensor.Infos[8].distance,
                truckRaySensor.Infos[9].distance,
                trailerRaySensor.Infos[0].distance,
                trailerRaySensor.Infos[1].distance,
                trailerRaySensor.Infos[2].distance,
                trailerRaySensor.Infos[3].distance,
                trailerRaySensor.Infos[4].distance,
                trailerRaySensor.Infos[5].distance,
                trailerRaySensor.Infos[6].distance,
                trailerRaySensor.Infos[7].distance,
                trailerRaySensor.Infos[8].distance,
                trailerRaySensor.Infos[9].distance,
                truckPhysics.ForwardVelocity,
                NormalizedDistanceToParkingZone,
                AlignmentToParkingZone,
                AlignmentToTrailer,
            };
    }

    protected override void EvaluateOutput(float[] output)
    {
        if(heuristic) return;

        truckPhysics.aceleration = output[0];
        truckPhysics.breaking = output[1];
        truckPhysics.steer = output[2];
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Map")) return;

        Stop();
        IsCollidedWithMap = true;
        onCollideWithMapEvt?.Invoke();
    }

    public override void Stop()
    {
        truckPhysics.Stop();
        trailerPhysics.Stop();
    }

    public override void Respawn()
    {
        truckPhysics.Stop();
        trailerPhysics.Stop();
        truckPhysics.Respawn();
        trailerPhysics.Respawn();
        IsCollidedWithMap = false;
    }
}