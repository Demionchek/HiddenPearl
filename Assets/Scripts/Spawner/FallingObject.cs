using System.Collections.Generic;
using ObjectPool;
using UnityEngine;

namespace Spawner
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingObject : MonoBehaviour
    {
        private GameObjectPool _pool;
        private List<Vector2> _activePositions;
        private Vector2 _spawnPosition;

        private static int _groundLayerMask = -1;

        private void Awake()
        {
            if (_groundLayerMask == -1)
                _groundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        }

        public void Init(GameObjectPool pool, List<Vector2> activePositions, Vector2 spawnPosition)
        {
            _pool = pool;
            _activePositions = activePositions;
            _spawnPosition = spawnPosition;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & _groundLayerMask) == 0) return;

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _activePositions?.Remove(_spawnPosition);
            _pool?.Return(gameObject);
        }
    }
}
