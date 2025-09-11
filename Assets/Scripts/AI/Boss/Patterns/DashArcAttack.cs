using System.Collections;
using UnityEngine;

namespace AI.BossPatterns.Patterns
{
    public class DashArcAttack : AttackPattern
    {
        [Header("Dash Settings")]
        public float dashTime = 1f;
        public float arcHeight = 3f;

        protected override IEnumerator OnAttack()
        {
            Vector2 startPos = rb.position;
            Vector2 dashTarget = playerTarget.position;

            if (animator != null)
                animator.SetTrigger("Attack");

            // Рывок к игроку по прямой
            yield return StartCoroutine(MoveToPosition(dashTarget, dashTime, 10f));

            // Возврат по дуге
            float returnDuration = 1.5f;
            float elapsed = 0f;

            while (elapsed < returnDuration)
            {
                elapsed += Time.fixedDeltaTime;
                float t = elapsed / returnDuration;

                // Расчет дуги (парабола)
                Vector2 arcPoint = Vector2.Lerp(dashTarget, startPos, t);
                arcPoint.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

                rb.MovePosition(arcPoint);

                // Поворот по направлению движения
                Vector2 direction = (arcPoint - (Vector2)rb.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rb.MoveRotation(angle);

                yield return new WaitForFixedUpdate();
            }

            if (animator != null)
                animator.SetTrigger("Cooldown");

            // Точное возвращение в стартовую позицию
            yield return StartCoroutine(MoveToPosition(startPos, 0.3f));
        }
    }
}