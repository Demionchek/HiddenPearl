using Animations;
using Player;
using UnityEngine;

namespace AI.States
{
    public class RollStateMachine : StateMachineBehaviour
    {
        private AnimationController animationController;
        private PlayerController playerController;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animationController = animator.GetComponent<AnimationController>();
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animationController.isRolling = false;
        }

    }
}