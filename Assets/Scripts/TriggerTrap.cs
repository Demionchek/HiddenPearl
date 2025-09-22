using UnityEngine;

namespace DefaultNamespace
{
    public abstract class TriggerTrap : MonoBehaviour
    {
        [SerializeField] public ITrap trap;

        protected BoxCollider2D boxCollider2D;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                trap.Trigger();
                gameObject.SetActive(false);
            }
        }
    }
}