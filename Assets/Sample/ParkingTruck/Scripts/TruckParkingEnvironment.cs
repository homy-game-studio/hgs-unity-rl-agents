using HGS.RLAgents.Simulation;
using UnityEngine;

// Fitness is a pure function of the final state of the epoch (pose error, crash, parked),
// not a sum of per-step terms: the same final pose always scores the same, whatever the path.
public class TruckParkingEnvironment : SimulationEnvironment
{
    [SerializeField] TruckAgent agent;

    [Header("Pose error (fitness = -error)")]
    [Tooltip("Weight of the square root of the normalized distance between the trailer and the parking zone")]
    [SerializeField] float distanceWeight = 2f;
    [Tooltip("Weight of the trailer heading error relative to the parking zone (angle / 180: 0 = aligned, 1 = opposite)")]
    [SerializeField] float headingWeight = 2f;
    [Tooltip("Weight of the truck/trailer articulation error (0 = straight, 1 = opposite)")]
    [SerializeField] float articulationWeight = 0.25f;

    [Header("Proximity (graded reward, no crash)")]
    [Tooltip("Reward for ending the epoch at the zone without crashing, scaled by how close it is and how slow")]
    [SerializeField] float proximityWeight = 1.5f;
    [Tooltip("Distance in meters at which the proximity reward starts (0 reward at this distance or farther)")]
    [SerializeField] float proximityRadiusMeters = 12f;

    [Header("Parking quality (refinement)")]
    [Tooltip("Weight of exp(-distance / scale) * heading * articulation * (1 - speed). Unlike a linear distance it keeps growing steeply over the last meters, so parking closer and straighter always pays.")]
    [SerializeField] float qualityWeight = 2f;
    [Tooltip("Meters: the distance at which the closeness part has dropped to 37%")]
    [SerializeField] float qualityDistanceScale = 0.8f;
    [Tooltip("Heading and articulation errors (degrees) at which their part of the quality reaches zero")]
    [SerializeField] float qualityAngleDegrees = 30f;

    [Header("Partial success (stopped, aligned, no crash)")]
    [SerializeField] float nearDistanceMeters = 2f;
    [SerializeField] float nearReward = 0.5f;
    [SerializeField] float closeDistanceMeters = 1f;
    [Tooltip("Added on top of the near reward")]
    [SerializeField] float closeReward = 0.5f;
    [SerializeField] float partialAlignParking = 0.95f;
    [SerializeField] float partialAlignTrailer = 0.9f;

    [Header("Collision")]
    [Tooltip("Flat penalty for any crash. Kept light on purpose: the truck must learn to park first, and only then to not hit the wall.")]
    [SerializeField] float collisionPenalty = 0.1f;
    [Tooltip("How much the impact speed counts: 0 = every crash is the same light penalty and keeps its rewards, 1 = a hard hit is penalized and loses its rewards. Raise it once the truck parks, to teach it not to crash.")]
    [SerializeField, Range(0f, 1f)] float impactSpeedInfluence = 0f;
    [Tooltip("Extra penalty for crashing at the agent's max observed speed (scales linearly with the impact speed)")]
    [SerializeField] float impactSpeedPenalty = 1f;

    [Header("Final speed")]
    [Tooltip("Penalty for ending the epoch at the max observed speed (crash or timeout): teaches braking")]
    [SerializeField] float finalSpeedWeight = 0.5f;

    [Header("Success")]
    [SerializeField] float successReward = 2f;
    [Tooltip("Extra reward for parking early: this times the fraction of the epoch left")]
    [SerializeField] float successTimeBonus = 1f;
    [Tooltip("Meters between the trailer's center of mass and the parking zone (independent of the agent's max observed distance)")]
    [SerializeField] float successDistanceMeters = 0.6f;
    [SerializeField] float successAlignParking = 0.98f;
    [SerializeField] float successAlignTrailer = 0.95f;
    [SerializeField] float successMaxSpeed = 0.3f;

    [Header("Early stop")]
    [Tooltip("Ends the epoch after this many seconds without moving (0 = never). Speeds up training a lot when many agents just stand still.")]
    [SerializeField] float stillTimeout = 3f;
    [SerializeField] float stillSpeed = 0.05f;
    [Tooltip("Near the zone the truck may need to stop and adjust: within this many meters the longer timeout below applies")]
    [SerializeField] float nearZoneMeters = 4f;
    [SerializeField] float nearZoneStillTimeout = 10f;

    float stillTime;
    float minVelocity;
    bool crashed;
    float impactSpeed;
    bool parked;
    float lastSpeed;

    // FinishEpoch stops the agent (zero velocity) before evaluating, so keep the last real speed
    protected override void FixedUpdate()
    {
        if (!isFinished)
        {
            lastSpeed = Mathf.Abs(agent.FowardVelocity);
            minVelocity = Mathf.Min(minVelocity, agent.FowardVelocity);

            // The fitness only depends on the final pose, so a truck that stopped scores the same
            // now or at the end of the epoch: do not spend the rest of the epoch on it
            stillTime = lastSpeed < stillSpeed ? stillTime + Time.fixedDeltaTime : 0f;
            var timeout = DistanceToZoneMeters < nearZoneMeters ? nearZoneStillTimeout : stillTimeout;
            if (stillTimeout > 0f && stillTime >= timeout)
            {
                FinishEpoch();
                return;
            }
        }

        base.FixedUpdate();
    }

    private void Awake()
    {
        agent.onEvaluationEnd += CheckParked;
        agent.onCollideWithMapEvt += OnCollideWithMap;
    }

    private void OnDestroy()
    {
        agent.onEvaluationEnd -= CheckParked;
        agent.onCollideWithMapEvt -= OnCollideWithMap;
    }

    private void OnCollideWithMap(float speed)
    {
        // Contatos extras no mesmo passo de fisica nao contam duas vezes
        if (isFinished) return;

        crashed = true;
        impactSpeed = speed;
        FinishEpoch();
    }

    // Must be checked while the truck is still moving: FinishEpoch stops the agent
    private void CheckParked()
    {
        if (isFinished) return;
        if (!IsParked()) return;

        parked = true;
        FinishEpoch();
    }

    float DistanceToZoneMeters => agent.NormalizedDistanceToParkingZone * agent.MaxDistanceToParkingZone;

    bool IsParked()
    {
        return DistanceToZoneMeters < successDistanceMeters &&
               agent.AlignmentToParkingZone > successAlignParking &&
               agent.AlignmentToTrailer > successAlignTrailer &&
               Mathf.Abs(agent.FowardVelocity) < successMaxSpeed;
    }

    // The agents respawn at their default pose before this runs: every epoch starts from the same pose
    protected override void OnStartEpoch()
    {
        crashed = false;
        parked = false;
        impactSpeed = 0f;
        lastSpeed = 0f;
        stillTime = 0f;
        minVelocity = 0f;
    }

    // Called once, by FinishEpoch
    public override void EvaluateFitness()
    {
        // Square root: the last meters weigh more than the first ones, so closing the final
        // gap is worth the risk (a linear error makes it almost free to stop short)
        float distanceError = Mathf.Sqrt(Mathf.Clamp01(agent.NormalizedDistanceToParkingZone));
        // Linear in the angle: (1 - cos) is flat near 0, so the last degrees of misalignment would be almost free
        float headingError = Mathf.Acos(Mathf.Clamp(agent.AlignmentToParkingZone, -1f, 1f)) / Mathf.PI;
        float articulationError = (1f - agent.AlignmentToTrailer) * 0.5f;

        float fitness = -(distanceError * distanceWeight +
                          headingError * headingWeight +
                          articulationError * articulationWeight);

        // Speed when the epoch ended: the impact speed on a crash (scaled by impactSpeedInfluence), otherwise the last real speed
        float endSpeed = crashed ? Mathf.Abs(impactSpeed) * impactSpeedInfluence : lastSpeed;
        float endSpeed01 = Mathf.Clamp01(endSpeed / agent.MaxObservedSpeed);

        // Arriving fast is bad whether or not it ends in a crash
        fitness -= finalSpeedWeight * endSpeed01;

        // A crash is paid by its speed: a gentle touch of the wall costs almost nothing, a hard hit costs a lot
        if (crashed)
        {
            fitness -= collisionPenalty + impactSpeedPenalty * endSpeed01;
        }

        // The rewards below apply to a crash too, gated by the speed at the end (the impact speed on a
        // crash). Skipping them on a crash made touching the wall at the end of a good parking cost
        // as much as crashing far from the zone, which discourages the final push.

        // Graded reward for stopping close to the zone: the last meters keep paying off,
        // so parking is a slope instead of a cliff. Moving fast at the end cancels it.
        float distanceMeters = DistanceToZoneMeters;
        float closeness = 1f - Mathf.Clamp01(distanceMeters / proximityRadiusMeters);
        fitness += proximityWeight * closeness * (1f - endSpeed01);

        // Refinement: exponential in the distance, so 1.6 m and 0.3 m are far apart in score,
        // and gated by heading, articulation and speed so it only pays for a clean stop
        float headingDegrees = Mathf.Abs(agent.HeadingErrorToParkingZone);
        float articulationDegrees = Mathf.Abs(agent.AngleToTrailer);
        float quality = Mathf.Exp(-distanceMeters / qualityDistanceScale)
                        * (1f - Mathf.Clamp01(headingDegrees / qualityAngleDegrees))
                        * (1f - Mathf.Clamp01(articulationDegrees / qualityAngleDegrees))
                        * (1f - endSpeed01);
        fitness += qualityWeight * quality;

        // Steps towards the success: stopped and aligned within 2 m, then within 1 m
        bool stoppedAligned = endSpeed < successMaxSpeed &&
                              agent.AlignmentToParkingZone > partialAlignParking &&
                              agent.AlignmentToTrailer > partialAlignTrailer;
        if (stoppedAligned && distanceMeters < nearDistanceMeters) fitness += nearReward;
        if (stoppedAligned && distanceMeters < closeDistanceMeters) fitness += closeReward;

        if (parked)
        {
            float timeLeft = maxEpochDuration > 0f ? Mathf.Clamp01(1f - elapsedTime / maxEpochDuration) : 0f;
            fitness += successReward + successTimeBonus * timeLeft;
        }

        agent.fitness = fitness;

        if (logStats) RecordStats(fitness);

        if (logNewBest && fitness > _bestLogged)
        {
            _bestLogged = fitness;
            Debug.Log(
                $"[TruckParking] new best {fitness:F3} | distance {agent.NormalizedDistanceToParkingZone * agent.MaxDistanceToParkingZone:F1} m | " +
                $"alignParking {agent.AlignmentToParkingZone:F2} | alignTrailer {agent.AlignmentToTrailer:F2} | " +
                $"end speed {endSpeed:F2} m/s | crashed {crashed} | parked {parked} | time {elapsedTime:F1}s");
        }
    }

    [Header("Debug")]
    [Tooltip("Logs the pose, speed and outcome every time an epoch beats the best fitness seen so far")]
    [SerializeField] bool logNewBest = true;

    [Tooltip("Every 100 epochs, logs how many crashed, went away from the zone, stopped, or parked")]
    [SerializeField] bool logStats = true;

    static float _bestLogged = float.NegativeInfinity;
    static int _statEpochs, _statCrashed, _statWentAway, _statParked, _statStopped, _statReversed, _statAlignedFar;
    static float _statFitness;

    void RecordStats(float fitness)
    {
        float startMeters = agent.StartNormalizedDistanceToParkingZone * agent.MaxDistanceToParkingZone;
        float endMeters = agent.NormalizedDistanceToParkingZone * agent.MaxDistanceToParkingZone;
        bool wentAway = endMeters > startMeters + 1f;

        _statEpochs++;
        _statFitness += fitness;
        if (crashed) _statCrashed++;
        if (wentAway) _statWentAway++;
        if (parked) _statParked++;
        if (!crashed && !parked && lastSpeed < 0.3f) _statStopped++;
        // Drove backwards at some point (more than noise), and ended aligned with the zone but outside the proximity radius
        if (minVelocity < -0.5f) _statReversed++;
        if (!crashed && agent.AlignmentToParkingZone > 0.95f && endMeters > proximityRadiusMeters) _statAlignedFar++;

        if (_statEpochs < 100) return;

        Debug.Log(
            $"[TruckParking] last 100 epochs | mean fitness {_statFitness / _statEpochs:F2} | crashed {_statCrashed}% | " +
            $"ended farther than the start {_statWentAway}% | parked {_statParked}% | timed out standing still {_statStopped}% | " +
            $"drove in reverse {_statReversed}% | ended aligned but beyond {proximityRadiusMeters:F0} m {_statAlignedFar}%");

        _statEpochs = _statCrashed = _statWentAway = _statParked = _statStopped = _statReversed = _statAlignedFar = 0;
        _statFitness = 0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetBestLogged()
    {
        _bestLogged = float.NegativeInfinity;
        _statEpochs = _statCrashed = _statWentAway = _statParked = _statStopped = _statReversed = _statAlignedFar = 0;
        _statFitness = 0f;
    }
}
