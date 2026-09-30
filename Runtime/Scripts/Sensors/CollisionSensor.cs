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
            for (int i = 0; i < rigidbodies?.Length; i++)
            {
                var child = rigidbodies[i].gameObject?.AddComponent<CollisionSensor>();
                if (child != null) child.parent = this;
            }
        }

        void OnCollisionEnter(Collision col)
        {
            if (parent != null)
            {
                parent.onCollisionEnterEvent?.Invoke(col);
            }
            else
            {
                onCollisionEnterEvent?.Invoke(col);

            }
        }

        void OnCollisionStay(Collision col)
        {
            if (onCollisionStayEvent != null)
            {
                onCollisionStayEvent?.Invoke(col);
            }
            else
            {
                parent?.onCollisionStayEvent?.Invoke(col);
            }
        }

        void OnCollisionExit(Collision col)
        {
            if (parent != null)
            {
                parent.onCollisionExitEvent?.Invoke(col);
            }
            else
            {
                onCollisionExitEvent?.Invoke(col);
            }
        }
    }
}
