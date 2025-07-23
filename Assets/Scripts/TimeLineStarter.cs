using System;
using Player;
using UnityEngine;
using UnityEngine.Playables;
using Zenject;

namespace DefaultNamespace
{
    public class TimeLineStarter : MonoBehaviour
    {
        public PlayableDirector director;

        private bool isActive = false;
        [Inject]
        private PlayerController playerController;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playerController.isDead) return;

            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
            {
                isActive = true;
                director.Play();
                if (playerController.transform.position.y > transform.position.y)
                    playerController.transform.position = transform.position;
            }
        }
    }
}