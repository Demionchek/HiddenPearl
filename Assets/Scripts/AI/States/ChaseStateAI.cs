using Animations;
using UnityEngine;

namespace AI.States
{
    public class ChaseStateAI : BaseStateAI
    {
        public override void EnterState()
        {

        }

        public override void StateFixedUpdate()
        {
            if (baseEnemy.target == null)
                return;

            Vector2 direction = baseEnemy.target.position - baseEnemy.transform.position;
            float distance = direction.magnitude;

            if (distance > baseEnemy.stoppingDistance)
            {
                direction.Normalize();
                baseEnemy.rb.linearVelocity = direction * baseEnemy.speed;
                animatonController.SetAnimatorFloat("Speed", 1f);
            }
            else
            {
                baseEnemy.rb.linearVelocity = Vector2.zero;
                animatonController.SetAnimatorFloat("Speed", 0);
            }
            animatonController.GetSpriteRenderer().flipX = direction.x < 0;
        }
    }
}