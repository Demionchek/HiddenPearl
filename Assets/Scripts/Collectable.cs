using System;
using AI;
using Interfaces;
using Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace DefaultNamespace
{
    public class Collectable : Opener
    {
        public InteractableCharacter character;
        public GameObject sign;
        public bool isTriggerDialog;
        public DialogType dialogueType;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;

        [Inject]
        private DialogueSystem dialogueSystem;

        [Inject]
        private PlayerController playerController;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();

            playerController.OnRevive += OnRevive;
        }

        private void OnDestroy()
        {
            playerController.OnRevive -= OnRevive;
        }

        private void OnRevive()
        {
            isActive = false;
            spriteRenderer.enabled = true;
            if (TryGetComponent(out Light2D light2D)) light2D.enabled = true;

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActive) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
                other.gameObject.layer == LayerMask.NameToLayer("Rat") ||
                other.gameObject.layer == LayerMask.NameToLayer("Bird"))
            {
                isActive = true;
                if (character != null) character.doorCondition = true;
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                if (audioSource != null) audioSource.Play();
                if (sign != null) sign.SetActive(false);
                if (isTriggerDialog) dialogueSystem.InitDialogue((int)dialogueType);
                if (TryGetComponent(out Light2D light2D)) light2D.enabled = false;
            }
        }

    }
}