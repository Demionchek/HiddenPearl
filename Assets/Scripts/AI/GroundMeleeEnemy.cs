using System;
using System.Collections;
using System.Threading.Tasks;
using AI.States;
using DefaultNamespace;
using Interfaces;
using UnityEngine;

namespace AI
{
    public class GroundMeleeEnemy : BaseEnemy , IHittable , IHealth
    {
        [Header("GroundEnemySettings")]
        public int Health;
        public int MaxHealth;
        public float attackDistance = 0.3f;
        [SerializeField] public float attackDelaySec = 0.3f;

        private void Start()
        {
            Init();
            StartCoroutine(DetectionRoutine());
            ChangeState<PatrolStateAI>();
        }

        private void Update()
        {
            if (AnimationController.isAttacking || isDead) return;

            bool isAttackState = currState is AttackStateAI;

            if (canSeeTarget && canAttack && !isAttackState)
            {
                ChangeState<AttackStateAI>();
            }

            bool isChaseState = currState is ChaseStateAI;

            if (canSeeTarget && !canAttack && !isChaseState)
            {
                ChangeState<ChaseStateAI>();
            }

            if (!canSeeTarget)
            {
                ChangeState<PatrolStateAI>();
            }

            currState?.StateUpdate();
            currentAttackTime = Time.time;
        }

        private void FixedUpdate()
        {
            if (AnimationController.isAttacking || isDead) return;

            currState?.StateFixedUpdate();
        }

        public void OnAttack()
        {
            bool isFlip = AnimationController.GetSpriteRenderer().flipX;
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float hitDistance = attackDistance - radius;
            Vector2 direction = (isFlip ? new Vector2(-hitDistance,0) : new Vector2(hitDistance,0));
            origin += direction;

            // Выполняем CircleCast
            // Получаем все коллайдеры в радиусе
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, radius);

            if (hitColliders.Length == 0)
            {
                Debug.Log("В радиусе нет объектов.");
                return;
            }

            // Перебираем все найденные коллайдеры
            foreach (Collider2D collider in hitColliders)
            {
                if (collider == capsule) continue;

                Debug.Log($"Обнаружен объект: {collider.name}");

                // Проверяем, есть ли у него компонент для взаимодействия
                IHittable hittable = collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hittable.Hit();
                    break;
                }
            }

            if (attackSound != null)
                PlaySound(attackSound);
        }

        public void AttackDelay()
        {
            AnimationController.SetAnimatorSpeed(0);

            StartCoroutine(AttackDelayCoroutine());
        }

        private IEnumerator AttackDelayCoroutine()
        {
            yield return new WaitForSeconds(attackDelaySec);
            AnimationController.SetAnimatorSpeed(1);
            OnAttack();
        }

        private void PlaySound(AudioClip clip)
        {
            audioSource?.PlayOneShot(clip);
        }

        public void PlayAttackSound()
        {
            audioSource?.PlayOneShot(attackSound);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            bool isFlip = AnimationController.GetSpriteRenderer().flipX;
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float hitDistance = attackDistance - radius;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-hitDistance,0) : new Vector2(hitDistance,0));
            origin += direction;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, radius);
        }

        public void Hit()
        {
            if (isDead) return;

            SetHealth(Health - 1);

            healthChanged?.Invoke(Health);

            SetPlayerAsTarget();

            if (Health <=  0)
            {
                isDead = true;
                ChangeState<DeathState>();
            }
        }

        public Action<int> healthChanged { get; set; }
        public int GetHealth()
        {
            return Health;
        }
        public int GetMaxHealth()
        {
            return MaxHealth;
        }
        public void SetHealth(int health)
        {
            Health = health;
            healthChanged?.Invoke(health);
        }
    }
}