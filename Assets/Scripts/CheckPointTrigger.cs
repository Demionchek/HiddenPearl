using System;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class CheckPointTrigger : MonoBehaviour
    {
        public int checkPointIndex;

        [Inject]
        private CheckPoints checkPoints;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                checkPoints.SetCurrentCheckpoint(checkPointIndex);
                Debug.Log(checkPointIndex);
                gameObject.SetActive(false);
            }
        }
    }
}