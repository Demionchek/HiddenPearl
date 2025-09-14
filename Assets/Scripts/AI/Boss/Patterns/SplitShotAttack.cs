using AI.BossPatterns;
using AI.BossPatterns.Patterns;
using System.Collections;
using ObjectPool;
using UnityEngine;

namespace AI.Boss.Patterns
{

    public class SplitShotAttack : AttackPattern
    {
        [Header("Split Shot Settings")]
        public GameObject projectilePrefab;
        public Transform[] firePoints; // 3 точки выстрела
        public float projectileSpeed = 8f;
        public float spreadAngle = 30f;
        public int shotsPerVolley = 3;
        public float shotDelay = 0.2f;

        private GameObjectPool _pool;

        protected override IEnumerator OnPreparation()
        {
            // Анимация открытия пасти
            if (animator != null)
                animator.SetTrigger("Prepare");

            if (_pool == null)
            {
                _pool = new GameObjectPool(projectilePrefab, 20);
            }

            yield return new WaitForSeconds(preparationTime);
        }

        protected override IEnumerator OnAttack()
        {
            for (int i = 0; i < shotsPerVolley; i++)
            {
                FireSplitShot();
                yield return new WaitForSeconds(shotDelay);
            }
        }

        private void FireSplitShot()
        {
            if (firePoints.Length != 3) return;

            // Выстрел тремя снарядами веером
            for (int i = 0; i < 3; i++)
            {
                Vector2 fireDirection = CalculateSpreadDirection(i);
                GameObject projectile = _pool.Get();
                if (i == 0)
                {
                    projectile.GetComponent<AudioSource>().Play();
                }
                projectile.transform.position = firePoints[i].position;
                Projectile proj = projectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Initialize(fireDirection, projectileSpeed, _pool);
                }
            }

            // Визуальный эффект выстрела
            if (animator != null)
                animator.SetTrigger("Attack");
        }

        private Vector2 CalculateSpreadDirection(int index)
        {
            Vector2 baseDirection = -transform.right; // Направление головы

            // Расчет угла для каждого снаряда
            float angle;
            switch (index)
            {
                case 0: angle = -spreadAngle; break; // Левый
                case 1: angle = 0f; break;           // Центральный
                case 2: angle = spreadAngle; break;  // Правый
                default: angle = 0f; break;
            }

            // Поворот направления на угол
            return Quaternion.Euler(0, 0, angle) * baseDirection;
        }

        protected override IEnumerator OnCooldown()
        {

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Reload");
            }

            yield return new WaitForSeconds(cooldownTime);
        }
    }
}