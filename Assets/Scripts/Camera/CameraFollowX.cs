using UnityEngine;

namespace Camera
{
    public class CameraFollowX : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target; // Цель для слежения

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 5f; // Плавность слежения
        [SerializeField] private float yOffset = 0f; // Смещение по Y

        [Header("Camera Boundaries")]
        [SerializeField] private bool useBoundaries = true; // Использовать границы
        [SerializeField] private float minXPosition = -10f; // Минимальная позиция X
        [SerializeField] private float maxXPosition = 10f; // Максимальная позиция X

        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (target == null) return;

            // Целевая позиция камеры
            Vector3 targetPosition = new Vector3(target.position.x, yOffset, transform.position.z);

            // Плавное перемещение
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _currentVelocity,
                smoothSpeed * Time.deltaTime
            );

            // Применение границ
            if (useBoundaries)
            {
                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minXPosition, maxXPosition);
            }

            transform.position = smoothedPosition;
        }

        // Метод для установки новых границ камеры
        public void SetBoundaries(float minX, float maxX)
        {
            minXPosition = minX;
            maxXPosition = maxX;
            useBoundaries = true;
        }

        // Метод для отключения границ
        public void DisableBoundaries()
        {
            useBoundaries = false;
        }
    }
}