using HGS.RLAgents.NeuralNetworks;
using Unity.Burst;
using UnityEngine;

namespace HGS.RLAgents.DriverSample
{
    [BurstCompile]
    public struct DriverLogic
    {
        public float speed;
        public float steer;

        public OBB2D[] trackParts;
        public OBB2D carPolygon;

        public Vector2 startPosition;
        public float startRotation;

        public float maxRayDistance;
        public float maxSpeed;
        public float maxSteerAngle;

        // Neural Networks
        public float decisionInterval;
        public NeuralNetwork nn;

        // Inputs
        public float rayA;
        public float rayB;
        public float rayC;

        public Vector2 DirectionRayA => carPolygon.Right;
        public Vector2 DirectionRayB => (carPolygon.Right + carPolygon.Up).normalized;
        public Vector2 DirectionRayC => (carPolygon.Right - carPolygon.Up).normalized;

        void Move(float deltaTime)
        {
            carPolygon.rotation += steer * deltaTime;
            carPolygon.center += carPolygon.Right * speed * deltaTime;
        }

        void Sense()
        {
            Simulation.Physics.Raycast(carPolygon.center, DirectionRayA, trackParts, out rayA, maxRayDistance);
            Simulation.Physics.Raycast(carPolygon.center, DirectionRayB, trackParts, out rayB, maxRayDistance);
            Simulation.Physics.Raycast(carPolygon.center, DirectionRayC, trackParts, out rayC, maxRayDistance);
        }

        void CheckCollision()
        {
            if (speed == 0 || steer == 0) return;
            if (Simulation.Physics.CheckCollision(carPolygon, trackParts))
            {
                steer = 0;
                speed = 0;
            }
        }

        public void Tick(float deltaTime)
        {
            Sense();
            Move(deltaTime);
            CheckCollision();
        }

        public void DrawGizmos()
        {
            carPolygon.DrawGizmos();
            Debug.DrawRay(carPolygon.center, carPolygon.Right * 2f, Color.red);
            Debug.DrawRay(carPolygon.center, carPolygon.Up * 2f, Color.green);
            Debug.DrawRay(carPolygon.center, DirectionRayA * rayA, Color.yellow);
            Debug.DrawRay(carPolygon.center, DirectionRayB * rayB, Color.yellow);
            Debug.DrawRay(carPolygon.center, DirectionRayC * rayC, Color.yellow);
        }
    }
}