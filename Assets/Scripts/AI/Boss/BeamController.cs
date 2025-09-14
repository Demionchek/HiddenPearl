using System.Collections.Generic;
using Interfaces;
using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace AI.BossPatterns
{

    public class BeamController : MonoBehaviour
    {
        [Header("References")]
        public GameObject beamChildObject; // Дочерний объект с лучом
        public Transform beamTransform; // Transform дочернего луча для анимации
        public Collider2D beamCollider; // Коллайдер луча для обнаружения попаданий

        [Header("Beam Settings")]
        public float damageInterval = 0.1f; // Интервал между нанесением урона

        private Animator animator;
        private bool isBeamActive = false;
        private Vector3 originalBeamScale;
        private float lastDamageTime;
        private HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Метод для запуска луча из внешнего кода
        public void Initialize()
        {
            gameObject.SetActive(true);
            damagedTargets.Clear();
        }


        // Обработка попаданий через коллайдер
        void OnTriggerEnter2D(Collider2D other)
        {
            HandleDamage(other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            // Наносим урон с интервалом
            if (Time.time - lastDamageTime >= damageInterval)
            {
                HandleDamage(other);
                lastDamageTime = Time.time;
            }
        }

        private void HandleDamage(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                // Проверяем, чтобы не наносить урон несколько раз за один "тик"
                // если объект уже получил урон в этом интервале
                if (damagedTargets.Contains(other))
                    return;

                IHittable playerHealth = other.GetComponent<IHittable>();
                if (playerHealth != null)
                {
                    playerHealth.Hit();
                    damagedTargets.Add(other);

                    // Очищаем хэшсет через небольшое время
                    StartCoroutine(ClearDamagedTarget(other));
                }
            }
        }

        private IEnumerator ClearDamagedTarget(Collider2D target)
        {
            yield return new WaitForSeconds(damageInterval * 1.1f);
            damagedTargets.Remove(target);
        }

        // Очистка при уничтожении
        void OnDestroy()
        {
            if (beamTransform != null)
            {
                beamTransform.DOKill();
            }
        }

    }
}