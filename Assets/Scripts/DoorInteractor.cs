using System;
using Animations;
using Interfaces;
using UnityEngine;

namespace DefaultNamespace
{
    public class DoorInteractor : MonoBehaviour
    {
        [SerializeField] public Opener[] openers;
        [SerializeField] public AudioClip onOpenSound;
        [SerializeField] public AudioClip onCloseSound;

        private BoxCollider2D boxCollider2D;
        private AudioSource audioSource;
        private Animator animator;
        private bool wasOpen;
        private bool manuallyOpen;

        private void Start()
        {
            animator = GetComponent<Animator>();
            boxCollider2D = GetComponent<BoxCollider2D>();
            audioSource = GetComponent<AudioSource>();
            wasOpen = AreAllOpenersActive(); // Инициализируем начальное состояние
        }

        private void Update()
        {
            if (openers == null || openers.Length == 0) return;

            bool isOpenNow = AreAllOpenersActive();

            // Проверяем изменение состояния
            if (isOpenNow != wasOpen && !manuallyOpen)
            {
                UpdateDoorState(isOpenNow);
                wasOpen = isOpenNow;
            }
        }

        private bool AreAllOpenersActive()
        {
            foreach (var opener in openers)
            {
                if (opener == null || !opener.isActive)
                    return false;
            }
            return true;
        }

        private void UpdateDoorState(bool isOpen)
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (boxCollider2D == null) boxCollider2D = GetComponent<BoxCollider2D>();

            animator.SetBool(AnimationController.IS_OPEN_S, isOpen);
            if (boxCollider2D != null)boxCollider2D.enabled = !isOpen;

            // Воспроизводим соответствующий звук
            if (audioSource != null)
            {
                AudioClip clip = isOpen ? onOpenSound : onCloseSound;
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }

        public void OpenManual(bool isOpen)
        {
            UpdateDoorState(isOpen);
            wasOpen = isOpen;
            manuallyOpen = isOpen;
        }
    }
}