using System;
using UnityEngine;

public class TriggerTargetAnimatorAttack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerName = "Attack";
    public bool disableOntrigger = true;
    private Collider2D collider2D;

    private void Start()
    {
        collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        animator.SetTrigger(triggerName);
        if (disableOntrigger) collider2D.enabled = false;
    }
}
