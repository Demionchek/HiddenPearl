using AI.States;
using Animations;
using UnityEngine;

namespace AI.States
{
    public class AttackStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            animatonController.SetAnimatorFloat(AnimationController.SPEED_S, 0);
            baseEnemy.rb.linearVelocity = Vector2.zero;
        }
        public override void ExitState() { }

        public override void StateUpdate()
        {
            if (baseEnemy.target != null)
            {
                Vector2 direction = baseEnemy.target.position - baseEnemy.transform.position;
                float distance = direction.magnitude;

                if (distance > baseEnemy.stoppingDistance && !animatonController.isAttacking)
                {
                    direction.Normalize();
                    baseEnemy.rb.linearVelocity = direction * baseEnemy.speed;
                    baseEnemy.canAttack = false;
                    baseEnemy.ChangeState<ChaseStateAI>();
                    return;
                }
            }

            if (!animatonController.isAttacking && !baseEnemy.canSeeTarget)
            {
                if (baseEnemy.isChasing)
                {
                    baseEnemy.ChangeState<ChaseStateAI>();
                }
                else
                {
                    baseEnemy.ChangeState<IdleStateAI>();
                }
            }
            
            bool canAttack = !animatonController.isAttacking && 
                             Time.time > baseEnemy.lastAttackTime + baseEnemy.attackDelay;

            if (canAttack)
            {
                AttackTrigger();
            }
        }

        private void AttackTrigger()
        {
            animatonController.SetAnimatorTrigger(AnimationController.ATTACK_S);
            animatonController.isAttacking = true;
            baseEnemy.lastAttackTime = Time.time;
            baseEnemy.savedHitPosition = baseEnemy.target.position;
            if (baseEnemy.target != null && baseEnemy.canSeeTarget)
            {
                Vector2 dir = baseEnemy.target.transform.position - baseEnemy.transform.position;
                animatonController.GetSpriteRenderer().flipX = dir.x < 0;
            }
        }

        public override void StateFixedUpdate() { }
    }
}