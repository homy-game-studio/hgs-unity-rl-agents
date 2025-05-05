using System;
using UnityEngine;

namespace HGS.RLAgents
{
    [Serializable]
    public class AcademyEnvironmentReplication
    {
        [SerializeField] GameObject environmentGo;
        [SerializeField] int numberOfInstances = 10;
        [SerializeField] float size = 30;

        public void Spawn(Transform container)
        {
            var startPosition = environmentGo.transform.position + Vector3.right * size;
            var startRotation = environmentGo.transform.rotation;

            for (int x = 0; x < numberOfInstances; x++)
            {
                var position = startPosition * 2 + new Vector3(x * size, startPosition.y);
                var go = GameObject.Instantiate(environmentGo, position, startRotation, container);
            }
        }
    }
}
