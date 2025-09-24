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
        bool isTriggered = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isTriggered)
            {
                isTriggered = true;
                boxCollider.enabled = false;
                animator.SetTrigger(AnimationController.IS_OPEN_S);
                IHealth health = other.GetComponent<IHealth>();
                health.IncreaseHealth(1);
                int currentHealth = PlayerPrefs.GetInt("Health");
                Debug.Log("Health: " + currentHealth);
                PlayerPrefs.SetInt("Health", currentHealth + 1);
                currentHealth = PlayerPrefs.GetInt("Health");
                Debug.Log("Health: " + currentHealth);
                PlayerPrefs.Save();
            }
        }
    }
}