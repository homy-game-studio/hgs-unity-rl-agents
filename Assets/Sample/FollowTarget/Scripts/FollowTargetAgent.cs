using UnityEngine;

namespace HGS.RLAgents.FollowTargetSample
{
    public class FollowTargetAgent : Agent
    {
        [SerializeField] Transform target;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float maxDistance = 11f;
        [SerializeField] float distanceToReachTarget = 1f;

        Vector2 _dir = Vector2.zero;
        Vector2 _startPosition;
        float _initialDistance = 0;
        float _endDistance = 0;

        void Awake()
        {
            _startPosition = transform.position;
        }

        protected override void Restart()
        {
            transform.position = _startPosition;
        }

        protected override void ExecuteAction(int action)
        {
            switch (action)
            {
                case 0: _dir = Vector2.left; break;
                case 1: _dir = Vector2.up; break;
                case 2: _dir = Vector2.right; break;
                case 3: _dir = Vector2.down; break;
                case 4: _dir = Vector2.left + Vector2.up; break;
                case 5: _dir = Vector2.right + Vector2.up; break;
                case 6: _dir = Vector2.left + Vector2.down; break;
                case 7: _dir = Vector2.right + Vector2.down; break;
            }
        }

        protected override void StartDecision()
        {
            base.StartDecision();
            _initialDistance = Vector2.Distance(target.position, transform.position);
        }

        protected override void EndDecision()
        {
            base.EndDecision();

            _endDistance = Vector2.Distance(target.position, transform.position);
            if (_endDistance <= 1f)
            {
                FinishEpoch();
            }
        }

        protected override float GetReward()
        {
            var reward = 1f - _endDistance / 3f;
            if (_endDistance <= 1.5f) return reward * 2f;
            if (_endDistance > _initialDistance) return -1f;
            return reward;
        }

        protected override float[] GetState()
        {
            var direction = (transform.position - target.position).normalized;
            return new float[] { direction.x, direction.y };
        }

        protected override void Update()
        {
            base.Update();
            transform.Translate(_dir * Time.deltaTime * moveSpeed);
            transform.position = new Vector2(
                Mathf.Clamp(transform.position.x, -maxDistance, maxDistance),
                Mathf.Clamp(transform.position.y, -maxDistance, maxDistance)
            );
        }
    }
}