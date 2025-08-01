using System;
using System.Collections;
using Animations;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using Zenject;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IHittable
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float jumpForce = 2.5f;
        [SerializeField] private float interactDelay = 0.5f;

        [Header("Swimming Settings")]
        [SerializeField] private float swimSpeed = 3f;
        [SerializeField] private float swimUpForce = 1f;
        [SerializeField] private float waterSurfaceLevelOffset = 3.5f;
        [SerializeField] private LayerMask waterLayer;

        [Header("Other Settings")]
        [SerializeField] private int MaxHealth = 3;
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
        private BoxCollider2D  boxTriggerCollider;
        private PlayerAnimationController playerAnimationController;
        private Collider2D waterCollider;
        public bool isDead {get; private set;}
        private bool isFlip = false;
        private bool isDoubleJumping = false;

        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public int Health { get; private set; }
        public Vector2 Velocity => rb.linearVelocity;

        private Vector2 effectorVelocity = Vector2.zero;
        private bool isSwimming;

        public event Action OnRevive;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            boxTriggerCollider = GetComponent<BoxCollider2D>();
            playerAnimationController = GetComponent<PlayerAnimationController>();
            Health = MaxHealth;
        }

        private void Update()
        {
            isFlip = playerAnimationController.IsSpriteFliped();
            if (playerAnimationController.isAttacking ||
                isDead || dialogueSystem.isDialogRunning) return;

            HandleAttack();
            HandleJump();
            UpdateAnimationParameters();
        }

        private void FixedUpdate()
        {
            if (isDead || dialogueSystem.isDialogRunning)
                return;

            if (playerAnimationController.isAttacking) return;

            if (isSwimming)
            {
                HandleSwimmingMovement();
            } else
            {
                HandleGroundMovement();
            }
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

        private void HandleSwimmingMovement()
        {
            CurrentSpeed = swimSpeed;

            // Проверяем, достигли ли мы поверхности воды
            bool atWaterSurface = transform.position.y >= GetWaterSurfaceLevel() - waterSurfaceLevelOffset;

            // Ограничиваем движение вверх у поверхности воды
            float verticalInput = atWaterSurface && inputHandler.MoveInput.y > 0 ? 0 : inputHandler.MoveInput.y;

            Vector2 swimDirection = new Vector2(inputHandler.MoveInput.x, verticalInput).normalized;

            // Добавляем силу движения
            rb.AddForce(swimDirection * swimSpeed, ForceMode2D.Force);

            // Ограничиваем максимальную скорость
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -swimSpeed, swimSpeed),
                Mathf.Clamp(rb.linearVelocity.y, -swimSpeed, swimSpeed)
            );

            // Если мы у поверхности и стоим на земле, проверяем возможность выхода
            if (atWaterSurface && IsGrounded)
            {
                CheckWaterExit();
            }
        }

        private float GetWaterSurfaceLevel()
        {
            return waterCollider.bounds.max.y;
        }

        private void CheckWaterExit()
        {
            // Проверяем, есть ли земля над нами, чтобы выйти из воды
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.up,
                capsuleCollider.size.y * 0.6f,
                LayerMask.GetMask("Ground", "Platform"));

            if (hit.collider == null)
            {
                ExitWater();
            }
        }

        private void HandleJump()
        {
            if (isSwimming) return;

            if (inputHandler.JumpPressed && (IsGrounded || !isDoubleJumping))
            {
               playerAnimationController.SetTrigger("Jump");
               rb.linearVelocity = Vector2.zero;
               rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
               isDoubleJumping = !isDoubleJumping;
            }
        }

        private void EnterWater()
        {
            rb.gravityScale = 0.5f;
            StartCoroutine(WaitForDive());
        }

        private IEnumerator WaitForDive()
        {
            while (transform.position.y >= (GetWaterSurfaceLevel() - waterSurfaceLevelOffset) / 2)
            {
                yield return new WaitForEndOfFrame();
            }
            isSwimming = true;
            rb.linearDamping = 5f;
            playerAnimationController.SetBool("isSwimming", true);
            rb.gravityScale = 0;
        }

        private void ExitWater()
        {
            isSwimming = false;
            rb.gravityScale = 1;
            rb.linearDamping = 0;
            playerAnimationController.SetBool("isSwimming", false);
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

            Health--;

            if (Health <= 0)
            {
                playerAnimationController.SetTrigger(AnimationController.IS_DEAD_S);
                isDead = true;
                rb.gravityScale = 1;

                StartCoroutine(ReviveCoroutine());
            }
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
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("Bullet"))
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
                    isDoubleJumping = false;
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
            if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                waterCollider = other;
                EnterWater();
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                if (!isDead)
                    Hit();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                ExitWater();
            }
        }
    }
}