using System;
using System.Linq;
using HGS.RLAgents.Sensors;
using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerAgent : Agent
    {
        [Header("Pickup")]
        [SerializeField] LayerMask pickupItemLayerMask;
        [SerializeField] Transform holdContainer;
        [Header("Styling")]
        [SerializeField] SpriteRenderer spriteRenderer;
        [Header("Sensors")]
        [SerializeField] Transform checkpoint;
        [SerializeField] RaySensor raySensor;
        [Header("Control")]
        [SerializeField] Rigidbody2D myRigidbody2D;
        [SerializeField] float maxSpeed = 5f;
        [SerializeField] float maxSteeringSpeed = 45f;

        Vector2 _startPosition;
        Vector3 _startEulerAngles;

        // Actions
        public float Speed { get; private set; } = 0;
        public bool IsPressingHold { get; private set; } = false;
        // Sensors
        public float IdleTime { get; private set; } = 0;
        public float MovingTime { get; private set; } = 0;
        public int CollectedCrates { get; private set; } = 0;
        public float Steering { get; private set; } = 0;
        public bool IsCollidedWithMap { get; private set; } = false;
        public bool IsPickedCrate { get; private set; } = false;
        public bool IsCollidedWithCrate { get; private set; } = false;
        public float MinDistanceToCrate { get; private set; } = 1f;
        public float MinDistanceToCheckpoint { get; private set; } = 1f;
        public bool IsHoldingCrate => _holdItem != null;

        public Action onCollideWithMapEvt;
        public Action onCollideWithCrateEvt;
        public Action onPickCrateEvt;
        public Action onCollectCrateEvt;

        private Transform _holdItem;

        protected override void Awake()
        {
            base.Awake();
            _startPosition = transform.position;
            _startEulerAngles = transform.eulerAngles;
            spriteRenderer.color = UnityEngine.Random.ColorHSV();
        }

        protected override float[] GetInput()
        {
            var sensorInput = raySensor.Infos;

            return new float[] {
                sensorInput[0].distance,
                sensorInput[0].tagIndex,
                sensorInput[1].distance,
                sensorInput[1].tagIndex,
                sensorInput[2].distance,
                sensorInput[2].tagIndex,
                sensorInput[3].distance,
                sensorInput[3].tagIndex,
                sensorInput[4].distance,
                sensorInput[4].tagIndex,
                sensorInput[5].distance,
                sensorInput[5].tagIndex,
                IsHoldingCrate ? 1f : -1f
            };
        }

        protected override void EvaluateOutput(float[] output)
        {
            Speed = Mathf.Clamp(output[0] * maxSpeed, 0, maxSpeed);
            Steering = output[1] * maxSteeringSpeed;
            IsPressingHold = output[2] > 0.5f;
        }

        private void Pick()
        {
            if (IsHoldingCrate) return;

            var pickupCollider = Physics2D.OverlapCircle(holdContainer.position, 0.5f, pickupItemLayerMask);

            if (pickupCollider != null)
            {
                IsPickedCrate = true;
                _holdItem = pickupCollider.transform;
                pickupCollider.enabled = false;
                _holdItem.SetParent(holdContainer);
                _holdItem.localPosition = Vector3.zero;
                _holdItem.localRotation = Quaternion.identity;
                onPickCrateEvt?.Invoke();
            }
        }

        public void Collect()
        {
            if (!IsHoldingCrate) return;

            _holdItem.gameObject.SetActive(false);

            Drop();

            CollectedCrates++;
            onCollectCrateEvt?.Invoke();
        }

        private void Drop()
        {
            if (!IsHoldingCrate) return;

            var collider = _holdItem.GetComponent<Collider2D>();
            collider.enabled = true;

            _holdItem.SetParent(null);
            _holdItem = null;
        }

        protected override void Update()
        {
            base.Update();

            if (IsPressingHold && !IsHoldingCrate)
            {
                Pick();
            }

            if (!IsPressingHold && IsHoldingCrate)
            {
                if (Vector2.Distance(_holdItem.transform.position, checkpoint.position) <= 0.8f)
                {
                    Collect();
                }
                else
                {
                    Drop();
                }
            }

            var sensorInput = raySensor.Infos;

            var bestCrate = sensorInput
                .Where(ray => ray.tag == "Pickable")
                .OrderBy(ray => ray.distance)
                .FirstOrDefault();

            if (bestCrate.distance > 0 && MinDistanceToCrate > bestCrate.distance)
            {
                MinDistanceToCrate = bestCrate.distance;
            }

            if (IsHoldingCrate)
            {
                var distanceToCheckpoint = Vector2.Distance(transform.position.normalized, checkpoint.position.normalized);
                MinDistanceToCheckpoint = Mathf.Min(MinDistanceToCheckpoint, distanceToCheckpoint);
            }
        }

        protected void FixedUpdate()
        {
            if (!active) return;

            myRigidbody2D.linearVelocity = transform.right * Speed;
            myRigidbody2D.MoveRotation(myRigidbody2D.rotation + Steering * Time.fixedDeltaTime);

            if (myRigidbody2D.linearVelocity.magnitude <= 0.1f)
            {
                IdleTime += Time.fixedDeltaTime;
            }
            else
            {
                MovingTime += Time.fixedDeltaTime;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Map"))
            {
                IsCollidedWithMap = true;
                onCollideWithMapEvt?.Invoke();
            }

            if (collision.gameObject.CompareTag("Pickable"))
            {
                IsCollidedWithCrate = true;
                onCollideWithCrateEvt?.Invoke();
            }
        }

        public override void Stop()
        {
            myRigidbody2D.linearVelocity = Vector2.zero;
            myRigidbody2D.rotation = 0;
            Speed = 0;
            Steering = 0;
            IsPressingHold = false;
        }

        public override void Respawn()
        {
            Stop();
            Drop();

            transform.position = _startPosition;
            transform.eulerAngles = _startEulerAngles;

            // Sensors
            IdleTime = 0;
            MovingTime = 0;
            CollectedCrates = 0;
            IsCollidedWithMap = false;
            IsCollidedWithCrate = false;
            IsPickedCrate = false;
            MinDistanceToCrate = 1f;
            MinDistanceToCheckpoint = 10f;
        }
    }
}