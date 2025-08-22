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

            Vector2 targetPos = baseEnemy.canSeeTarget ? (Vector2)baseEnemy.target.position : baseEnemy.lastKnownPosition;
            Vector2 direction = targetPos - (Vector2)baseEnemy.transform.position;
            float distance = direction.magnitude;

            if (distance > baseEnemy.stoppingDistance)
            {
                direction.Normalize();
                baseEnemy.rb.linearVelocity = direction * baseEnemy.speed;
            }
            else
            {
                RaycastHit2D[] hits = Physics2D.CircleCastAll(baseEnemy.transform.position, 0.5f, Vector2.up);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        baseEnemy.canSeeTarget = true;
                    }
                }
                
                baseEnemy.rb.linearVelocity = Vector2.zero;
                if (!baseEnemy.canSeeTarget)
                {
                    baseEnemy.isChasing = false;
                    baseEnemy.ChangeState<PatrolStateAI>();
                }
            }
            animatonController.SetAnimatorFloat(AnimationController.SPEED_S, 1f);

            animatonController.GetSpriteRenderer().flipX = direction.x < 0;
        }
    }
}