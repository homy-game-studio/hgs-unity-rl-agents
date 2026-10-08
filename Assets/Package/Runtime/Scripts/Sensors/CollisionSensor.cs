using System;
using UnityEngine;

namespace HGS.RLAgents
{
    public class CollisionSensor : MonoBehaviour
    {
        [SerializeField] Rigidbody[] rigidbodies;

        [HideInInspector]
        public CollisionSensor parent;

        public Action<Collision> onCollisionEnterEvent;
        public Action<Collision> onCollisionStayEvent;
        public Action<Collision> onCollisionExitEvent;

        private void Awake()
        {
            if (rigidbodies == null) return;

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                if (rigidbodies[i] == null) continue;

                // Collisions of this rigidbody are already received by this component
                if (rigidbodies[i].gameObject == gameObject) continue;

                var child = rigidbodies[i].gameObject.AddComponent<CollisionSensor>();
                child.parent = this;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            if (parent != null) parent.onCollisionEnterEvent?.Invoke(col);
            else onCollisionEnterEvent?.Invoke(col);
        }

        void OnCollisionStay(Collision col)
        {
            if (parent != null) parent.onCollisionStayEvent?.Invoke(col);
            else onCollisionStayEvent?.Invoke(col);
        }

        void OnCollisionExit(Collision col)
        {
            if (parent != null) parent.onCollisionExitEvent?.Invoke(col);
            else onCollisionExitEvent?.Invoke(col);
        }
    }
}
