using Interfaces;
using ObjectPool;
using UnityEngine;

namespace AI.BossPatterns
{
    public class HailProjectile : MonoBehaviour
    {
        public float speed = 8f;
        private Vector2 targetPosition;
        private GameObjectPool _pool;

        bool hasHit = false;

        public void Initialize(Vector2 target, GameObjectPool pool)
        {
            targetPosition = target;
            _pool = pool;
            hasHit = false;
        }

        void Update()
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                _pool.Return(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                hasHit = true;
                other.GetComponent<IHittable>().Hit();
                _pool.Return(gameObject);
            }
        }
    }
}