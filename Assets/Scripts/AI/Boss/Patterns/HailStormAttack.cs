using System.Collections;
using AI.BossPatterns;
using AI.BossPatterns.Patterns;
using UnityEngine;

namespace AI.Boss.Patterns
{
    public class HailStormAttack : AttackPattern
    {
        [Header("Hail Storm Settings")]
        public GameObject hailProjectilePrefab;
        public Transform hailSpawnPoint;
        public float randomSpawnDisatnce = 2;
        public int hailCount = 8;
        public float hailInterval = 0.3f;
    
        protected override IEnumerator OnAttack()
        {
            // Поворот головы вверх
            rb.MoveRotation(90f);
        
            // Анимация "рычания"
            if (animator != null)
                animator.SetTrigger("Roar");
        
            // Создание градин
            for (int i = 0; i < hailCount; i++)
            {
                SpawnHail();
                yield return new WaitForSeconds(hailInterval);
            }
        }
    
        private void SpawnHail()
        {
            Vector2 spawnPos = hailSpawnPoint.position;
            spawnPos.x += Random.Range(-randomSpawnDisatnce, randomSpawnDisatnce);
            Vector2 targetPos = spawnPos + Vector2.down * 20f; // Падение сверху вниз
        
            GameObject hail = Instantiate(hailProjectilePrefab, spawnPos, Quaternion.identity);
            HailProjectile projectile = hail.GetComponent<HailProjectile>();
            
            if (projectile != null)
            {
                projectile.Initialize(targetPos);
            }
        }
    }
}