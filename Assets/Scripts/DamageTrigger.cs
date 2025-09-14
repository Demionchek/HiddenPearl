using System;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class DamageTrigger : MonoBehaviour
    {
        private CircleCollider2D collider2D;
        bool hasBeenTriggered = false;

        private void Start()
        {
            collider2D = GetComponent<CircleCollider2D>();
        }

        public void EnableCollider()
        {
            collider2D.enabled = true;
            hasBeenTriggered = false;
        }
        public void DisableCollider() => collider2D.enabled = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Player") || hasBeenTriggered) return;

            if (collision.TryGetComponent(out IHittable hittable))
            {
                hasBeenTriggered = true;
                hittable.Hit();
                DisableCollider();
            }
        }
    }
}