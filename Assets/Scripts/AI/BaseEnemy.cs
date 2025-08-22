using System;
using System.Collections;
using System.Collections.Generic;
using AI.States;
using Animations;
using DefaultNamespace;
using Interfaces;
using Player;
using UnityEngine;
using Zenject;

namespace AI
{
    public enum AIState
    {
        empty,
        idle,
        attack,
        death,
        patrol
    }

    [RequireComponent(typeof(EnemyAnimationController), typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public abstract class BaseEnemy : MonoBehaviour, IHealth, IHittable
    {
        [SerializeField] public float speed;
        [SerializeField] private bool isRange = false;
        [SerializeField] AIState startState = AIState.idle;

        [Space(5)]
        [Header("Spotting")]
        [SerializeField] private float sightRange = 2f;
        [SerializeField] private float sightAngle = 90f;
        [SerializeField] private float checkFrequency = 0.2f;
        [SerializeField] protected LayerMask targetMask;
        [SerializeField] protected LayerMask obstacleMask;
        [SerializeField] public float attackDelay = 2f;
        [SerializeField] public float attackHoldSec = 0.3f;
        [HideInInspector] public bool canSeeTarget = false;

        [Space(5)]
        [Header("Patroll")]
        [SerializeField] public List<Transform> patrolPoints;
        [SerializeField] public float waitTimeAtPoint = 2f;
        public float reachedPointDistance = 0.1f;
        [HideInInspector] public int currentPointIndex = 0;
        [HideInInspector] public bool isWaiting = false;

        [Space(5)]
        [Header("Chasing")]
        [SerializeField] public float stoppingDistance = 1f;
        [SerializeField] private bool ignoreOutOfRange = false;
        [HideInInspector] public bool canAttack = false;
        [HideInInspector] public bool isChasing = false;
        public Vector2 lastKnownPosition;
        public Vector2 savedHitPosition;
        [Header("Health")]
        public int Health = 3;
        public int MaxHealth = 3;
        public event Action<BaseEnemy> OnDeath;

        protected bool isDead = false;
        protected BaseStateAI currState;
        protected List<BaseStateAI> createdStates;
        public Transform target;
        public Rigidbody2D rb { get; protected set; }
        public CapsuleCollider2D capsule { get; protected set; }
        public EnemyAnimationController AnimationController { get; protected set; }
        [Header("StateOverride")]
        [SerializeField] public bool loopOverrideState = false;
        [SerializeField] public AIState overrideAIState;

        [Header("Audio")]
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip attackSound;
        [SerializeField] protected AudioClip deathSound;

        [HideInInspector] public float currentAttackTime = 0f;
        [HideInInspector] public float lastAttackTime = 0f;

        private static float SIGHT_OFFSET = 0.1f;
        private static float TARGET_OFFSET = 0.1f;

        public Action<int> healthChanged { get; set; }

        [Inject]
        PlayerController playerController;

        public virtual void Init()
        {
            AnimationController = GetComponent<EnemyAnimationController>();
            rb = GetComponent<Rigidbody2D>();
            capsule = GetComponent<CapsuleCollider2D>();
            createdStates = new List<BaseStateAI>();
            StartCoroutine(DetectionRoutine());
        }

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
        
        public void IncreaseHealth(int amount)
        {
            MaxHealth += amount;
            SetHealth(MaxHealth);
        }
        
        public void Hit()
        {
            if (isDead) return;

            SetHealth(Health - 1);

            healthChanged?.Invoke(Health);

            SetPlayerAsTarget();

            if (Health <= 0)
            {
                isDead = true;
                ChangeState<DeathState>();
                OnDeath?.Invoke(this);
            }
        }

        public virtual void ActivateSpecial(bool isActive) { }

        public IEnumerator DetectionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();

                if (canSeeTarget)
                {
                    CanAttack();
                }
            }
        }

        private void CanAttack()
        {
            Vector2 direction = target.position - transform.position;
            float distance = direction.magnitude;

            if (distance < stoppingDistance)
            {
                canAttack = true;
            }
        }

        public void SetPlayerAsTarget()
        {
            this.target = PlayerController.Instance.transform;
            lastKnownPosition = target.position;
            canSeeTarget = true;
            isChasing = true;
            ChangeState<ChaseStateAI>();
        }

        public void AttackDelay()
        {
            AnimationController.SetAnimatorSpeed(0);

            StartCoroutine(AttackDelayCoroutine());
        }

        private IEnumerator AttackDelayCoroutine()
        {
            yield return new WaitForSeconds(attackHoldSec);
            AnimationController.SetAnimatorSpeed(1);

            if (isDead) yield break;

            OnAttack();
        }

        protected virtual void OnAttack() { }

        private void DetectTarget()
        {
            canSeeTarget = false;
            canAttack = false;

            if (playerController == null) return;

            Vector2 sightPoint = new Vector2(transform.position.x, transform.position.y - SIGHT_OFFSET);

            Vector2 potentialTargetPos = new Vector2(playerController.transform.position.x, playerController.transform.position.y + TARGET_OFFSET);
            Vector2 directionToTarget = (potentialTargetPos - sightPoint).normalized;

            Vector2 sightDirection = AnimationController.GetSpriteRenderer().flipX ? -transform.right : transform.right;

            float distanceToTarget = Vector2.Distance(sightPoint, potentialTargetPos);

            bool inAngle = Vector2.Angle(sightDirection, directionToTarget) < sightAngle / 2;
            bool inRange = distanceToTarget <= sightRange;

            sightPoint = new Vector2(transform.position.x, transform.position.y + SIGHT_OFFSET);

            RaycastHit2D hit = Physics2D.Raycast(sightPoint, directionToTarget, distanceToTarget, obstacleMask);

            bool noObstacle = hit.collider == null;

            if (inRange && inAngle && noObstacle)
            {
                target = playerController.transform;
                lastKnownPosition = potentialTargetPos;
                canSeeTarget = true;
                isChasing = true;
            }
            else
            {
                canSeeTarget = false;

                if (isChasing)
                {
                    bool lostDueToObstacle = inRange && inAngle && !noObstacle;
                    bool lostDueToRangeOrAngle = !(inRange && inAngle);
                    bool shouldStop = lostDueToObstacle || (lostDueToRangeOrAngle && !ignoreOutOfRange);

                    if (shouldStop)
                    {
                        isChasing = false;
                    }
                }
            }
        }

        public void Revive()
        {
            SetHealth(MaxHealth);
            ChangeState<IdleStateAI>();
            capsule.enabled = true;
            if(!isDead) return;
            isDead = false;
            if (AnimationController == null)
            {
                AnimationController = GetComponent<EnemyAnimationController>();
            }
            AnimationController.SetAnimatorTrigger("Revive");
        }

        #region State switch and creation

        public void ChangeState<T>() where T : BaseStateAI, new()
        {
            BaseStateAI previousState = null;

            if (currState != null)
            {
                if (typeof(T) == currState.GetType()) return;

                previousState = currState;
                currState.ExitState();
            }

            // creates and enters the new state
            currState = CreateState<T>();
            currState.baseEnemy = this;
            currState.animatonController = AnimationController;
            currState.prevState = previousState;
            currState.EnterState();
        }

        public BaseStateAI GetStateAI(bool getPrevState)
        {
            if (getPrevState)
                return currState.prevState;

            return currState;
        }

        private BaseStateAI CreateState<T>() where T : BaseStateAI, new()
        {
            if (createdStates == null) createdStates = new List<BaseStateAI>();
            if (createdStates.Count == 0) return new T();
            
            //check if state has been created
            for (int i = 0; i < createdStates.Count; i++)
            {
                if (typeof(T) == createdStates[i].GetType())
                    return createdStates[i];
            }

            return new T();
        }
        #endregion


    }
}