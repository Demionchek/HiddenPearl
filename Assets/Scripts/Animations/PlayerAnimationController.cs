using DefaultNamespace;
using Player;
using Zenject;
using UnityEngine;

namespace Animations
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class PlayerAnimationController : AnimationController
    {
        private PlayerController player;

        [Inject]
        private InputHandler inputHandler;
        [Inject]
        private DialogueSystem dialogSystem;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if(dialogSystem.isDialogRunning) return;
            UpdateSpriteDirection();
        }

        public void SetMovementParameters(float speed, bool isGrounded)
        {
            animator.SetFloat(SPEED_S, speed);
            animator.SetBool("IsGrounded", isGrounded);
        }

        public void SetTrigger(string trigger) => animator.SetTrigger(trigger);
        public void ResetTrigger(string trigger) => animator.ResetTrigger(trigger);
        public void SetBool(string boolName, bool value) => animator.SetBool(boolName, value);
        public void SetInt(string intName, int value) => animator.SetInteger(intName, value);

        private void UpdateSpriteDirection()
        {
            if (inputHandler.MoveInput.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (inputHandler.MoveInput.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }

        public bool IsSpriteFliped() => spriteRenderer.flipX;
    }
}