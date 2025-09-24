using System;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class OxygenBubble : MonoBehaviour
    {
        [SerializeField] private float maxY = 0;
        [SerializeField] private float speed = 1.2f;

        public BubbleSpawner spawner;

        private Rigidbody2D rb;
        private RandomSoundPlayer audio;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            audio = GetComponent<RandomSoundPlayer>();
            transform.position = spawner.transform.position;
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = Vector2.up * speed;

            if (transform.position.y >= maxY) Return();
        }

        public void Return()
        {
            spawner.Return(gameObject);
            transform.position = spawner.transform.position;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Return();
                audio.PlayRandomSoundNow();
            }
        }
    }
}