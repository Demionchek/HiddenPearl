using System;
using System.Collections;
using ObjectPool;
using UnityEngine;

namespace DefaultNamespace
{
    public class BubbleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject bubblePrefab;
        [SerializeField] private float spawnDelay;

        private GameObjectPool pool;

        private void Start()
        {
            pool = new GameObjectPool(bubblePrefab, 3);
            StartCoroutine(SpawnBubbleWithDelay(spawnDelay));
        }

        private IEnumerator SpawnBubbleWithDelay(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);

                OxygenBubble bubble = pool.Get().GetComponent<OxygenBubble>();
                bubble.spawner = this;
            }
        }

        public void Return(GameObject bubble) => pool.Return(bubble);
    }
}