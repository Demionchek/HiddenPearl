using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class FollowX : MonoBehaviour
    {
        public Transform target;
        private Vector2 startPos;

        private void Start()
        {
            startPos = new Vector2(target.position.x, transform.position.y);
        }

        private void FixedUpdate()
        {
            transform.position = new Vector3(target.position.x, startPos.y, transform.position.z);
        }
    }
}