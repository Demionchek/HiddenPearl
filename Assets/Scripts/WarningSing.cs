using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DefaultNamespace
{
    public class WarningSing : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] private string signText = "HIDE!";
        [SerializeField] private TextMeshPro textMesh;

        [Header("Light Settings")]
        [SerializeField] private List<Light2D> warningLights = new List<Light2D>();

        [Header("Check Settings")]
        [SerializeField] private DestinationSetOnSight destinationSetter;
        [SerializeField] private float checkInterval = 0.2f; // Интервал проверки в секундах

        private bool _previousVisibilityState;
        private bool _isInitialized;
        private Coroutine _checkCoroutine;

        private void Start()
        {
            Initialize();
            StartChecking();
        }

        private void OnEnable()
        {
            StartChecking();
        }

        private void OnDisable()
        {
            StopChecking();
        }

        private void OnDestroy()
        {
            StopChecking();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            // Инициализация текста
            if (textMesh != null)
            {
                textMesh.text = signText;
                textMesh.gameObject.SetActive(false);
            }

            // Инициализация света - выключаем все при старте
            SetLightsState(false);

            _previousVisibilityState = false;
            _isInitialized = true;
        }

        private void StartChecking()
        {
            if (_checkCoroutine == null && gameObject.activeInHierarchy)
            {
                _checkCoroutine = StartCoroutine(CheckVisibilityCoroutine());
            }
        }

        private void StopChecking()
        {
            if (_checkCoroutine != null)
            {
                StopCoroutine(_checkCoroutine);
                _checkCoroutine = null;
            }
        }

        private IEnumerator CheckVisibilityCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);

                if (destinationSetter == null) continue;

                bool currentVisibility = destinationSetter.CanSeeTarget;

                if (currentVisibility != _previousVisibilityState)
                {
                    UpdateVisualState(currentVisibility);
                    _previousVisibilityState = currentVisibility;
                }
            }
        }

        private void UpdateVisualState(bool isVisible)
        {
            // Обновляем текст
            if (textMesh != null)
            {
                textMesh.gameObject.SetActive(isVisible);
            }

            // Обновляем свет
            SetLightsState(isVisible);
        }

        private void SetLightsState(bool isEnabled)
        {
            foreach (var light in warningLights)
            {
                if (light != null)
                {
                    light.enabled = isEnabled;
                }
            }
        }
    }
}