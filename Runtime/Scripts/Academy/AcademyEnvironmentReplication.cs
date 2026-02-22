using System;
using UnityEngine;

namespace HGS.RLAgents
{
    [Serializable]
    public class AcademyEnvironmentReplication
    {
        [SerializeField] GameObject environmentGo;
        [SerializeField] int numberOfRenderers = 4;
        [SerializeField] int numberOfInstances = 10;
        [SerializeField] float size = 30;

        public void Spawn(Transform container)
        {
            var basePosition = environmentGo.transform.position;
            var rotation = environmentGo.transform.rotation;

            for (int x = 0; x < numberOfInstances; x++)
            {
                var position = basePosition + Vector3.right * (x * size);
                var instance = GameObject.Instantiate(environmentGo, position, rotation, container);
                var env = instance.GetComponent<Environment>();
                env.ToggleRenderer(x < numberOfRenderers);
            }
        }
    }
}
