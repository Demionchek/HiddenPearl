using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool AttackPressed { get; private set; }
        public bool DodgeAction { get; private set; }
        public bool DuckAction { get; private set; }

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;
        private InputAction dodgeAction;
        private InputAction duckAction;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            attackAction = playerInput.actions["Attack"];
            dodgeAction = playerInput.actions["Dodge"];
            duckAction = playerInput.actions["Duck"];
        }

        private void Update()
        {
            MoveInput = moveAction.ReadValue<Vector2>();
            DuckAction = duckAction.IsPressed();
            Debug.Log(DuckAction);
        }

        private void LateUpdate()
        {
            // Автоматический сброс триггеров в конце кадра
            JumpPressed = false;
            AttackPressed = false;
            DodgeAction = false;
        }


        private void OnJump(InputValue context)
        {
            if (context.isPressed)
            {
                JumpPressed = true;
            }
        }

        private void OnAttack(InputValue context)
        {
            if (context.isPressed)
            {
                AttackPressed = true;
            }
        }

        private void OnDodge(InputValue context)
        {
            if (context.isPressed)
            {
                DodgeAction = true;
            }
        }
    }
}