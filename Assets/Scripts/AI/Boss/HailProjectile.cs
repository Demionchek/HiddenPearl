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

        public void Initialize(Vector2 target, GameObjectPool pool)
        {
            targetPosition = target;
            _pool = pool;
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
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                other.GetComponent<IHittable>().Hit();
                _pool.Return(gameObject);
            }
        }
    }
}