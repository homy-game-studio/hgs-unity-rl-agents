using HGS.RLAgents;
using UnityEngine;

public class TruckParkPolicy : Policy
{
    [SerializeField] Environment env;
    [SerializeField] Transform target;
    [SerializeField] Transform trailer;
    [SerializeField] TruckAgent agent;
    [SerializeField] Transform parkingZoneRef;

    private void Awake()
    {
        agent.onEvaluationEnd += OnAgentEvaluationEnd;
    }

    public override void StartEpoch()
    {
        base.StartEpoch();
    }

    private void OnAgentEvaluationEnd()
    {
        float distance = Vector3.Distance(trailer.position, target.position);

        agent.reward -= 0.001f;
        agent.reward -= distance * 0.002f;
        agent.reward -= agent.AngleBetweenCabinAndTrailer * 0.01f;
        agent.reward += agent.TrailerAlignmentToParking * 0.01f;
        agent.reward += agent.ParkingPercent;
    }

    public override void TransitionIn()
    {
        base.TransitionIn();
        target.position = parkingZoneRef.position;
        target.rotation = parkingZoneRef.rotation;
    }

    private void FixedUpdate()
    {
        if (agent.active && agent.IsCollidedWithMap)
        {
            agent.reward -= 1f;
            env.CompleteEpoch();
        }
    }
}
