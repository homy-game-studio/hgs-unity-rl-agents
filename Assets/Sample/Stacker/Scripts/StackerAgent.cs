using System;
using System.Collections.Generic;
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
        [SerializeField] RaySensor raySensor;
        [Header("Control")]
        [SerializeField] Rigidbody2D myRigidbody2D;
        [SerializeField] float maxSpeed = 5f;
        [SerializeField] float maxSteeringSpeed = 45f;
        [Header("Env")]
        [SerializeField] Transform checkpoint;
        [SerializeField] List<Transform> _crates;

        Vector2 _startPosition;
        Vector3 _startEulerAngles;

        // Actions
        public float Speed { get; private set; } = 0;
        public bool IsPressingHold { get; private set; } = false;
        // Sensors
        public float IdleTime { get; private set; } = 0;
        public float TimeWithoutCrate { get; private set; } = 0;
        public float MinDistanceToCrate { get; private set; } = 0;
        public float AvgTimeToPickCrate { get; private set; } = 0;
        public float MinDistanceToCheckpoint { get; private set; } = 0;
        public float TimeToDeliveryCrate { get; private set; } = 0;
        public int PickedCrateCount { get; private set; } = 0;
        public int DeliveredCrates { get; private set; } = 0;
        public float Steering { get; private set; } = 0;
        public bool IsCollidedWithMap { get; private set; } = false;
        public bool IsPickedCrate { get; private set; } = false;
        public int CollisionCount { get; private set; } = 0;
        public bool IsHoldingCrate => _holdItem != null;

        public Action onCollideWithMapEvt;
        public Action onCollideWithCrateEvt;
        public Action<Transform> onPickCrateEvt;
        public Action onDropCrateEvt;
        public Action onDeliveryCrateEvt;

        private Transform _holdItem;
        private Transform _crate;

        protected override void Awake()
        {
            base.Awake();
            _startPosition = transform.position;
            _startEulerAngles = transform.eulerAngles;
            spriteRenderer.color = UnityEngine.Random.ColorHSV();
        }

        protected override float[] GetInput()
        {

            return new float[] {
                IsHoldingCrate ? 1f : 0f,

                raySensor.Infos[0].distance,
                raySensor.Infos[0].tags[0],
                raySensor.Infos[0].tags[1],
                raySensor.Infos[0].tags[2],

                raySensor.Infos[1].distance,
                raySensor.Infos[1].tags[0],
                raySensor.Infos[1].tags[1],
                raySensor.Infos[1].tags[2],

                raySensor.Infos[2].distance,
                raySensor.Infos[2].tags[0],
                raySensor.Infos[2].tags[1],
                raySensor.Infos[2].tags[2],

                raySensor.Infos[3].distance,
                raySensor.Infos[3].tags[0],
                raySensor.Infos[3].tags[1],
                raySensor.Infos[3].tags[2],

                raySensor.Infos[4].distance,
                raySensor.Infos[4].tags[0],
                raySensor.Infos[4].tags[1],
                raySensor.Infos[4].tags[2],

                raySensor.Infos[5].distance,
                raySensor.Infos[5].tags[0],
                raySensor.Infos[5].tags[1],
                raySensor.Infos[5].tags[2],
            };
        }

        protected override void EvaluateOutput(float[] output)
        {
            Speed = Mathf.Clamp(output[0] * maxSpeed, 0, maxSpeed);
            Steering = (output[1] - 1f) * maxSteeringSpeed;
            IsPressingHold = output[2] > 0.5f;
        }

        private void FindNearestCrate()
        {
            _crate = _crates
                .Where(item => item.gameObject.activeSelf)
                .OrderBy(item => Vector2.Distance(item.position, transform.position)).FirstOrDefault();
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
                PickedCrateCount++;
                AvgTimeToPickCrate = TimeWithoutCrate / PickedCrateCount;
                onPickCrateEvt?.Invoke(_holdItem);
            }
        }

        public void Delivery()
        {
            if (!IsHoldingCrate) return;

            _holdItem.gameObject.SetActive(false);

            Drop(false);


            DeliveredCrates++;
            onDeliveryCrateEvt?.Invoke();
        }

        public void Drop(bool raiseEvent = true)
        {
            if (!IsHoldingCrate) return;

            var collider = _holdItem.GetComponent<Collider2D>();
            collider.enabled = true;

            _holdItem.SetParent(null);
            _holdItem = null;
            FindNearestCrate();

            if (raiseEvent) onDropCrateEvt?.Invoke();
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
                if (Vector2.Distance(_holdItem.transform.position, checkpoint.position) <= 1.2f)
                {
                    Delivery();
                }
                else
                {
                    Drop();
                }
            }

            if (IsHoldingCrate)
            {
                var checkpointDistance = Vector2.Distance(checkpoint.position, transform.position);
                if (checkpointDistance < MinDistanceToCheckpoint)
                {
                    MinDistanceToCheckpoint = checkpointDistance;
                }
            }

            if (!IsHoldingCrate && _crate != null)
            {
                var crateDistance = Vector2.Distance(_crate.position, transform.position);
                if (crateDistance < MinDistanceToCrate)
                {
                    MinDistanceToCrate = crateDistance;
                }
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

            if (!IsHoldingCrate)
            {
                TimeWithoutCrate += Time.fixedDeltaTime;
            }

            TimeToDeliveryCrate += Time.fixedDeltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CollisionCount++;

            if (collision.gameObject.CompareTag("Map"))
            {
                IsCollidedWithMap = true;
                onCollideWithMapEvt?.Invoke();
            }

            if (collision.gameObject.CompareTag("Pickable"))
            {
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
            FindNearestCrate();
            Stop();
            Drop();

            transform.position = _startPosition;
            transform.eulerAngles = _startEulerAngles;

            // Sensors
            PickedCrateCount = 0;
            AvgTimeToPickCrate = 0f;
            MinDistanceToCheckpoint = 10f;
            MinDistanceToCrate = 10f;
            TimeWithoutCrate = 0;
            TimeToDeliveryCrate = 0;
            IdleTime = 0;
            DeliveredCrates = 0;
            CollisionCount = 0;
            IsCollidedWithMap = false;
            IsPickedCrate = false;
        }
    }
}