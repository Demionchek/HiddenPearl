using Interfaces;
using Player;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class Teleporter : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform teleportTo;

        [Inject]
        private PlayerController playerController;

        public void Interact()
        {
            playerController.transform.position = teleportTo.position;
        }
    }
}