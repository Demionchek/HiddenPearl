using Player;

namespace Animations
{
    using UnityEngine;

    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerAnimationController : AnimationController
    {
        private PlayerController player;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            UpdateSpriteDirection();
        }

        public void OnShapeChanged(PlayerController.Shape newShape)
        {
            if (animator == null) return;

            animator.SetInteger("Shape", (int)newShape);
        }

        public void SetMovementParameters(float speed, bool isGrounded)
        {
            animator.SetFloat(SPEED_S, speed);
            animator.SetBool("IsGrounded", isGrounded);
        }

        public void SetTrigger(string trigger) => animator.SetTrigger(trigger);

        private void UpdateSpriteDirection()
        {
            if (player.Velocity.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (player.Velocity.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }

        public bool IsSpriteFliped() => spriteRenderer.flipX;
    }
}