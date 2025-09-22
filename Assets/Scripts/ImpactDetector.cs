using Interfaces;

namespace DefaultNamespace
{
    using UnityEngine;

    public class ImpactDetector : MonoBehaviour
    {
        [Header("Настройки удара")]
        [SerializeField] private float minImpactSpeed = 3f; // Минимальная скорость для регистрации удара

        [SerializeField] private float minImpactForce = 10f; // Минимальная сила удара
        [SerializeField] private LayerMask hitLayers = ~0; // Какие слои могут быть поражены

        [Header("Визуальные эффекты (опционально)")]
        [SerializeField] private ParticleSystem impactParticles;

        [SerializeField] private AudioClip impactSound;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ProcessImpact(collision);
        }

        private void ProcessImpact(Collision2D collision)
        {
            // Проверяем слой
            if (((1 << collision.gameObject.layer) & hitLayers) == 0)
                return;

            // Получаем относительную скорость
            float impactSpeed = collision.relativeVelocity.magnitude;

            // Проверяем минимальную скорость
            if (impactSpeed < minImpactSpeed)
                return;

            // Получаем нормаль удара (направление)
            Vector2 impactDirection = collision.contacts[0].normal;

            // Вычисляем силу удара (можно настроить формулу)
            float impactForce = impactSpeed * 2f;

            if (impactForce < minImpactForce)
                return;

            // Пытаемся получить интерфейс IHittable
            IHittable hittable = collision.gameObject.GetComponent<IHittable>();

            if (hittable != null)
            {
                // Вызываем метод Hit с параметрами
                hittable.Hit();

                // Визуальные и звуковые эффекты
                PlayImpactEffects(collision.contacts[0].point);
            }
        }

        private void PlayImpactEffects(Vector2 position)
        {
            // Воспроизводим частицы
            if (impactParticles != null)
            {
                ParticleSystem particles = Instantiate(impactParticles, position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, particles.main.duration);
            }

            // Воспроизводим звук
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, position);
            }
        }

        // Опционально: для отладки
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}