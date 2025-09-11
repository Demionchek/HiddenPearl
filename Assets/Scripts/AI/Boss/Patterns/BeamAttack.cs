using System.Collections;
using AI.BossPatterns;
using AI.BossPatterns.Patterns;
using UnityEngine;
namespace AI.Boss.Patterns
{

    public class BeamAttack : AttackPattern
    {
        [Header("Beam Settings")]
        public GameObject currentBeam;
        public Transform mouthPosition;
        public float beamDuration = 2f;
        public float beamWidth = 0.5f;

        public Vector2 lowerPos;
        public Vector2 finalPos;


        protected override IEnumerator OnPreparation()
        {
            // Двигаемся в нижнюю точку

            yield return StartCoroutine(MoveToPosition(lowerPos, preparationTime));

            // Анимация открытия пасти
            if (animator != null)
                animator.SetTrigger("Prepare");

            yield return new WaitForSeconds(0.2f);
        }

        protected override IEnumerator OnAttack()
        {
            animator.SetTrigger("Attack");

            // Создаем луч
            currentBeam.SetActive(true);
            BeamController beam = currentBeam.GetComponent<BeamController>();

            if (beam != null)
            {
                beam.Initialize();
            }

            // Двигаемся вверх while shooting
            float elapsed = 0f;
            Vector2 startPos = rb.position;

            while (elapsed < beamDuration)
            {
                elapsed += Time.fixedDeltaTime;
                float t = elapsed / beamDuration;

                Vector2 newPos = Vector2.Lerp(startPos, finalPos, t);
                rb.MovePosition(newPos);

                yield return new WaitForFixedUpdate();
            }

            // Уничтожаем луч
            if (currentBeam != null)
                currentBeam.SetActive(false);
        }

        protected override IEnumerator OnCooldown()
        {
            // Возвращаемся на исходную позицию
            Vector2 combatPos = snakeAI.defaultCombatPosition.position;
            yield return StartCoroutine(MoveToPosition(combatPos, cooldownTime));

            if (animator != null)
                animator.SetTrigger("CloseMouth");
        }
    }
}