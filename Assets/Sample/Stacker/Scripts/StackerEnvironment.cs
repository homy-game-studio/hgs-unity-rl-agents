using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerEnvironment : Environment
    {
        public StackerAgent agent;
        public List<Transform> crates;

        List<Pose> _initialCratePoses;
        protected override void Awake()
        {
            base.Awake();

            _initialCratePoses = crates
                .Select(crate => new Pose(crate.position, crate.rotation))
                .ToList();
        }
        
        public void RespawnCrates()
        {
            for (int i = 0; i < _initialCratePoses.Count(); i++)
            {
                crates[i].position = _initialCratePoses[i].position;
                crates[i].rotation = _initialCratePoses[i].rotation;
                crates[i].SetParent(this.transform);
                crates[i].gameObject.SetActive(true);
            }
        }
    }

}