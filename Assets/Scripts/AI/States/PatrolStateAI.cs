using System.Collections;
using System.Collections.Generic;
using Animations;
using UnityEngine;

namespace AI.States
{
    public class PatrolStateAI : BaseStateAI
    {
        private Coroutine coroutine;

        public override void StateFixedUpdate()
        {
            if (baseEnemy.patrolPoints.Count < 2)
            {
                baseEnemy.ChangeState<IdleStateAI>();
                return;
            }

            if(baseEnemy.isWaiting) return;

            MoveToPosition();
        }

        public override void ExitState()
        {
            if (coroutine != null)
                baseEnemy.StopCoroutine(coroutine);
            baseEnemy.isWaiting = false;
        }

        private void MoveToPosition()
        {
            Vector2 targetPosition = baseEnemy.patrolPoints[baseEnemy.currentPointIndex].position;
            Vector2 moveDirection = (targetPosition - baseEnemy.rb.position).normalized;

            animatonController.GetSpriteRenderer().flipX = moveDirection.x < 0;

            // Двигаемся с помощью Rigidbody
            baseEnemy.rb.linearVelocity = moveDirection * baseEnemy.speed;
            baseEnemy.AnimationController.SetAnimatorFloat("Speed", 1);

            // Проверяем, достигли ли точки
            if (Vector2.Distance(baseEnemy.rb.position, targetPosition) < baseEnemy.reachedPointDistance)
            {
                coroutine = baseEnemy.StartCoroutine(WaitAtPoint());
            }
        }

        private IEnumerator WaitAtPoint()
        {
            baseEnemy.isWaiting = true;
            baseEnemy.rb.linearVelocity = Vector2.zero;
            baseEnemy.AnimationController.SetAnimatorFloat(AnimationController.SPEED_S, 0);

            yield return new WaitForSeconds(baseEnemy.waitTimeAtPoint);

            GetNextPointIndex();
            baseEnemy.isWaiting = false;
        }

        private void GetNextPointIndex()
        {
            if (baseEnemy.patrolPoints.Count == 0) return;

            if (baseEnemy.patrolPoints.Count == 1)
            {
                baseEnemy.currentPointIndex = 0;
                return;
            }

            baseEnemy.currentPointIndex = (baseEnemy.currentPointIndex + 1) % baseEnemy.patrolPoints.Count;
        }
    }
}