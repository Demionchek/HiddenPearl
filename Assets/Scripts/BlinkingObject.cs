using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace DefaultNamespace
{

    public class BlinkingObject : MonoBehaviour
    {
        [Header("Основные настройки")]
        public float interval = 1f; // Интервал между включением/выключением
        public float startDelay = 0f; // Задержка перед первым включением
        public bool startEnabled = false; // Начинать в включенном состоянии

        [Header("Волновой эффект")]
        public bool useWaveEffect = false; // Использовать волновой эффект
        public float waveDelay = 0.1f; // Задержка между объектами в волне
        public List<BlinkingObject> waveObjects; // Список объектов для волны

        private SpriteRenderer spriteRenderer;
        private Collider2D objectCollider;
        private Light2D light2D;
        private AudioSource audioSource;
        private bool isActive;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            objectCollider = GetComponent<Collider2D>();
            light2D = GetComponent<Light2D>();
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            // Устанавливаем начальное состояние
            SetActive(startEnabled);

            // Запускаем корутину
            if (useWaveEffect && waveObjects.Count > 0)
            {
                StartCoroutine(WaveRoutine());
            }
            else
            {
                //StartCoroutine(BlinkRoutine());
            }
        }

        IEnumerator BlinkRoutine()
        {
            yield return new WaitForSeconds(startDelay);

            while (true)
            {
                yield return new WaitForSeconds(interval);
                ToggleActive();
            }
        }

        IEnumerator WaveRoutine()
        {
            yield return new WaitForSeconds(startDelay);

            while (true)
            {
                // Включаем текущий объект
                SetActive(true);

                // Запускаем волну для связанных объектов
                foreach (var obj in waveObjects)
                {
                    yield return new WaitForSeconds(waveDelay);
                    obj.SetActive(true);
                }

                yield return new WaitForSeconds(interval);

                // Выключаем текущий объект
                SetActive(false);

                // Выключаем волну для связанных объектов
                foreach (var obj in waveObjects)
                {
                    yield return new WaitForSeconds(waveDelay);
                    obj.SetActive(false);
                }

                yield return new WaitForSeconds(interval);
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;

            if (spriteRenderer != null)
                spriteRenderer.enabled = active;

            if (objectCollider != null)
                objectCollider.enabled = active;

            if (light2D != null)
                light2D.enabled = active;

            if (audioSource != null)
                audioSource.enabled = active;
        }

        public void ToggleActive()
        {
            SetActive(!isActive);
        }
    }
}