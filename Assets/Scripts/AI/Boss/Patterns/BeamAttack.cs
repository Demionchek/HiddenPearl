using System.Collections;
using AI.BossPatterns;
using AI.BossPatterns.Patterns;
using UnityEngine;
namespace AI.Boss.Patterns
{

    public class BeamAttack : AttackPattern
    {
        [Header("Beam Settings")]
        public GameObject beamPrefab;
        public Transform mouthPosition;
        public float beamDuration = 2f;
        public float beamWidth = 0.5f;
        
        private GameObject currentBeam;
        
        protected override IEnumerator OnPreparation()
        {
            // Двигаемся в нижнюю точку
            Vector2 startPos = rb.position;
            Vector2 lowPosition = startPos + Vector2.down * 3f;
            
            yield return StartCoroutine(MoveToPosition(lowPosition, preparationTime));
            
            // Анимация открытия пасти
            if (animator != null)
                animator.SetTrigger("OpenMouth");
                
            yield return new WaitForSeconds(0.2f);
        }
        
        protected override IEnumerator OnAttack()
        {
            // Создаем луч
            currentBeam = Instantiate(beamPrefab, mouthPosition.position, Quaternion.identity);
            BeamController beam = currentBeam.GetComponent<BeamController>();
            
            if (beam != null)
            {
                beam.Initialize(transform.right, beamDuration, beamWidth);
            }
            
            // Двигаемся вверх while shooting
            float elapsed = 0f;
            Vector2 startPos = rb.position;
            Vector2 endPos = startPos + Vector2.up * 4f;
            
            while (elapsed < beamDuration)
            {
                elapsed += Time.fixedDeltaTime;
                float t = elapsed / beamDuration;
                
                Vector2 newPos = Vector2.Lerp(startPos, endPos, t);
                rb.MovePosition(newPos);
                
                yield return new WaitForFixedUpdate();
            }
            
            // Уничтожаем луч
            if (currentBeam != null)
                Destroy(currentBeam);
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