using System;
using System.Collections;
using Animations;
using Camera;
using DefaultNamespace;
using Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IHittable, IHealth
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float jumpForce = 2.5f;
        [SerializeField] private float rollForce = 1.5f;
        [SerializeField] private float interactDelay = 0.5f;
        [SerializeField] private Vector2 groundColliderSize;
        [SerializeField] private Vector2 groundColliderOffset;
        public float wallDetectOffset = 0.1f;

        [Header("Swimming Settings")]
        [SerializeField] private float swimSpeed = 3f;
        [SerializeField] private float swimUpForce = 1f;
        [SerializeField] private float waterSurfaceLevelOffset = 3.5f;
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private Vector2 swimColliderSize;
        [SerializeField] private Vector2 swimColliderOffset;

        [Header("Climbing Settings")]
        [SerializeField] private float climbSpeed = 2f;
        [SerializeField] private Vector2 climbColliderSize;
        [SerializeField] private Vector2 climbColliderOffset;

        [Header("Other Settings")]
        [SerializeField] private bool reloadLevelOnDeath = false;
        [SerializeField] private int Health;
        [SerializeField] private int MaxHealth;
        [SerializeField] private OxygenController oxygenController;
        [SerializeField] private RandomSoundPlayer attackPlayerSound;
        [SerializeField] private RandomSoundPlayer hitPlayerSound;
        [SerializeField] private RandomSoundPlayer jumpPlayerSound;
        [SerializeField] private RandomSoundPlayer doubleJumpPlayerSound;
        [SerializeField] private RandomSoundPlayer deathPlayerSound;
        [SerializeField] private Image fadeScreen;

        [Inject]
        private InputHandler inputHandler;

        [Inject]
        private DialogueSystem dialogueSystem;

        [Inject]
        private CheckPoints checkPoints;

        [Inject]
        private CameraController cameraController;

        [Inject]
        private TimelineManager timelineManager;

        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private BoxCollider2D boxTriggerCollider;
        private PlayerAnimationController animController;
        private Collider2D waterCollider;
        private Collider2D climbCollider;

        private int currentAttackIndex = -1;
        public bool isDead { get; private set; }
        private bool isFlip = false;
        private bool isDoubleJumping = false;
        private bool isRolling = false;
        private bool isClimbing = false;
        public Action<int> healthChanged { get; set; }

        public int GetHealth()
        {
            return Health;
        }

        public int GetMaxHealth()
        {
            return MaxHealth;
        }

        public void IncreaseHealth(int amount)
        {
            MaxHealth += amount;
            SetHealth(MaxHealth);
        }

        public void SetHealth(int health)
        {
            Health = health;
            healthChanged?.Invoke(Health);
        }

        public bool IsGrounded { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool isDiving { get; private set; }
        private bool _previousIsDiving;
        public Vector2 Velocity => rb.linearVelocity;

        private Vector2 effectorVelocity = Vector2.zero;
        private bool isSwimming;

        public static PlayerController Instance { get; private set; }

        public event Action OnRevive;
        public event Action OnDeath;

        public Action<bool> hasDive { get; set; }

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            boxTriggerCollider = GetComponent<BoxCollider2D>();
            animController = GetComponent<PlayerAnimationController>();
            SetHealth(MaxHealth);
        }

        private void Update()
        {
            isFlip = animController.IsSpriteFliped();
            UpdateAnimationParameters();

            if (animController.isAttacking || isDead || dialogueSystem.isDialogRunning || animController.isRolling) return;

            HandleAttack();
            HandleJump();
            HandleRoll();
            HandleClimb();
        }

        private void FixedUpdate()
        {
            if (isDead || dialogueSystem.isDialogRunning || animController.isRolling || timelineManager.IsCutscenePlaying())
                return;

            if (isClimbing)
            {
                HandleClimbingMovement();
            }
            else if (isSwimming)
            {
                HandleSwimmingMovement();
            }
            else
            {
                HandleGroundMovement();
            }
        }

        private void HandleGroundMovement()
        {
            CurrentSpeed = speed;

            bool isWallInFront = false;
            bool isSteepSlope = false;

            if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            {
                float direction = Mathf.Sign(inputHandler.MoveInput.x);

                Vector2 capsuleSize = new Vector2(
                    capsuleCollider.size.x * 0.3f,
                    capsuleCollider.size.y * 0.7f
                );

                Vector2 capsuleOffset = new Vector2(
                    direction * (capsuleCollider.size.x * 0.5f + capsuleSize.x * 0.5f),
                    capsuleCollider.offset.y
                );

                Vector2 capsulePosition = (Vector2)transform.position + capsuleOffset;

                RaycastHit2D capsuleHit = Physics2D.CapsuleCast(
                    capsulePosition,
                    capsuleSize,
                    CapsuleDirection2D.Vertical,
                    0f,
                    Vector2.right * direction,
                    0.1f,
                    LayerMask.GetMask("Ground", "Wall", "Platform", "Enemy")
                );

                if (capsuleHit.collider != null)
                {
                    float surfaceAngle = Vector2.Angle(capsuleHit.normal, Vector2.up);
                    float playerBottom = transform.position.y + capsuleCollider.offset.y - capsuleCollider.size.y * 0.5f;
                    float surfaceHeight = capsuleHit.point.y;
                    float heightDifference = surfaceHeight - playerBottom;
                    float maxAllowedHeight = capsuleCollider.size.y * 0.25f;

                    if (surfaceAngle <= 46f && heightDifference <= maxAllowedHeight)
                    {
                        isWallInFront = false;
                    }
                    else if (surfaceAngle > 46f || heightDifference > maxAllowedHeight)
                    {
                        isWallInFront = true;
                        isSteepSlope = surfaceAngle > 46f;
                    }

                    Debug.Log($"Surface angle: {surfaceAngle}°, Height diff: {heightDifference}, Allowed: {maxAllowedHeight}");
                }

                DrawDebugCapsule(capsulePosition, capsuleSize, CapsuleDirection2D.Vertical,
                    isWallInFront ? Color.red : Color.yellow);
            }

            float horizontalVelocity = isWallInFront ? 0 : inputHandler.MoveInput.x * CurrentSpeed;
            rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y) + effectorVelocity;
        }

        private void DrawDebugCapsule(Vector2 position, Vector2 size, CapsuleDirection2D direction, Color color)
        {
            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;

            Vector2 topLeft = position + new Vector2(-halfWidth, halfHeight);
            Vector2 topRight = position + new Vector2(halfWidth, halfHeight);
            Vector2 bottomLeft = position + new Vector2(-halfWidth, -halfHeight);
            Vector2 bottomRight = position + new Vector2(halfWidth, -halfHeight);

            Debug.DrawLine(topLeft, topRight, color);
            Debug.DrawLine(topRight, bottomRight, color);
            Debug.DrawLine(bottomRight, bottomLeft, color);
            Debug.DrawLine(bottomLeft, topLeft, color);
        }

        private void HandleSwimmingMovement()
        {
            CurrentSpeed = swimSpeed;

            bool atWaterSurface = transform.position.y >= GetWaterSurfaceLevel() - waterSurfaceLevelOffset;

            _previousIsDiving = isDiving;
            isDiving = !atWaterSurface;

            if (_previousIsDiving != isDiving)
            {
                hasDive?.Invoke(isDiving);
            }

            float verticalInput = atWaterSurface && inputHandler.MoveInput.y > 0 ? 0 : inputHandler.MoveInput.y;

            Vector2 swimDirection = new Vector2(inputHandler.MoveInput.x, verticalInput).normalized;

            SwitchColliderSwimming(swimDirection.x > 0.1f);

            rb.AddForce(swimDirection * swimSpeed, ForceMode2D.Force);

            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -swimSpeed, swimSpeed),
                Mathf.Clamp(rb.linearVelocity.y, -swimSpeed, swimSpeed)
            );

            if (atWaterSurface && IsGrounded)
            {
                CheckWaterExit();
            }
        }

        private void HandleClimbingMovement()
        {
            CurrentSpeed = climbSpeed;

            Vector2 climbDirection = new Vector2(inputHandler.MoveInput.x, inputHandler.MoveInput.y).normalized;

            //SwitchColliderClimbing(climbDirection.x > 0.1f);

            rb.AddForce(climbDirection * climbSpeed, ForceMode2D.Force);
            rb.linearVelocity = new Vector2(
                Mathf.Clamp(rb.linearVelocity.x, -climbSpeed, climbSpeed),
                Mathf.Clamp(rb.linearVelocity.y, -climbSpeed, climbSpeed)
            );

            if (inputHandler.MoveInput.magnitude < 0.1f) rb.linearVelocity = Vector2.zero;

            float climbSurfaceLevel = climbCollider.bounds.max.y;
            if (transform.position.y >= climbSurfaceLevel - capsuleCollider.size.y && inputHandler.MoveInput.y > 0)
            {
                animController.SetTrigger("Pullup");
                rb.linearVelocity = Vector2.zero;
            }

            if (IsGrounded)
            {
                ExitClimbing();
            }
        }

        private void SwitchColliderClimbing(bool isMovingHorizontally)
        {
            if (isMovingHorizontally)
            {
                capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                capsuleCollider.size = climbColliderSize;
                capsuleCollider.offset = climbColliderOffset;
                boxTriggerCollider.size = climbColliderSize;
                boxTriggerCollider.offset = climbColliderOffset;
            }
            else
            {
                capsuleCollider.direction = CapsuleDirection2D.Vertical;
                capsuleCollider.size = groundColliderSize;
                capsuleCollider.offset = groundColliderOffset;
                boxTriggerCollider.size = groundColliderSize;
                boxTriggerCollider.offset = groundColliderOffset;
            }
        }

        private void SwitchColliderSwimming(bool isSwimming)
        {
            if (isSwimming)
            {
                capsuleCollider.direction = CapsuleDirection2D.Horizontal;
                capsuleCollider.size = swimColliderSize;
                capsuleCollider.offset = swimColliderOffset;
                boxTriggerCollider.size = swimColliderSize;
                boxTriggerCollider.offset = swimColliderOffset;
            }
            else
            {
                capsuleCollider.direction = CapsuleDirection2D.Vertical;
                capsuleCollider.size = groundColliderSize;
                capsuleCollider.offset = groundColliderOffset;
                boxTriggerCollider.size = groundColliderSize;
                boxTriggerCollider.offset = groundColliderOffset;
            }
        }

        private float GetWaterSurfaceLevel()
        {
            return waterCollider.bounds.max.y;
        }

        private void CheckWaterExit()
        {
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
            if (isSwimming || isClimbing) return;

            if (inputHandler.JumpPressed && (IsGrounded || !isDoubleJumping))
            {
                animController.SetTrigger("Jump");
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                isDoubleJumping = !isDoubleJumping;

                if (!isDoubleJumping)
                {
                    if (jumpPlayerSound != null)
                        jumpPlayerSound.PlayRandomSoundNow();
                }
                else
                {
                    if (doubleJumpPlayerSound != null)
                        doubleJumpPlayerSound.PlayRandomSoundNow();
                }
            }
        }

        private void HandleClimb()
        {
            if (isSwimming || isDead || dialogueSystem.isDialogRunning || animController.isRolling) return;

            if (inputHandler.MoveInput.y > 0.1f && climbCollider != null && !isClimbing)
            {
                EnterClimbing();
            }
        }

        private void EnterClimbing()
        {
            isClimbing = true;
            rb.gravityScale = 0;
            rb.linearDamping = 5f;
            animController.SetBool("isClimbing", true);
        }

        private void ExitClimbing()
        {
            isClimbing = false;
            rb.gravityScale = 1;
            rb.linearDamping = 0;
            animController.SetBool("isClimbing", false);
            //SwitchColliderClimbing(false);
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
            animController.SetBool("isSwimming", true);
            rb.gravityScale = 0;

            if (!isDiving)
            {
                isDiving = true;
                hasDive?.Invoke(true);
            }
        }

        private void ExitWater()
        {
            isSwimming = false;
            rb.gravityScale = 1;
            rb.linearDamping = 0;
            animController.SetBool("isSwimming", false);

            SwitchColliderSwimming(false);

            if (isDiving)
            {
                isDiving = false;
                hasDive?.Invoke(false);
            }
        }

        private void HandleAttack()
        {
            if (inputHandler.AttackPressed)
            {
                if (rb.linearVelocity.magnitude > 0.1f)
                {
                    animController.SetInt(AnimationController.ATTACK_S, 3);
                }
                else
                {
                    int attackIndex = currentAttackIndex == 1 ? 2 : 1;
                    animController.SetInt(AnimationController.ATTACK_S, attackIndex);
                    currentAttackIndex = attackIndex;
                }

                animController.isAttacking = true;

                StartCoroutine(PlayAttackSoundWithDelay());
            }
        }

        private void HandleRoll()
        {
            if (inputHandler.DodgeAction && !animController.isRolling && IsGrounded && !isSwimming && !isClimbing)
            {
                animController.isRolling = true;

                StartCoroutine(RollCoroutine());
            }
        }

        private IEnumerator RollCoroutine()
        {
            animController.SetTrigger(AnimationController.ROLL_S);
            Vector2 direction = animController.IsSpriteFliped() ? -Vector2.right : Vector2.right;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direction * rollForce, ForceMode2D.Impulse);
            gameObject.layer = LayerMask.NameToLayer("Roll");

            while (animController.isRolling)
                yield return new WaitForEndOfFrame();

            gameObject.layer = LayerMask.NameToLayer("Player");
        }

        private void CastAttack()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-0.1f, 0) : new Vector2(0.1f, 0));
            origin += direction;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin, radius);

            if (hitColliders.Length == 0)
            {
                Debug.Log("В радиусе нет объектов.");
                return;
            }

            foreach (Collider2D collider in hitColliders)
            {
                if (collider.gameObject.layer == LayerMask.NameToLayer("Player")) continue;
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

        private IEnumerator PlayAttackSoundWithDelay()
        {
            yield return new WaitForSeconds(0.1f);
            if (attackPlayerSound != null)
                attackPlayerSound.PlayRandomSoundNow();
        }

        private void OnDrawGizmos()
        {
            Vector2 origin = transform.position + new Vector3(0, 0.15f, 0);
            float radius = 0.2f;
            float distance = 0.02f;
            Vector2 direction = (isFlip ? new Vector2(-0.25f, 0) : new Vector2(0.25f, 0));
            origin += direction;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, radius);
        }

        private void UpdateAnimationParameters()
        {
            if (dialogueSystem.isDialogRunning)
            {
                animController.SetMovementParameters(0, IsGrounded);
                return;
            }

            animController.SetMovementParameters(
                Mathf.Abs(inputHandler.MoveInput.x),
                IsGrounded
            );
        }

        public void Hit()
        {
            if (isDead || dialogueSystem.isDialogRunning || animController.isRolling) return;

            SetHealth(Health - 1);

            if (Health <= 0)
            {
                animController.SetTrigger(AnimationController.IS_DEAD_S);
                isDead = true;
                rb.gravityScale = 1;
                if (deathPlayerSound != null)
                    deathPlayerSound.PlayRandomSoundNow();

                if (reloadLevelOnDeath)
                {
                    StartCoroutine(ReloadLevel());
                } else
                {
                    StartCoroutine(ReviveCoroutine());
                }
            }
            else
            {
                if (hitPlayerSound != null)
                    hitPlayerSound.PlayRandomSoundNow();
            }
        }

        public void Kill()
        {
            SetHealth(0);
            animController.SetTrigger(AnimationController.IS_DEAD_S);
            isDead = true;
            rb.gravityScale = 1;

            if (deathPlayerSound != null)
                deathPlayerSound.PlayRandomSoundNow();

            StartCoroutine(ReloadLevel());
        }

        public void PullUp()
        {
            if (climbCollider != null)
            {
                transform.position = new Vector3(transform.position.x, climbCollider.bounds.max.y + capsuleCollider.size.y * 0.5f, transform.position.z);
                ExitClimbing();
            }
        }

        private IEnumerator ReloadLevel()
        {
            yield return new WaitForSeconds(2f);
            int index = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(index);
        }

        private IEnumerator ReviveCoroutine()
        {
            OnDeath?.Invoke();

            fadeScreen.gameObject.SetActive(true);
            while (fadeScreen.color.a < 1f)
            {
                var color = fadeScreen.color;
                color.a = color.a + 0.05f;
                fadeScreen.color = color;
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(1f);

            isDead = false;
            transform.position = checkPoints.CurrentCheckPoint.position;
            animController.SetTrigger(AnimationController.REVIVE_S);
            SetHealth(MaxHealth);
            healthChanged?.Invoke(Health);

            while (fadeScreen.color.a > 0f)
            {
                var color = fadeScreen.color;
                color.a = color.a - 0.05f;
                fadeScreen.color = color;
                yield return new WaitForSeconds(0.2f);
            }

            fadeScreen.gameObject.SetActive(false);

            OnRevive?.Invoke();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    IsGrounded = true;
                    isDoubleJumping = false;
                    if (isClimbing)
                    {
                        ExitClimbing();
                    }
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

            if (other.gameObject.layer == LayerMask.NameToLayer("Climb"))
            {
                climbCollider = other;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Hide"))
            {
                cameraController.SwitchVolumeWeight(0.75f);
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Oxygen"))
            {
                oxygenController.FillOxygen();
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                bool atWaterSurface = transform.position.y <= GetWaterSurfaceLevel();

                if (!isSwimming && !atWaterSurface)
                {
                    rb.gravityScale = 0.5f;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                ExitWater();
                cameraController.SwitchVolumeWeight(0);
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Climb"))
            {
                climbCollider = null;
                if (isClimbing)
                {
                    ExitClimbing();
                }
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Hide"))
            {
                cameraController.SwitchVolumeWeight(0);
            }
        }
    }
}