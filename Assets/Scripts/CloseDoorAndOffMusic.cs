using System.Collections;
using Player;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class CloseDoorAndOffMusic : MonoBehaviour
    {
        public AudioSource audioSource;

        public DoorInteractor door;

        public int CheckPointIndex = 3;
        [Inject]
        private CheckPoints checkPoints;
        private bool isActivated = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            if (other.TryGetComponent(out PlayerController player))
            {
                door.OpenManual(false);
                StartCoroutine(SoundOffSmooth());
                checkPoints.SetCurrentCheckpoint(CheckPointIndex);
                isActivated = true;
            }
        }

        private IEnumerator SoundOffSmooth()
        {
            while (audioSource.volume > 0.1f)
            {
                yield return new WaitForSeconds(0.3f);
                audioSource.volume -= 0.1f;
            }
            audioSource.Stop();
        }

    }
}