using System.Collections.Generic;
using Inventory;
using UnityEngine;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Tooltip("Все ItemData в проекте — для поиска иконки по Id")]
        [SerializeField] private ItemData[] itemDatabase;

        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private Transform       slotsParent;

        private readonly Dictionary<string, InventorySlotUI> _slots = new Dictionary<string, InventorySlotUI>();
        private readonly Dictionary<string, ItemData>        _db    = new Dictionary<string, ItemData>();

        private void Awake()
        {
            foreach (var data in itemDatabase)
                if (data != null) _db[data.Id] = data;
        }

        private void Start()
        {
            InventorySystem.Instance.OnItemChanged += OnItemChanged;
            Rebuild();
        }

        private void OnDestroy()
        {
            if (InventorySystem.Instance != null)
                InventorySystem.Instance.OnItemChanged -= OnItemChanged;
        }

        // Полная перестройка при старте / загрузке сцены
        private void Rebuild()
        {
            foreach (var kvp in InventorySystem.Instance.GetAll())
                RefreshSlot(kvp.Key, kvp.Value);

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            gameObject.SetActive(_slots.Count > 0);
        }

        private void OnItemChanged(string itemId, int newCount)
        {
            RefreshSlot(itemId, newCount);
            UpdateVisibility();
        }

        private void RefreshSlot(string itemId, int count)
        {
            if (count <= 0)
            {
                RemoveSlot(itemId);
                return;
            }

            if (!_slots.TryGetValue(itemId, out InventorySlotUI slot))
            {
                slot = Instantiate(slotPrefab, slotsParent);
                _slots[itemId] = slot;
            }

            _db.TryGetValue(itemId, out ItemData data);
            slot.Set(data != null ? data.Icon : null, count);
        }

        private void RemoveSlot(string itemId)
        {
            if (!_slots.TryGetValue(itemId, out InventorySlotUI slot)) return;
            _slots.Remove(itemId);
            Destroy(slot.gameObject);
        }
    }
}
