using UnityEngine;

namespace HGS.RLAgents.FollowTargetSample
{
    public class FollowTargetEnvironment : Environment
    {
        [SerializeField] FollowTargetAgent followTargetAgent;
        [SerializeField] Transform target;
        [SerializeField] int maxEvaluations = 200;
        [SerializeField] float randomAngle = 30;
        [SerializeField] float radius = 5;
        [SerializeField] int randomizeRate = 20;

        protected override void Awake()
        {
            base.Awake();

            followTargetAgent.onReachTarget += CompleteEpoch;
        }

        protected override void OnStartEpoch()
        {
            followTargetAgent.active = true;
            followTargetAgent.Respawn();

            if (Epoch % randomizeRate == 0)
            {
                var angle = Time.time * randomAngle;
                var x = Mathf.Sin(Mathf.Deg2Rad * angle);
                var y = Mathf.Cos(Mathf.Deg2Rad * angle);
                target.position = new Vector2(x, y) * radius;
            }
        }

        protected override void OnFinishEpoch()
        {
            followTargetAgent.active = false;
        }
    }
}