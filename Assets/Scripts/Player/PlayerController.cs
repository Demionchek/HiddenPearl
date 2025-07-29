using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using UnityEngine.XR.WSA;
using Zenject;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IHittable
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private float jumpForce = 2f;
        [SerializeField] private float interactDelay = 0.5f;

        [Header("Other Settings")]
        [SerializeField] private RandomSoundPlayer randomAttackPlayerSound;
        [SerializeField] private RandomSoundPlayer randomHitPlayerSound;

        [Inject]
        private InputHandler inputHandler;
        [Inject]
        private DialogueSystem dialogueSystem;
        [Inject]
        private CheckPoints checkPoints;

        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private BoxCollider2D boxTriggerCollider;
        private PlayerAnimationController playerAnimationController;
        public bool isDead {get; private set;}
        private bool isFlip = false;
        private bool isDoubleJumping = false;
        private bool canJump = false;
        public float jumpTimer = 0.25f;
        private float lastJumpTime = 0f;

        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;

        private Vector2 effectorVelocity = Vector2.zero;

        public event Action OnRevive;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            boxTriggerCollider = GetComponent<BoxCollider2D>();
            playerAnimationController = GetComponent<PlayerAnimationController>();
        }

        private void Update()
        {
            isFlip = playerAnimationController.IsSpriteFliped();
            if (playerAnimationController.isAttacking ||
                isDead || dialogueSystem.isDialogRunning) return;

            HandleAttack();
            HandleJump();
            UpdateAnimationParameters();

            if (Time.time >= lastJumpTime + jumpTimer) canJump = true;
        }

        private void FixedUpdate()
        {
            if (isDead || dialogueSystem.isDialogRunning)
                return;

            if (playerAnimationController.isAttacking) return;

            HandleGroundMovement();
        }

        private void HandleGroundMovement()
        {
            CurrentSpeed = speed;

            // Проверяем, есть ли стена перед игроком в направлении движения
            bool isWallInFront = false;
            if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            {
                float direction = Mathf.Sign(inputHandler.MoveInput.x);
                float rayLength = capsuleCollider.size.x * 0.6f;
                Vector2 rayOrigin = (Vector2)transform.position + capsuleCollider.offset;

                float rayHeight = capsuleCollider.size.y * 0.4f;

                RaycastHit2D hitLower = Physics2D.Raycast(
                    rayOrigin + Vector2.up * (capsuleCollider.offset.y - rayHeight * 1.75f),
                    Vector2.right * direction,
                    rayLength,
                    LayerMask.GetMask("Ground", "Wall", "Platform"));

                isWallInFront = hitLower.collider != null;

                Debug.DrawRay(rayOrigin + Vector2.up * (capsuleCollider.offset.y - rayHeight * 1.75f),
                             Vector2.right * direction * rayLength, Color.red);
            }

            // Если есть стена перед нами, не применяем горизонтальное движение, но оставляем возможность прыжка
            float horizontalVelocity = isWallInFront ? 0 : inputHandler.MoveInput.x * CurrentSpeed;
            rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y) + effectorVelocity;
        }

        private void HandleJump()
        {
            if (inputHandler.JumpPressed && (IsGrounded || !isDoubleJumping) && canJump)
            {
               playerAnimationController.SetTrigger("Jump");
               rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
               isDoubleJumping = !isDoubleJumping;
               lastJumpTime = Time.time;
               canJump = false;
            }
        }

        public void Jump()
        {
        }

        private void HandleAttack()
        {
            if (inputHandler.AttackPressed)
            {
                playerAnimationController.SetTrigger("Attack");
                playerAnimationController.isAttacking = true;

                StartCoroutine(PlayAttackSoundWithDelay());

                Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
                float radius = 0.2f;
                float distance = 0.02f;
                Vector2 direction = (isFlip ? new Vector2(-0.1f,0) : new Vector2(0.1f,0));
                origin += direction;

                // Выполняем CircleCast
                // Получаем все коллайдеры в радиусе
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, radius);

                if (hitColliders.Length == 0)
                {
                    Debug.Log("В радиусе нет объектов.");
                    return;
                }

                foreach (Collider2D collider in hitColliders)
                {
                    Debug.Log($"Обнаружен объект: {collider.name}");

                    IInteractable interactable = collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.Interact();
                    }

                    IHittable hit = collider.GetComponent<IHittable>();
                    if (hit != null)
                    {
                        hit.Hit();
                    }
                }
            }
        }

        private IEnumerator PlayAttackSoundWithDelay()
        {
            yield return new WaitForSeconds(0.1f);
            if (randomAttackPlayerSound != null)
                randomAttackPlayerSound.PlayRandomSoundNow();
        }

        // Отрисовка радиуса в редакторе
        private void OnDrawGizmos()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-0.25f,0) : new Vector2(0.25f,0));
            origin += direction;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, radius);
        }

        private void UpdateAnimationParameters()
        {
            playerAnimationController.SetMovementParameters(
                Mathf.Abs(inputHandler.MoveInput.x),
                IsGrounded
            );
        }

        public void Hit()
        {
            if (isDead || dialogueSystem.isDialogRunning) return;

            playerAnimationController.SetTrigger(AnimationController.IS_DEAD_S);
            isDead = true;
            rb.gravityScale = 1;

            StartCoroutine(ReviveCoroutine());
        }

        private IEnumerator ReviveCoroutine()
        {
            yield return new WaitForSeconds(3f);

            isDead = false;
            transform.position = checkPoints.CurrentCheckPoint.position;
            playerAnimationController.SetTrigger(AnimationController.REVIVE_S);

            OnRevive?.Invoke();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                other.collider.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!isDead)
                    Hit();
            }

            if (other.collider.gameObject.TryGetComponent(out SurfaceEffector2D surfaceEffector))
            {
                effectorVelocity = new Vector2(surfaceEffector.speed, 0);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    IsGrounded = true;
                    return;
                }
            }
            IsGrounded = false;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            IsGrounded = false;

            if (collision.collider.gameObject.TryGetComponent(out SurfaceEffector2D surfaceEffector))
            {
                effectorVelocity = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!isDead)
                    Hit();
            }
        }
    }
}