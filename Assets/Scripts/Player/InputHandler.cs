using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    public class InputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool BarkPressed { get; private set; }
        public bool ShapeDog { get; private set; }
        public bool ShapeBird { get; private set; }
        public bool ShapeRat { get; private set; }

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction barkAction;
        private InputAction shapeDogAction;
        private InputAction shapeBirdAction;
        private InputAction shapeRatAction;

        private void Awake()
        {
            var playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            barkAction = playerInput.actions["Bark"];
            shapeDogAction = playerInput.actions["ShapeDog"];
            shapeBirdAction = playerInput.actions["ShapeBird"];
            shapeRatAction = playerInput.actions["ShapeRat"];
        }

        private void Update()
        {
            MoveInput = moveAction.ReadValue<Vector2>();
        }

        private void LateUpdate()
        {
            // Автоматический сброс триггеров в конце кадра
            JumpPressed = false;
            BarkPressed = false;
            ShapeDog = false;
            ShapeBird = false;
            ShapeRat = false;
        }

        private void OnJump(InputValue context)
        {
            if (context.isPressed) JumpPressed = true;
        }

        private void OnBark(InputValue context)
        {
            if (context.isPressed) BarkPressed = true;
        }

        private void OnShapeDog(InputValue context)
        {
            if (context.isPressed) ShapeDog = true;
        }

        private void OnShapeBird(InputValue context)
        {
            if (context.isPressed) ShapeBird = true;
        }
        private void OnShapeRat(InputValue context)
        {
            if (context.isPressed) ShapeRat = true;
        }
    }
}