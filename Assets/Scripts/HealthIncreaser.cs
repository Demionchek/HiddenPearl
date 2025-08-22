using System;
using Animations;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class HealthIncreaser : MonoBehaviour
    {
        Animator animator;
        BoxCollider2D boxCollider;
        
        private void Start()
        {
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                boxCollider.enabled = false;
                animator.SetTrigger(AnimationController.IS_OPEN_S);
                IHealth health = other.GetComponent<IHealth>();
                health.IncreaseHealth(1);
            }
        }
    }
}