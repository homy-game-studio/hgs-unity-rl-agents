using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    public class DriverSimulation : MonoBehaviour
    {
        [Header("Game Scene")]
        [SerializeField] Collider2D[] trackColliders;
        [SerializeField] BoxCollider2D carCollider;

        [Header("Simulation Scene")]
        [SerializeField] float simulationInterval = 0.02f;
        [SerializeField] int populationSize = 100;
        [SerializeField] float maxSpeed = 5f;
        [SerializeField] float maxSteerAngle = 45f;
        [SerializeField] float maxRayDistance = 5f;

        OBB2D[] _trackParts;
        DriverLogic[] drivers;

        private float _timer = float.PositiveInfinity;

        private void Awake()
        {
            GenerateTrack();
            InstantiateDrivers();
        }

        void InstantiateDrivers()
        {
            drivers = new DriverLogic[populationSize];
            for (int i = 0; i < populationSize; i++)
            {
                drivers[i] = InstantiateDriver();
            }
        }

        DriverLogic InstantiateDriver()
        {
            Vector2 center = carCollider.transform.position;
            float rotation = carCollider.transform.rotation.eulerAngles.z;

            return new DriverLogic
            {
                speed = Random.Range(-maxSpeed, maxSpeed),
                steer = Random.Range(-maxSteerAngle, maxSteerAngle),
                trackParts = _trackParts,
                startPosition = center,
                startRotation = rotation,
                maxSpeed = maxSpeed,
                maxSteerAngle = maxSteerAngle,
                maxRayDistance = maxRayDistance,
                carPolygon = new OBB2D
                {
                    center = center,
                    size = carCollider.size,
                    rotation = rotation
                }
            };
        }

        void GenerateTrack()
        {
            _trackParts = new OBB2D[trackColliders.Length];

            for (int i = 0; i < trackColliders.Length; i++)
            {
                _trackParts[i] = new OBB2D
                {
                    center = trackColliders[i].transform.position,
                    size = trackColliders[i].transform.localScale,
                    rotation = trackColliders[i].transform.rotation.eulerAngles.z
                };
            }
        }

        void StepSimulation(float dt)
        {
            for (int i = 0; i < drivers.Length; i++)
            {
                drivers[i].Tick(dt);
            }
        }

        private void Update()
        {
            if (_timer > simulationInterval)
            {
                StepSimulation(simulationInterval);
                _timer = 0f;
            }

            _timer += Time.deltaTime;
        }

        private void OnDrawGizmos()
        {
            for (var i = 0; i < 10; i++)
            {
                drivers?[i].DrawGizmos();
            }

            for (var i = 0; i < _trackParts.Length; i++)
            {
                _trackParts[i].DrawGizmos();
            }
        }
    }
}