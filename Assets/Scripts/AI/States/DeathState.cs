using Animations;
using UnityEngine;

namespace AI.States
{
    public class DeathState : BaseStateAI
    {
        public override void EnterState()
        {
            animatonController.SetAnimatorTrigger(AnimationController.IS_DEAD_S);
            baseEnemy.capsule.enabled = false;
            baseEnemy.rb.gravityScale = 0;
            baseEnemy.rb.linearVelocity = Vector2.zero;
        }
    }
}