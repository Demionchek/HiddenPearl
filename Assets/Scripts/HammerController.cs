using Animations;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class HammerController : MonoBehaviour
    {

        [Header("Timing Settings")]
        [SerializeField] private float activeTime = 1f;    // Время работы молота
        [SerializeField] private float inactiveTime = 2f; // Время простоя молота

        [Header("Attack Settings")]
        [SerializeField] private Vector2 castSize;        // Размер CubeCast (должен соответствовать коллайдеру)
        [SerializeField] private LayerMask hitLayers;     // Слои для атаки

        [SerializeField] private AudioClip hitSound;

        private Animator animator;
        private Collider2D hammerCollider;
        private AudioSource hammerSoundSource;
        private float timer;
        private bool isActive;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            hammerCollider = GetComponent<Collider2D>();
            hammerSoundSource = GetComponent<AudioSource>();

            // Если размер не задан, используем размер коллайдера
            if (castSize == Vector2.zero && hammerCollider != null)
            {
                castSize = hammerCollider.bounds.size;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (isActive && timer >= inactiveTime)
            {
                isActive = false;
                timer = 0f;
                animator.SetBool(AnimationController.ATTACK_S, false);
            }
            else if (!isActive && timer >= activeTime)
            {
                isActive = true;
                timer = 0f;
                animator.SetBool(AnimationController.ATTACK_S, true);
                hammerCollider.enabled = false;
            }
        }

        private void Attack()
        {
            // Получаем центр и поворот коллайдера
            Vector2 center = hammerCollider.bounds.center;
            float angle = -transform.eulerAngles.z; // Получаем угол поворота по оси Z

            Collider2D[] results = new Collider2D[5];

            // Делаем OverlapBox с учетом поворота
            var size = Physics2D.OverlapBoxNonAlloc(center, castSize, angle, results);

            hammerSoundSource.PlayOneShot(hitSound);

            foreach (var collider in results)
            {
                if(collider != null)
                {
                    // Проверяем, реализует ли объект интерфейс IHittable
                    IHittable hittable = collider.GetComponent<IHittable>();
                    if (hittable != null)
                    {
                        hittable.Hit();
                    }
                }
            }

            hammerCollider.enabled = true;
        }

        private void OnDrawGizmos()
        {
            if (!hammerCollider) return;

            Gizmos.color = Color.blue;

            // Получаем позицию и поворот
            Vector2 center = hammerCollider.bounds.center;
            float angle = -transform.eulerAngles.z;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);

            // Сохраняем текущую матрицу
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Применяем поворот
            Gizmos.matrix = rotationMatrix;

            // Рисуем проволочный куб с учетом поворота
            Gizmos.DrawWireCube(Vector3.zero, castSize);

            // Восстанавливаем матрицу
            Gizmos.matrix = oldMatrix;
        }
    }
}