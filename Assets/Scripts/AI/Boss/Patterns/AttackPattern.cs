using System.Collections;
using ObjectPool;
using UnityEngine;

namespace AI.BossPatterns.Patterns
{
    public abstract class AttackPattern : MonoBehaviour
    {
        [Header("Base Pattern Settings")]
        public string patternName;
        public float preparationTime = 0.5f;
        public float attackDuration = 2f;
        public float cooldownTime = 1f;
        public Transform playerTarget;
        public AudioClip attackSound;

        protected SnakeAI snakeAI;
        protected Rigidbody2D rb;
        protected Animator animator;

        protected GameObjectPool pool;

        private bool isExecuting = false;

        public void Initialize(SnakeAI ai, Rigidbody2D rigidbody, Animator anim)
        {
            snakeAI = ai;
            rb = rigidbody;
            animator = anim;
        }

        public virtual void DisableExtras(){}

        public IEnumerator ExecutePattern()
        {
            if (isExecuting) yield break;

            isExecuting = true;

            // Фаза подготовки
            yield return StartCoroutine(OnPreparation());

            // Фаза атаки
            yield return StartCoroutine(OnAttack());

            // Фаза восстановления
            yield return StartCoroutine(OnCooldown());

            isExecuting = false;
        }

        protected virtual IEnumerator OnPreparation()
        {
            // Анимация подготовки (рычание, свечение и т.д.)
            if (animator != null)
                animator.SetTrigger("Prepare");

            yield return new WaitForSeconds(preparationTime);
        }

        protected abstract IEnumerator OnAttack();

        protected virtual IEnumerator OnCooldown()
        {
            // Возврат в исходное состояние
            if (animator != null)
                animator.SetTrigger("Cooldown");

            yield return new WaitForSeconds(cooldownTime);
        }

        // Метод для плавного перемещения головы (основной для паттернов)
        protected IEnumerator MoveToPosition(Vector2 targetPosition, float duration, float rotationSpeed = 5f)
        {
            float elapsed = 0f;
            Vector2 startPosition = rb.position;

            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                float t = elapsed / duration;

                // Плавное перемещение
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, t);
                rb.MovePosition(newPosition);

                // Плавный поворот к цели
                Vector2 direction = (targetPosition - newPosition).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                rb.MoveRotation(Quaternion.Slerp(rb.transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

                yield return new WaitForFixedUpdate();
            }
        }
    }
}