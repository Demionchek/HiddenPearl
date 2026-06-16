using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    public class CollectableItem : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int amount = 1;
        [SerializeField] private UnityEvent onCollected;

        private bool _collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            Collect();
        }

        private void Collect()
        {
            _collected = true;

            InventorySystem.Instance.Add(item.Id, amount);
            onCollected?.Invoke();
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (item != null && string.IsNullOrEmpty(item.Id))
                Debug.LogWarning($"[CollectableItem] У предмета '{item.name}' не задан Id.", item);
        }
#endif
    }
}
