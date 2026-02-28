using HGS.RLAgents;
using HGS.RLAgents.Simulation;
using UnityEngine;

public class TruckParkPolicy : Policy
{
    [SerializeField] SimulationEnvironment env;
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

        agent.fitness -= 0.001f;
        agent.fitness -= distance * 0.002f;
        agent.fitness -= agent.AngleBetweenCabinAndTrailer * 0.01f;
        agent.fitness += agent.TrailerAlignmentToParking * 0.01f;
        agent.fitness += agent.ParkingPercent;
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
            agent.fitness -= 1f;
            env.FinishEpoch();
        }
    }
}
