using HGS.RLAgents.Simulation;
using UnityEngine;

public class TruckParkingEnvironment : SimulationEnvironment
{
    [SerializeField] TruckAgent agent;

    float prevDistance;
    float prevAlignParking;
    float prevAlignTrailer;
    float prevVelocity;

    string agentHash;

    private void Awake()
    {
        agentHash = System.Guid.NewGuid().ToString();
        agent.onEvaluationEnd += EvaluateFitness;
        agent.onCollideWithMapEvt += OnCollideWithMap;
    }

    private void OnCollideWithMap()
    {
        // Penalidade base fixa
        float penalty = 3.0f;

        // Penalidade extra se bater em alta velocidade (estimula cautela)
        if (agent.FowardVelocity > 1.0f) penalty += 2.0f;

        agent.fitness -= penalty;
        FinishEpoch();
    }

    protected override void OnStartEpoch()
    {
        prevDistance = agent.StartNormalizedDistanceToParkingZone;
        prevAlignParking = agent.StartAlignmentToParkingZone;
        prevAlignTrailer = agent.StartAlignmentToTrailer;
        prevVelocity = 0f;
    }

    //public override void EvaluateFitness()
    //{
    //    float currentDistance = agent.NormalizedDistanceToParkingZone;
    //    float currentAlignParking = agent.AlignmentToParkingZone;
    //    float currentAlignTrailer = agent.AlignmentToTrailer;

    //    float deltaDistance = prevDistance - currentDistance;
    //    float deltaAlignParking = currentAlignParking - prevAlignParking;
    //    float deltaAlignTrailer = currentAlignTrailer - prevAlignTrailer;

    //    prevDistance = currentDistance;
    //    prevAlignParking = currentAlignParking;
    //    prevAlignTrailer = currentAlignTrailer;

    //    agent.fitness -= 0.001f;
    //    agent.fitness += 0.05f * deltaDistance;
    //    agent.fitness += 0.03f * deltaAlignParking;
    //    agent.fitness += 0.001f * deltaAlignTrailer;

    //    if(currentDistance < 0.01f && currentAlignParking > 0.98f && currentAlignTrailer > 0.95f)
    //    {
    //        agent.fitness += 2f;
    //        FinishEpoch();
    //    }
    //}

    //public override void EvaluateFitness()
    //{
    //    float currentDistance = agent.NormalizedDistanceToParkingZone;
    //    float currentAlignParking = agent.AlignmentToParkingZone;
    //    float currentAlignTrailer = agent.AlignmentToTrailer;
    //    float currentVelocity = agent.FowardVelocity;

    //    float deltaDistance = prevDistance - currentDistance;
    //    float deltaAlignParking = currentAlignParking - prevAlignParking;
    //    float deltaAlignTrailer = currentAlignTrailer - prevAlignTrailer;
    //    float deltaVelocity = currentVelocity - prevVelocity;

    //    prevDistance = currentDistance;
    //    prevAlignParking = currentAlignParking;
    //    prevAlignTrailer = currentAlignTrailer;

    //    // 1. Custo de vida (estimula rapidez)
    //    agent.fitness -= 0.005f;

    //    // 2. Progresso de distância (Peso aumentado)
    //    // Se delta for positivo, ele se aproximou.
    //    agent.fitness += deltaDistance * 8.0f;

    //    // 3. Alinhamento com a vaga (Importante para a manobra final)
    //    // Recompensar apenas se estiver perto da vaga
    //    //if (currentDistance < 10f)
    //    //{
    //        agent.fitness += deltaAlignParking * 2.0f;
    //    //}

    //    // 5. Sucesso (Prêmio dominante)
    //    if (currentDistance < 0.01f && currentAlignParking > 0.98f && currentAlignTrailer > 0.95f && currentVelocity > 0 && currentVelocity < 0.3f)
    //    {
    //        agent.fitness += 15f;
    //        FinishEpoch();
    //    }
    //}

    //public override void EvaluateFitness()
    //{
    //    float d = agent.NormalizedDistanceToParkingZone;
    //    float alignCar = agent.AlignmentToParkingZone;
    //    float alignTrailer = agent.AlignmentToTrailer;
    //    float v = agent.FowardVelocity;

    //    float dDist = prevDistance - d;
    //    float dAlignCar = alignCar - prevAlignParking;
    //    float dAlignTrailer = alignTrailer - prevAlignTrailer;

    //    prevDistance = d;
    //    prevAlignParking = alignCar;
    //    prevAlignTrailer = alignTrailer;

    //    // =========================
    //    // 1. Custo de vida (leve)
    //    // =========================
    //    agent.fitness -= 0.002f;

    //    // =========================
    //    // 2. FASE: APROXIMAÇÃO (longe)
    //    // =========================
    //    if (d > 0.5f)
    //    {
    //        // Aproximar é bom, mas não dominante
    //        agent.fitness += dDist * 2.0f;

    //        // Pequeno incentivo a alinhar já, mas fraco
    //        agent.fitness += dAlignCar * 0.5f;
    //    }

    //    // =========================
    //    // 3. FASE: SETUP (médio alcance)
    //    // =========================
    //    else if (d > 0.25f)
    //    {
    //        // Aproximação controlada
    //        agent.fitness += dDist * 1.5f;

    //        // Alinhamento começa a importar mais
    //        agent.fitness += dAlignCar * 1.5f;

    //        // Incentiva reduzir velocidade
    //        if (Mathf.Abs(v) > 0.6f)
    //            agent.fitness -= 0.01f;
    //    }

    //    // =========================
    //    // 4. FASE: MANOBRA (perto)
    //    // =========================
    //    else
    //    {
    //        // Aproximação leve (não dominante)
    //        agent.fitness += dDist * 1.0f;

    //        // Alinhamento crítico
    //        agent.fitness += dAlignCar * 2.5f;
    //        agent.fitness += dAlignTrailer * 1.5f;

    //        // Incentivar uso de ré (ESSENCIAL)
    //        if (v < 0)
    //            agent.fitness += 0.03f;

    //        // Permitir afastar para corrigir ângulo
    //        if (dDist < 0)
    //            agent.fitness += 0.015f;

    //        // Penalizar velocidade alta
    //        if (Mathf.Abs(v) > 0.4f)
    //            agent.fitness -= 0.05f;
    //    }

    //    // =========================
    //    // 5. Penalidade de colisão leve contínua (opcional)
    //    // =========================
    //    // (mantém seu OnCollideWithMap como está)

    //    // =========================
    //    // 6. SUCESSO
    //    // =========================
    //    if (
    //        d < 0.01f &&
    //        alignCar > 0.98f &&
    //        alignTrailer > 0.95f &&
    //        Mathf.Abs(v) < 0.2f
    //    )
    //    {
    //        agent.fitness += 20f;
    //        FinishEpoch();
    //    }
    //}

    public override void EvaluateFitness()
    {
        float d = agent.NormalizedDistanceToParkingZone;
        float alignCar = agent.AlignmentToParkingZone;
        float alignTrailer = agent.AlignmentToTrailer;
        float v = agent.FowardVelocity;

        float dDist = prevDistance - d;
        float dAlignCar = alignCar - prevAlignParking;
        float dAlignTrailer = alignTrailer - prevAlignTrailer;

        prevDistance = d;
        prevAlignParking = alignCar;
        prevAlignTrailer = alignTrailer;

        // Penalidade por exisir (estimula soluções mais rápidas)
        agent.fitness -= 0.005f;

        // punicao por fica parado
        if(agent.FowardVelocity < 0.5f)
        {
            agent.fitness -= 0.005f;
        }

        // Indo em direção da vaga é bom
        if (dDist > 0)
        {
            agent.fitness += 0.01f;

            if (alignCar > 0)
            {
                agent.fitness += 0.005f;
            }

        }
    }
}
