using System;
using Animations;
using Interfaces;
using UnityEngine;
using Random = System.Random;

namespace AI
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class TriggerAttack : MonoBehaviour
    {
        [SerializeField] private float radius;
        private Collider2D triggerCollider;
        private AudioSource audioSource;
        private Animator animator;
        private bool canAttack = false;
        [SerializeField] private float attackDelay = 1f;
        private float lastAttackTime;

        [SerializeField] private AudioClip[] attackSounds;

        private void Start()
        {
            triggerCollider = GetComponent<CircleCollider2D>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!canAttack && Time.time - lastAttackTime > attackDelay)
                canAttack = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (canAttack && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                canAttack = false;
                animator.SetTrigger(AnimationController.ATTACK_S);
                OnAttack();
                lastAttackTime = Time.time;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (canAttack && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                canAttack = false;
                animator.SetTrigger(AnimationController.ATTACK_S);
                OnAttack();
                lastAttackTime = Time.time;
            }
        }

        protected void OnAttack()
        {
            
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);

            if (hitColliders.Length == 0)
            {
                Debug.Log("В радиусе нет объектов.");
                return;
            }

            // Перебираем все найденные коллайдеры
            foreach (Collider2D collider in hitColliders)
            {
                if (collider == triggerCollider) continue;

                Debug.Log($"Обнаружен объект: {collider.name}");

                // Проверяем, есть ли у него компонент для взаимодействия
                IHittable hittable = collider.GetComponent<IHittable>();
                if (hittable != null)
                {
                    hittable.Hit();
                    break;
                }
            }

            if (attackSounds != null)
                PlaySound(attackSounds);
        }

        private void PlaySound(AudioClip[] clips)
        {
            Random r = new Random();

            int index = r.Next(0, clips.Length);
            
            audioSource.PlayOneShot(clips[index]);
        }
    }
}