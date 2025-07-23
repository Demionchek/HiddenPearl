using System;
using AI;
using Interfaces;
using UnityEngine;
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

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActive) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
                other.gameObject.layer == LayerMask.NameToLayer("Rat") ||
                other.gameObject.layer == LayerMask.NameToLayer("Bird"))
            {
                isActive = true;
                character.doorCondition = true;
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                if (audioSource != null) audioSource.Play();
                if (sign != null) sign.SetActive(false);
                if (isTriggerDialog) dialogueSystem.InitDialogue((int)dialogueType);
            }
        }

    }
}