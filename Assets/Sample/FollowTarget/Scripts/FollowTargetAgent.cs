using HGS.RLAgents.Agents;
using UnityEngine;

namespace HGS.RLAgents.FollowTargetSample
{
    public class FollowTargetAgent : Agent
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float maxDistance = 11f;

        Transform _target;
        Vector2 _dir = Vector2.zero;
        Vector2 _startPosition;

        void Awake()
        {
            _target = GameObject.Find("Target").transform;
            _startPosition = transform.position;
            spriteRenderer.color = Random.ColorHSV();
        }

        protected override float[] GetInput()
        {
            return new float[] {
                transform.position.x,
                transform.position.y,
                _target.position.x,
                _target.position.y
            };
        }

        protected override void ProcessOutput(float[] output)
        {
            reward -= 0.015f;
            if (Vector2.Distance(transform.position, _target.position) < 0.5f)
            {
                reward += 1f;
                active = false;
                return;
            }

            _dir.x = Mathf.Clamp(output[0], -1f, 1f);
            _dir.y = Mathf.Clamp(output[1], -1f, 1f);
        }

        protected override void Update()
        {
            base.Update();
            if (!active) return;
            transform.Translate(_dir.normalized * Time.deltaTime * moveSpeed);
            transform.position = new Vector2(
                Mathf.Clamp(transform.position.x, -maxDistance, maxDistance),
                Mathf.Clamp(transform.position.y, -maxDistance, maxDistance)
            );
        }

        public override void Restart()
        {
            base.Restart();
            transform.position = _startPosition;
            _dir = Vector2.zero;
        }
    }
}