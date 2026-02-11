using System;
using UnityEngine;

namespace HGS.RLAgents
{
    public class CollisionSensor2D : MonoBehaviour
    {
        [SerializeField] Rigidbody2D[] rigidbody2Ds;

        [HideInInspector]
        public CollisionSensor2D parent;

        public Action<Collision2D> onCollisionEnter2DEvent;
        public Action<Collision2D> onCollisionStay2DEvent;
        public Action<Collision2D> onCollisionExit2DEvent;

        private void Awake()
        {
            for (int i = 0; i < rigidbody2Ds?.Length; i++)
            {
                var child = rigidbody2Ds[i].gameObject?.AddComponent<CollisionSensor2D>();
                if (child != null) child.parent = this;
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (parent != null)
            {
                parent.onCollisionEnter2DEvent?.Invoke(col);
            }
            else
            {
                onCollisionEnter2DEvent?.Invoke(col);

            }
        }

        void OnCollisionStay2D(Collision2D col)
        {
            if (onCollisionStay2DEvent != null)
            {
                onCollisionStay2DEvent?.Invoke(col);
            }
            else
            {
                parent?.onCollisionStay2DEvent?.Invoke(col);
            }
        }

        void OnCollisionExit2D(Collision2D col)
        {
            if (parent != null)
            {
                parent.onCollisionExit2DEvent?.Invoke(col);
            }
            else
            {
                onCollisionExit2DEvent?.Invoke(col);
            }
        }
    }
}
