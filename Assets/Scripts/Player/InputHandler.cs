using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    public class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            attackAction = playerInput.actions["Attack"];
        }

        private void Update()
        {
            MoveInput = moveAction.ReadValue<Vector2>();
        }

        private void LateUpdate()
        {
            // Автоматический сброс триггеров в конце кадра
            JumpPressed = false;
            AttackPressed = false;
        }

        private void OnJump(InputValue context)
        {
            if (context.isPressed) JumpPressed = true;
        }

        private void OnФеефсл(InputValue context)
        {
            if (context.isPressed) AttackPressed = true;
        }
    }
}