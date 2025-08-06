using UnityEngine;

namespace Animations
{
    public class AnimationController : MonoBehaviour
    {
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;

        [HideInInspector] public bool isAttacking = false;
        [HideInInspector] public bool isRolling = false;

        public static string ATTACK_S = "Attack";
        public static string SPEED_S = "Speed";
        public static string IS_DEAD_S = "isDead";
        public static string REVIVE_S = "Revive";
        public static string IS_OPEN_S = "isOpen";
        public static string IS_ACTIVE_S = "isActive";
        public static string ROLL_S = "Roll";
    }
}