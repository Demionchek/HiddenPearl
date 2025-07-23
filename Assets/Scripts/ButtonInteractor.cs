using Animations;
using Interfaces;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace DefaultNamespace
{
    public class ButtonInteractor : Opener
    {
        [SerializeField] private LayerMask[] interactLayers;
        [SerializeField] private float checkInterval = 0.5f; // Интервал проверки в секундах

        private int collidingObjectsCount = 0;
        private Animator animator;
        private Collider2D buttonCollider;
        private Coroutine checkCollisionCoroutine;

        private void Start()
        {
            animator = GetComponent<Animator>();
            buttonCollider = GetComponent<Collider2D>();
            checkCollisionCoroutine = StartCoroutine(CheckCollisionsPeriodically());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsLayerInInteractLayers(other.gameObject.layer))
            {
                collidingObjectsCount++;
                UpdateButtonState();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsLayerInInteractLayers(other.gameObject.layer))
            {
                collidingObjectsCount--;
                if (collidingObjectsCount < 0) collidingObjectsCount = 0;
                UpdateButtonState();
            }
        }

        private IEnumerator CheckCollisionsPeriodically()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);

                // Получаем все коллайдеры, которые сейчас пересекаются с кнопкой
                var colliders = Physics2D.OverlapBoxAll(
                    buttonCollider.bounds.center,
                    buttonCollider.bounds.size,
                    0f
                );

                // Фильтруем только те, которые на нужных слоях
                int actualCount = colliders.Count(c =>
                    c != buttonCollider &&
                    IsLayerInInteractLayers(c.gameObject.layer)
                );

                // Если расхождение с нашим счетчиком - корректируем
                if (actualCount != collidingObjectsCount)
                {
                    collidingObjectsCount = actualCount;
                    UpdateButtonState();
                }
            }
        }

        private void UpdateButtonState()
        {
            bool newState = collidingObjectsCount > 0;
            if (newState != isActive)
            {
                isActive = newState;
                animator.SetBool(AnimationController.IS_ACTIVE_S, isActive);
            }
        }

        private bool IsLayerInInteractLayers(int layer)
        {
            int layerMask = 1 << layer;
            foreach (var interactLayer in interactLayers)
            {
                if (layerMask == interactLayer)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            if (checkCollisionCoroutine != null)
            {
                StopCoroutine(checkCollisionCoroutine);
            }
        }
    }
}