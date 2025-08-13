using System;
using System.Collections;
using Animations;
using Interfaces;
using Pathfinding;
using Player;
using UnityEngine;

namespace DefaultNamespace
{
    public class DestinationSetOnSight : MonoBehaviour
    {
        [Header("Vision Settings")]
        [SerializeField] private float sightRange = 10f; // Радиус обзора
        [SerializeField] [Range(0, 360)] private float sightAngle = 90f; // Угол обзора
        [SerializeField] private float checkFrequency = 0.2f; // Частота проверок в секундах
        [SerializeField] private LayerMask targetMask; // Маска целей
        [SerializeField] private LayerMask obstacleMask; // Маска препятствий
        [SerializeField] private bool ignoreWalls = false;
        [SerializeField] private bool ignoreOutOfRange = false;
        [SerializeField] private bool respawnAfterEliminated = false;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolDelay = 1f;
        [SerializeField] private float arrivalDistance = 0.5f;

        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private Vector2 attackOffset = new Vector2(0.2f, 0f);
        [SerializeField] private float attackRadius = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = Color.yellow;

        private Transform target;
        private bool canSeeTarget = false;
        private bool isChasing = false;
        private bool isWaiting = false;
        private int currentPatrolIndex = 0;
        private AIDestinationSetter aiDestinationSetter;
        private AIPath aiPath;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Transform homeTransform;
        private PlayerController playerController;
        private float lastAttackTime = -Mathf.Infinity;

        public bool CanSeeTarget => canSeeTarget;
        public Transform Target => target;

        private void Start()
        {
            aiDestinationSetter = GetComponent<AIDestinationSetter>();
            aiPath = GetComponent<AIPath>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            homeTransform = Instantiate(new GameObject($"home pos {gameObject.name}")).transform;
            homeTransform.position = transform.position;
            aiDestinationSetter.enabled = true;

            // Initialize starting destination
            if (patrolPoints.Length > 0)
            {
                currentPatrolIndex = 0;
                aiDestinationSetter.target = patrolPoints[currentPatrolIndex];
            }
            else
            {
                aiDestinationSetter.target = homeTransform;
            }

            StartCoroutine(DetectionRoutine());
        }

        private void Update()
        {
            // Handle sprite flip
            if (canSeeTarget && target != null)
            {
                spriteRenderer.flipX = (target.position.x - transform.position.x) < 0;
            }
            else if (aiPath.velocity.x != 0)
            {
                spriteRenderer.flipX = aiPath.velocity.x < 0;
            }
            
            // Handle attack when close to target
            if (canSeeTarget && Time.time >= lastAttackTime + attackCooldown)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                if (distanceToTarget <= attackRange)
                {
                    animator.SetTrigger(AnimationController.ATTACK_S);
                }
            }
            
            // Handle patrol progression when not chasing
            if (isChasing || patrolPoints.Length == 0 || isWaiting) return;

            if (Vector2.Distance(transform.position, aiDestinationSetter.target.position) < arrivalDistance)
            {
                StartCoroutine(WaitAndProceedToNextPatrolPoint());
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
                playerController.OnRevive -= ResetOnRevive;
        }

        private void ResetOnRevive()
        {
            transform.position = homeTransform.position;
            target = homeTransform;
            aiDestinationSetter.target = target;
            isChasing = false;
        }

        private IEnumerator DetectionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();
            }
        }

        private void DetectTarget()
        {
            // Reset state before checking
            canSeeTarget = false;
            target = null;

            Vector2 sightPoint = (Vector2)transform.position;

            if (playerController == null)
            {
                // Find potential targets (players) in sight range
                Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(sightPoint, sightRange, targetMask);

                foreach (Collider2D targetCollider in targetsInViewRadius)
                {
                    if (targetCollider.TryGetComponent(out PlayerController player))
                    {
                        playerController = player;
                        playerController.OnRevive += ResetOnRevive;
                        break; // Assuming only one player
                    }
                }
            }

            if (playerController == null) return;

            // Now check visibility to the known player
            Vector2 potentialTargetPos = (Vector2)playerController.transform.position;
            Vector2 directionToTarget = (potentialTargetPos - sightPoint).normalized;
            float distanceToTarget = Vector2.Distance(sightPoint, potentialTargetPos);

            // Sight direction based on current facing (but flip handled in Update)
            Vector2 sightDirection = spriteRenderer.flipX ? -transform.right : transform.right;

            bool inAngle = Vector2.Angle(sightDirection, directionToTarget) < sightAngle / 2;
            bool inRange = distanceToTarget <= sightRange;

            // Raycast up to sight range max
            RaycastHit2D hit = Physics2D.Raycast(sightPoint, directionToTarget, Mathf.Min(distanceToTarget, sightRange), obstacleMask);
            bool noObstacle = ignoreWalls || hit.collider == null;

            if (inRange && inAngle && noObstacle)
            {
                target = playerController.transform;
                aiDestinationSetter.target = target;
                canSeeTarget = true;
                isChasing = true;
            }
            else
            {
                target = null;

                if (isChasing)
                {
                    bool lostDueToObstacle = inAngle && !noObstacle;
                    bool lostDueToRangeOrAngle = !inRange || !inAngle;
                    bool shouldStop = lostDueToObstacle || (lostDueToRangeOrAngle && !ignoreOutOfRange);

                    if (shouldStop)
                    {
                        isChasing = false;
                        if (patrolPoints.Length > 0)
                        {
                            currentPatrolIndex = FindNearestPatrolIndex();
                            aiDestinationSetter.target = patrolPoints[currentPatrolIndex];
                        }
                        else
                        {
                            aiDestinationSetter.target = homeTransform;
                        }
                    }
                    // Else, keep aiDestinationSetter.target as the player (continue chasing)
                }
            }
        }

        private void Attack()
        {
            lastAttackTime = Time.time;

            // Calculate attack point based on facing direction
            Vector2 attackPoint = (Vector2)transform.position + (spriteRenderer.flipX ? -attackOffset : attackOffset);

            // Perform CircleCast for damage
            if (target != null)
            {
                Vector2 directionToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
                RaycastHit2D[] hits = Physics2D.CircleCastAll(attackPoint, attackRadius, directionToTarget, 0.1f, targetMask);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.TryGetComponent(out IHittable hittable))
                    {
                        hittable.Hit();
                        break;
                    }
                }
            }
        }

        private int FindNearestPatrolIndex()
        {
            int nearestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                float distance = Vector2.Distance(transform.position, patrolPoints[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private IEnumerator WaitAndProceedToNextPatrolPoint()
        {
            isWaiting = true;
            aiDestinationSetter.enabled = false;
            yield return new WaitForSeconds(patrolDelay);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            aiDestinationSetter.target = patrolPoints[currentPatrolIndex];
            aiDestinationSetter.enabled = true;
            isWaiting = false;
        }

        // Optional: Gizmos for debugging sight range, angle, and attack
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            Gizmos.color = gizmoColor;
            Vector3 position = transform.position;

            // Draw sight range circle
            Gizmos.DrawWireSphere(position, sightRange);

            // Draw sight angle lines
            Vector3 sightDirection = spriteRenderer != null && spriteRenderer.flipX ? -transform.right : transform.right;
            Quaternion leftRayRotation = Quaternion.AngleAxis(-sightAngle / 2, Vector3.forward);
            Quaternion rightRayRotation = Quaternion.AngleAxis(sightAngle / 2, Vector3.forward);
            Vector3 leftRayDirection = leftRayRotation * sightDirection;
            Vector3 rightRayDirection = rightRayRotation * sightDirection;
            Gizmos.DrawLine(position, position + leftRayDirection * sightRange);
            Gizmos.DrawLine(position, position + rightRayDirection * sightRange);

            // Draw attack range
            Vector3 attackPoint = position + (spriteRenderer != null && spriteRenderer.flipX ? new Vector3(-attackOffset.x, attackOffset.y) : (Vector3)attackOffset);
            Gizmos.DrawWireSphere(attackPoint, attackRadius);
        }
    }
}