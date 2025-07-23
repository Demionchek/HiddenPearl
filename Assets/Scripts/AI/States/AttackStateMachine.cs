using Animations;
using UnityEngine;

namespace AI.States
{
    public class AttackStateMachine : StateMachineBehaviour
    {
        private AnimationController animationController;

        //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animationController = animator.GetComponent<AnimationController>();
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animationController.isAttacking = false;
        }

    }
}
