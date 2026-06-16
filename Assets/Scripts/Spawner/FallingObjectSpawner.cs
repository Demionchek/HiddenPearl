using System.Collections;
using System.Collections.Generic;
using ObjectPool;
using UnityEngine;

namespace Spawner
{
    [RequireComponent(typeof(Collider2D))]
    public class FallingObjectSpawner : MonoBehaviour
    {
        [Header("Object Settings")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int preloadCount = 10;
        [SerializeField] private float mass = 1f;
        [SerializeField] private float minDistance = 0.5f;

        [Header("Spawn Timing")]
        [SerializeField] private float spawnInterval = 0.5f;

        private GameObjectPool _pool;
        private readonly List<Vector2> _activePositions = new List<Vector2>();
        private Bounds _spawnBounds;

        private void Start()
        {
            _spawnBounds = GetComponent<Collider2D>().bounds;
            _pool = new GameObjectPool(prefab, preloadCount);
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnInterval);
                TrySpawn();
            }
        }

        private void TrySpawn()
        {
            Vector2 spawnPos;
            if (!TryGetSpawnPosition(out spawnPos)) return;

            GameObject obj = _pool.Get();
            obj.transform.position = spawnPos;
            obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb == null) rb = obj.AddComponent<Rigidbody2D>();
            rb.mass = mass;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            FallingObject fallingObj = obj.GetComponent<FallingObject>();
            if (fallingObj == null) fallingObj = obj.AddComponent<FallingObject>();
            fallingObj.Init(_pool, _activePositions, spawnPos);
        }

        private bool TryGetSpawnPosition(out Vector2 result)
        {
            const int maxAttempts = 10;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2 candidate = new Vector2(
                    Random.Range(_spawnBounds.min.x, _spawnBounds.max.x),
                    Random.Range(_spawnBounds.min.y, _spawnBounds.max.y)
                );

                if (IsFarEnough(candidate))
                {
                    _activePositions.Add(candidate);
                    result = candidate;
                    return true;
                }
            }

            result = Vector2.zero;
            return false;
        }

        private bool IsFarEnough(Vector2 candidate)
        {
            foreach (Vector2 pos in _activePositions)
            {
                if (Vector2.Distance(candidate, pos) < minDistance)
                    return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null) return;

            Bounds b = Application.isPlaying ? _spawnBounds : col.bounds;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawCube(b.center, new Vector3(b.size.x, b.size.y, 0.1f));
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
            Gizmos.DrawWireCube(b.center, new Vector3(b.size.x, b.size.y, 0.1f));
        }
    }
}
