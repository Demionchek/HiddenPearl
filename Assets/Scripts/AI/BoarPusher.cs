using Unity.Cinemachine;
using UnityEngine;

namespace AI
{
    public class BoarPusher : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float pushForce = 10f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private Transform player;

        [Header("Dash Settings")]
        [SerializeField] private float dashDistance = 3f;
        [SerializeField] private float dashDuration = 0.3f;

        [SerializeField] private float returnDuration = 0.5f;

        // References
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Collider2D collider2D;
        private AudioSource audioSource;

        // State variables
        private bool isAttacking = false;
        private bool isDashing = false;
        private bool isReturning = false;
        private float lastAttackTime = 0f;
        private Vector2 originalPosition;
        private Vector2 dashDirection;
        private bool playerInRange = false;
        private float dashTimer = 0f;
        private float returnTimer = 0f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            collider2D = GetComponent<Collider2D>();
            audioSource = GetComponent<AudioSource>();

        }

        private void Update()
        {
            if (isAttacking) return;

            // Update sprite direction based on player position
            UpdateSpriteDirection();

            // Check if can attack based on cooldown and player in range
            if (Time.time - lastAttackTime >= attackCooldown && playerInRange)
            {
                StartAttack();
            }
        }

        private void FixedUpdate()
        {
            if (!isAttacking)
            {
                CheckForPlayer();
            }
        }

        private void LateUpdate()
        {
            HandleMovement();
        }

        private void CheckForPlayer()
        {
            // Calculate ray origin from collider center
            Vector2 rayOrigin = collider2D.bounds.center;

            // Determine ray direction based on sprite flip
            Vector2 rayDirection = spriteRenderer.flipX ?  Vector2.right : Vector2.left;

            // Perform raycast in FixedUpdate for physics consistency
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, attackRange, playerLayer);

            // Debug visualization
            Debug.DrawRay(rayOrigin, rayDirection * attackRange, Color.red, Time.fixedDeltaTime);

            playerInRange = hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        private void UpdateSpriteDirection()
        {
            if (player != null)
            {
                // Flip sprite based on player position
                spriteRenderer.flipX = player.position.x > transform.position.x;
            }
        }

        private void HandleMovement()
        {
            if (isDashing)
            {
                HandleDashMovement();
            } else if (isReturning)
            {
                HandleReturnMovement();
            }
        }

        private void StartAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            playerInRange = false; // Reset until next check

            // Trigger attack animation
            animator.SetTrigger("Attack");
        }

        // This method should be called from animation event
        public void OnAttack()
        {
            StartDash();
            audioSource.Play();
        }

        private void StartDash()
        {
            isDashing = true;
            isReturning = false;
            dashTimer = 0f;
            originalPosition = transform.position;

            // Determine dash direction based on sprite flip
            dashDirection = spriteRenderer.flipX ? Vector2.right : Vector2.left;
        }

        private void HandleDashMovement()
        {
            dashTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dashTimer / dashDuration);

            // Smooth movement using Lerp
            Vector2 targetPosition = originalPosition + dashDirection * dashDistance;
            transform.position = Vector2.Lerp(originalPosition, targetPosition, progress);

            // Check if dash completed
            if (dashTimer >= dashDuration)
            {
                EndDash();
            }
        }

        private void EndDash()
        {
            isDashing = false;
            isReturning = true;
            returnTimer = 0f;
        }

        private void HandleReturnMovement()
        {
            returnTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(returnTimer / returnDuration);

            // Smooth return to original position
            Vector2 currentDashPosition = originalPosition + dashDirection * dashDistance;
            transform.position = Vector2.Lerp(currentDashPosition, originalPosition, progress);

            // Check if return completed
            if (returnTimer >= returnDuration)
            {
                EndReturn();
            }
        }

        private void EndReturn()
        {
            isReturning = false;
            isAttacking = false;

            // Ensure exact position to avoid floating point errors
            transform.position = originalPosition;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.rigidbody != null)
            {
                collision.rigidbody.AddForce(dashDirection * pushForce, ForceMode2D.Impulse);
            }
        }

        // Gizmos for visualization
        private void OnDrawGizmosSelected()
        {
            if (collider2D == null)
                collider2D = GetComponent<Collider2D>();

            if (collider2D != null)
            {
                Vector2 rayOrigin = collider2D.bounds.center;
                Vector2 rayDirection = Application.isPlaying ? (spriteRenderer != null && spriteRenderer.flipX ? Vector2.right : Vector2.left) : Vector2.left;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(rayOrigin, rayDirection * attackRange);

                // Draw attack range sphere
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(rayOrigin, 0.1f);

                // Draw dash distance
                if (Application.isPlaying && isAttacking)
                {
                    Gizmos.color = Color.blue;
                    Vector2 dashEndPosition = originalPosition + dashDirection * dashDistance;
                    Gizmos.DrawLine(originalPosition, dashEndPosition);
                    Gizmos.DrawWireSphere(dashEndPosition, 0.2f);
                }
            }
        }
    }
}