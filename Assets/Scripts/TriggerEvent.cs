using System;
using Interfaces;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Layouts;
using Zenject;

namespace DefaultNamespace
{
    public class TriggerEvent : Opener
    {
        [SerializeField] private bool isActiveOnStart;
        [SerializeField] private bool triggerOnDeath;
        [SerializeField] private float maxHeightToTriggerOnDeath = -1;
        [SerializeField] private UnityEvent onTriggerEnter2D;
        [SerializeField] private UnityEvent onTriggerStay2D;
        [SerializeField] private UnityEvent onReviveTrigger;
        [SerializeField] private bool _oneShot = false;
        [SerializeField] private bool ignoreIfTimeline = false;
        [SerializeField] private bool ignoreIfDialog = false;
        private bool _isTriggered = false;
        private PlayerController _playerController;
        [Inject]
        private DialogueSystem _dialogueSystem;
        [Inject]
        private TimelineManager _timelineManager;

        private void Awake()
        {
            _playerController = FindAnyObjectByType<PlayerController>();

            if (triggerOnDeath) _playerController.OnRevive += OnPlayerRevive;

            isActive = isActiveOnStart;
        }

        private void OnPlayerRevive()
        {
            if (_playerController.transform.position.y < maxHeightToTriggerOnDeath)
                onReviveTrigger?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            LayerMask layerMask = LayerMask.GetMask("Player");
            if ((layerMask.value & (1 << other.gameObject.layer)) == 0) return;

            if (ignoreIfTimeline)
            {
                if (_timelineManager.IsCutscenePlaying()) return;
            }

            if (ignoreIfDialog)
            {
                if (_dialogueSystem.isDialogRunning) return;
            }

            if ((_isTriggered && _oneShot) || onTriggerEnter2D == null) return;
            onTriggerEnter2D?.Invoke();
            _isTriggered = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            LayerMask layerMask = LayerMask.GetMask("Player");
            if ((layerMask.value & (1 << other.gameObject.layer)) == 0) return;

            if (ignoreIfTimeline)
            {
                if (_timelineManager.IsCutscenePlaying()) return;
            }

            if (ignoreIfDialog)
            {
                if (_dialogueSystem.isDialogRunning) return;
            }

            if ((_isTriggered && _oneShot) || onTriggerStay2D == null) return;
            onTriggerStay2D?.Invoke();
            _isTriggered = true;
        }

        private void OnDestroy()
        {
            _playerController.OnRevive -= OnPlayerRevive;
        }
    }
}
