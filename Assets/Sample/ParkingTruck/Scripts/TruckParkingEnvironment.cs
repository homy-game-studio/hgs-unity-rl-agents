using HGS.RLAgents.Simulation;
using UnityEngine;

public class TruckParkingEnvironment : SimulationEnvironment
{
    [SerializeField] TruckAgent agent;

    float prevDistance;
    float prevAlignParking;
    float prevAlignTrailer;

    string agentHash;

    private void Awake()
    {
        agentHash = System.Guid.NewGuid().ToString();
        agent.onEvaluationEnd += EvaluateFitness;
        agent.onCollideWithMapEvt += OnCollideWithMap;
    }

    private void OnCollideWithMap()
    {
        agent.fitness -= 10;
        FinishEpoch();
    }

    protected override void OnStartEpoch()
    {
        prevDistance = agent.StartNormalizedDistanceToParkingZone;
        prevAlignParking = agent.StartAlignmentToParkingZone;
        prevAlignTrailer = agent.StartAlignmentToTrailer;
    }

    public override void EvaluateFitness()
    {
        float currentDistance = agent.NormalizedDistanceToParkingZone;
        float currentAlignParking = agent.AlignmentToParkingZone;
        float currentAlignTrailer = agent.AlignmentToTrailer;

        float deltaDistance = prevDistance - currentDistance;
        float deltaAlignParking = currentAlignParking - prevAlignParking;
        float deltaAlignTrailer = currentAlignTrailer - prevAlignTrailer;

        prevDistance = currentDistance;
        prevAlignParking = currentAlignParking;
        prevAlignTrailer = currentAlignTrailer;

        agent.fitness -= 0.001f;
        agent.fitness += 0.05f * deltaDistance;
        agent.fitness += 0.03f * deltaAlignParking;
        agent.fitness += 0.03f * deltaAlignTrailer;
    }
}
