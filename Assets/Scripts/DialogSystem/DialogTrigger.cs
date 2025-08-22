using System;
using DefaultNamespace;
using Player;
using UnityEngine;
using Zenject;

namespace DialogSystem
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogTrigger : MonoBehaviour
    {
        public int dialogIndex = 0;

        private BoxCollider2D boxCollider2D;

        [Inject]
        private DialogueSystem dialogueSystem;
        [Inject]
        private PlayerController playerController;

        private void Awake()
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
            playerController.OnRevive += EnableCollider;
        }

        private void OnDestroy()
        {
            playerController.OnRevive -= EnableCollider;
        }

        private void EnableCollider()
        {
            boxCollider2D.enabled = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                dialogueSystem.InitDialogue(dialogIndex);
                gameObject.SetActive(false);
            }
        }
    }
}