using System;
using AI.States;
using Interfaces;
using ObjectPool;
using UnityEngine;

namespace AI
{
    public class ShooterGroundEnemy : BaseEnemy, IHittable
    {

        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform shootPosLeft;
        [SerializeField] private Transform shootPosRight;
        private GameObjectPool pool;

        private void Awake()
        {
            Init();
            pool = new GameObjectPool(bulletPrefab, 10);
            StartCoroutine(DetectionRoutine());
            ChangeState<PatrolStateAI>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if(isDead) return;
            
            bool isAttackState = currState is AttackStateAI;
            bool canAttack = target != null && 
                             stoppingDistance > Vector2.Distance(transform.position, target.position);

            if (isChasing && canAttack && !isAttackState)
            {
                ChangeState<AttackStateAI>();
            }
            
            if (isChasing && !canAttack && !AnimationController.isAttacking) ChangeState<ChaseStateAI>();

            if (!isChasing)
            {
                ChangeState<PatrolStateAI>();
            }

            currState?.StateUpdate();
            currentAttackTime = Time.time;
        }
        private void FixedUpdate()
        {
            currState?.StateFixedUpdate();
        }

        protected override void OnAttack()
        {
            if (target == null) return;
            
            
            GameObject bullet = pool.Get();
            bullet.transform.position = AnimationController.GetSpriteRenderer().flipX ? shootPosLeft.position : shootPosRight.position;
            Vector2 targetPosition = new Vector2(savedHitPosition.x, savedHitPosition.y + 0.25f);
            // Направление к цели
            Vector2 direction = targetPosition - (Vector2)bullet.transform.position;
            // Вычисляем угол в радианах и конвертируем в градусы
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Поворачиваем объект (для 2D обычно используется ось Z)
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            audioSource.Play();
        }
    }
}