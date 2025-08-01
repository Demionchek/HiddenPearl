using System;
using AI;
using UnityEngine;

namespace Animations
{
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public class EnemyAnimationController : AnimationController
    {
        [HideInInspector] public BaseEnemy baseEnemy;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseEnemy = GetComponent<BaseEnemy>();
        }

        public void SetAnimatorTrigger(string trigger) => animator.SetTrigger(trigger);
        public void SetAnimatorBool(string trigger, bool value) => animator.SetBool(trigger, value);
        public void SetAnimatorFloat(string trigger, float value) => animator.SetFloat(trigger, value);
        public SpriteRenderer GetSpriteRenderer() => spriteRenderer;

        public void SetAnimatorSpeed(float speed) => animator.speed = speed;

        public void OnEnableFire()
        {
            baseEnemy.ActivateSpecial(true);
        }

        public void OnDisableFire()
        {
            baseEnemy.ActivateSpecial(false);
        }
    }
}