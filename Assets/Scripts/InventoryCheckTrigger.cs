using Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace
{
    public class InventoryCheckTrigger : MonoBehaviour
    {
        [System.Serializable]
        private struct ItemRequirement
        {
            public string itemId;
            public int amount;
        }

        [SerializeField] private ItemRequirement[] requirements;
        [SerializeField] private bool oneShot = true;
        [SerializeField] private UnityEvent onRequirementsMet;
        [SerializeField] private UnityEvent onRequirementsNotMet;

        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (oneShot && _triggered) return;

            LayerMask playerMask = LayerMask.GetMask("Player");
            if ((playerMask.value & (1 << other.gameObject.layer)) == 0) return;

            Check();
        }

        private void Check()
        {
            var inventory = InventorySystem.Instance;

            foreach (var req in requirements)
            {
                if (string.IsNullOrEmpty(req.itemId)) continue;
                if (!inventory.Has(req.itemId, req.amount))
                {
                    onRequirementsNotMet?.Invoke();
                    return;
                }
            }
            onRequirementsMet?.Invoke();
            _triggered = true;
        }
    }
}
