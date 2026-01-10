using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HGS.RLAgents.StackerSample
{
    public class StackerEnvironment : Environment
    {
        [SerializeField] Transform botttomLeftLimit;
        [SerializeField] Transform topRightLimit;

        public Transform checkpoint;
        public StackerAgent agent;
        public List<Transform> crates;

        List<Pose> _initialCratePoses;
        Vector2 _initialAgentPosition;

        protected override void Awake()
        {
            base.Awake();

            _initialAgentPosition = agent.transform.position;
            _initialCratePoses = crates
                .Select(crate => new Pose(crate.position, crate.rotation))
                .ToList();
        }

        public void SetCratePosition(int crateIndex, Vector2 position)
        {
            crates[crateIndex].position = position;
        }

        public void ShowCrate(int crateIndex)
        {
            crates[crateIndex].gameObject.SetActive(true);
        }

        public void ToggleCheckpoint(bool value)
        {
            checkpoint.gameObject.SetActive(value);
        }

        public void HideCrates()
        {
            for (int i = 0; i < crates.Count(); i++)
            {
                crates[i].gameObject.SetActive(false);
            }
        }

        public void RespawnCrate(int crateIndex)
        {
            crates[crateIndex].position = _initialCratePoses[crateIndex].position;
            crates[crateIndex].rotation = _initialCratePoses[crateIndex].rotation;
            crates[crateIndex].SetParent(this.transform);
            crates[crateIndex].gameObject.SetActive(true);
        }

        public void RespawnCrates()
        {
            for (int i = 0; i < _initialCratePoses.Count(); i++)
            {
                RespawnCrate(i);
            }
        }
    }

}