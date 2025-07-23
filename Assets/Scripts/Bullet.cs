using System;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        public float speed = 3f;
        public float lifetime = 5f;
        private Rigidbody2D rb;
        private float lifetimeCounter;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            lifetimeCounter += Time.deltaTime;

            if (lifetimeCounter >= lifetime) Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = transform.right * speed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                collision.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) return;

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                collision.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) return;

            Destroy(gameObject);
        }
    }
}