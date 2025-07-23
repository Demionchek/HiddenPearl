using System;
using AI.States;
using Interfaces;
using UnityEngine;
using Zenject;

namespace AI
{
    public class LoopedFireManEnemy : BaseEnemy, IHittable
    {
        [SerializeField] private GameObject fireGO;

        [Inject]
        private TimelineManager timelineManager;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            ChangeState<AttackStateAI>();
        }

        private void Update()
        {
            bool isAttackState = currState is AttackStateAI;

            if (loopOverrideState && !isAttackState && !isDead)
            {
                ChangeState<AttackStateAI>();
            }

            currState.StateUpdate();
        }

        public override void ActivateSpecial(bool isActive)
        {
            fireGO.SetActive(isActive);
        }

        public void Hit()
        {
            isDead = true;
            ChangeState<DeathState>();
            PlaySound(deathSound);
            timelineManager.PlayCutscene(2);
        }

        private void PlaySound(AudioClip clip)
        {
            audioSource?.PlayOneShot(clip);
        }
    }
}