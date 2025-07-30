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

        public void SetMovementParameters(float speed, bool isGrounded)
        {
            animator.SetFloat(SPEED_S, speed);
            animator.SetBool("IsGrounded", isGrounded);
        }

        public void SetTrigger(string trigger) => animator.SetTrigger(trigger);
        public void SetBool(string boolName, bool value) => animator.SetBool(boolName, value);

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