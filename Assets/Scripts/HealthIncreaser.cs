using Animations;
using UnityEngine;

namespace DefaultNamespace
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class HealthIncreaser : MonoBehaviour
    {
        Animator animator;
        BoxCollider2D boxCollider;
        AudioSource audio;
        bool isTriggered = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            boxCollider = GetComponent<BoxCollider2D>();
            audio = GetComponent<AudioSource>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isTriggered)
            {
                isTriggered = true;
                boxCollider.enabled = false;
                animator.SetTrigger(AnimationController.IS_OPEN_S);
                other.GetComponent<IHealth>()?.IncreaseHealth(1);
                if (audio != null) audio.Play();
            }
        }
    }
}