using System.Collections;
using Interfaces;
using ObjectPool;
using UnityEngine;

namespace AI.BossPatterns
{
    public class Projectile : MonoBehaviour
    {
        [Header("Movement Settings")]
        protected Vector2 direction;
        protected float speed;
        protected int damage = 5;

        [Header("Sine Wave Settings")]
        public float waveFrequency = 2f;    // Частота волны
        public float waveAmplitude = 1f;    // Амплитуда волны
        public bool useSineWave = true;     // Включить/выключить синусоиду

        [Header("Lifetime Settings")]
        public float lifetime = 3f;         // Время жизни снаряда

        private float spawnTime;
        private Vector2 perpendicularDirection;
        private float initialAngle;

        private bool hasHit = false;

        private GameObjectPool _pool;

        public void Initialize(Vector2 moveDirection, float moveSpeed, GameObjectPool pool)
        {
            direction = moveDirection.normalized;
            speed = moveSpeed;
            _pool = pool;

            // Рассчитываем перпендикулярное направление для синусоиды
            perpendicularDirection = new Vector2(-direction.y, direction.x);

            // Поворот спрайта в направлении движения
            initialAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, initialAngle);

            spawnTime = Time.time;

            hasHit = false;

            StartCoroutine(ReturnToPoolCoroutine());
        }

        private IEnumerator ReturnToPoolCoroutine()
        {
            yield return new WaitForSeconds(lifetime);
            _pool.Return(gameObject);
        }

        void Update()
        {
            // Базовое движение вперед
            Vector2 baseMovement = direction * speed * Time.deltaTime;

            // Синусоидальное движение
            Vector2 sineMovement = Vector2.zero;
            if (useSineWave)
            {
                float timeSinceSpawn = Time.time - spawnTime;
                float sineOffset = Mathf.Sin(timeSinceSpawn * waveFrequency * Mathf.PI) * waveAmplitude;
                sineMovement = perpendicularDirection * sineOffset * Time.deltaTime;
            }

            // Общее движение
            transform.Translate(baseMovement + sineMovement, Space.World);
        }

        private void ApplyWaveRotation()
        {
            float timeSinceSpawn = Time.time - spawnTime;
            float waveDerivative = Mathf.Cos(timeSinceSpawn * waveFrequency * Mathf.PI) * waveFrequency;
            float tiltAngle = waveDerivative * waveAmplitude * 10f; // Множитель для наглядности

            transform.rotation = Quaternion.Euler(0, 0, initialAngle + tiltAngle);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                hasHit = true;
                other.GetComponent<IHittable>().Hit();
                _pool.Return(gameObject);
            }
        }
    }
}