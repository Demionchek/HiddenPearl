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
        public bool Spell1Action { get; private set; }

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;
        private InputAction dodgeAction;
        private InputAction duckAction;
        private InputAction spell1Action;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            attackAction = playerInput.actions["Attack"];
            dodgeAction = playerInput.actions["Dodge"];
            duckAction = playerInput.actions["Duck"];
            spell1Action = playerInput.actions["Spell1"];
        }

        public bool InputEnabled { get; private set; } = true;

        public void SetInputEnabled(bool enabled)
        {
            InputEnabled = enabled;
            if (!enabled)
            {
                MoveInput = Vector2.zero;
                JumpPressed = false;
                AttackPressed = false;
                DodgeAction = false;
                DuckAction = false;
                Spell1Action = false;
            }
        }

        private void Update()
        {
            if (!InputEnabled) return;
            MoveInput = moveAction.ReadValue<Vector2>();
            DuckAction = duckAction.IsPressed();
        }

        private void LateUpdate()
        {
            // Автоматический сброс триггеров в конце кадра
            JumpPressed = false;
            AttackPressed = false;
            DodgeAction = false;
            Spell1Action = false;
        }


        private void OnJump(InputValue context)
        {
            if (InputEnabled && context.isPressed)
                JumpPressed = true;
        }

        private void OnAttack(InputValue context)
        {
            if (InputEnabled && context.isPressed)
                AttackPressed = true;
        }

        private void OnDodge(InputValue context)
        {
            if (InputEnabled && context.isPressed)
                DodgeAction = true;
        }

        private void OnSpell1(InputValue context)
        {
            if (InputEnabled && context.isPressed)
                Spell1Action = true;
        }
    }
}