using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class CheckPoints : MonoBehaviour
    {
        [SerializeField] private Transform[] _checkPoints;

        public int startCheckPoint = 0;

        public Transform CurrentCheckPoint { get; private set; }

        private void Awake()
        {
            CurrentCheckPoint = _checkPoints[startCheckPoint];
        }

        public void SetCurrentCheckpoint(int index) => CurrentCheckPoint = _checkPoints[index];
    }
}