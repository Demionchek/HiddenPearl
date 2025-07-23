using System;
using System.Collections;
using Animations;
using Pathfinding;
using Player;
//using Pathfinding;
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
        [SerializeField] private bool ignorePlayerShape = false;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = Color.yellow;

        private Transform target;
        private bool canSeeTarget = false;
        private AIDestinationSetter aiDestinationSetter;
        private SpriteRenderer spriteRenderer;
        private Transform homeTransform;
        private PlayerController playerController;

        public bool CanSeeTarget => canSeeTarget;
        public Transform Target => target;

        private void Start()
        {
            aiDestinationSetter = GetComponent<AIDestinationSetter>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            homeTransform = Instantiate(new GameObject($"home pos {gameObject.name}")).transform;
            homeTransform.position = transform.position;
            aiDestinationSetter.enabled = true;
            StartCoroutine(DetectionRoutine());
        }

        private IEnumerator DetectionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkFrequency);
                DetectTarget();
            }
        }

        private void SetDestinationEnable() => aiDestinationSetter.enabled = true;

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
        }

        private void DetectTarget()
        {
            // Сбрасываем состояние перед проверкой
            canSeeTarget = false;
            target = null;

            Vector2 sightPoint = new Vector2(transform.position.x, transform.position.y);

            // Ищем все цели в радиусе через SphereCast
            Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(sightPoint, sightRange, targetMask);

            foreach (Collider2D targetCollider in targetsInViewRadius)
            {

                if (playerController == null)
                {
                    if (targetCollider.TryGetComponent(out PlayerController player))
                    {
                        playerController = player;
                        playerController.OnRevive += ResetOnRevive;
                    }
                }

                if (!ignorePlayerShape && !canSeeTarget && playerController != null)
                {
                    if ( playerController.CurrentShape == PlayerController.Shape.Rat)
                    {
                        continue;
                    }
                }

                Transform potentialTarget = targetCollider.transform;
                Vector2 potentialTargetPos = new Vector2(potentialTarget.position.x, potentialTarget.position.y);
                Vector2 directionToTarget = (potentialTargetPos - sightPoint).normalized;

                spriteRenderer.flipX = directionToTarget.x < 0;

                Vector2 sightDirection = spriteRenderer.flipX ? -transform.right : transform.right;

                // Проверяем, находится ли цель в угле обзора
                if (Vector2.Angle(sightDirection, directionToTarget) < sightAngle / 2)
                {
                    float distanceToTarget = Vector2.Distance(sightPoint, potentialTargetPos);

                    sightPoint = new Vector2(transform.position.x, transform.position.y);

                    // Делаем Raycast для проверки препятствий
                    RaycastHit2D hit = Physics2D.Raycast(sightPoint, directionToTarget, distanceToTarget, obstacleMask);

                    // Если не попали в препятствие - цель видна
                    if (ignoreWalls || hit.collider == null)
                    {
                        target = potentialTarget;
                        aiDestinationSetter.target = target;
                        canSeeTarget = true;

                        break; // Выходим из цикла после обнаружения первой видимой цели
                    }
                }
            }

            if (!canSeeTarget && !ignoreOutOfRange) aiDestinationSetter.target = homeTransform;
        }
    }
}