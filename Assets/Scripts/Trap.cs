using System;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public interface ITrap
    {
        public void Trigger();
    }

    public class Trap : MonoBehaviour, ITrap
    {
        public float radius = 0.2f;
        public float distance = 0.1f;
        public bool destroyOnTrigger = false;
        public bool killOnTrigger = false;

        private Animator animator;
        private Rigidbody2D rigidbody2D;

        private void Start()
        {
            animator = GetComponent<Animator>();
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void Trigger()
        {
            if(animator != null) animator.SetTrigger("Attack");

            if (rigidbody2D != null)
            {
                rigidbody2D.WakeUp();
            }
        }

        public void OnAttack()
        {
            RaycastHit2D[] raycastHits2D;
            raycastHits2D = Physics2D.CircleCastAll(transform.position,radius, Vector2.up, distance, LayerMask.GetMask("Player"));

            for (int i = 0; i < raycastHits2D.Length; i++)
            {
                IHittable hittable = raycastHits2D[i].collider.GetComponent<IHittable>();
                hittable.Hit();
                break;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<IHittable>(out IHittable hit))
            {
                hit.Hit();
                if (killOnTrigger) hit.Kill();
            }

            if (destroyOnTrigger) Destroy(gameObject);
        }
    }
}