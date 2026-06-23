using UnityEngine;

namespace Inventory
{
    public class InventoryDebugAdder : MonoBehaviour
    {
        [System.Serializable]
        private struct ItemEntry
        {
            public string itemId;
            public int amount;
        }

        [SerializeField] private ItemEntry[] items;

        [ContextMenu("Add Items")]
        public void AddItems()
        {
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.itemId)) continue;
                InventorySystem.Instance.Add(item.itemId, item.amount);
                Debug.Log($"[InventoryDebug] Added {item.amount}x '{item.itemId}'");
            }
        }

        [ContextMenu("Remove Items")]
        public void RemoveItems()
        {
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.itemId)) continue;
                InventorySystem.Instance.Remove(item.itemId, item.amount);
                Debug.Log($"[InventoryDebug] Removed {item.amount}x '{item.itemId}'");
            }
        }

        [ContextMenu("Print Inventory")]
        public void PrintInventory()
        {
            var all = InventorySystem.Instance.GetAll();
            if (all.Count == 0) { Debug.Log("[InventoryDebug] Inventory is empty"); return; }
            foreach (var kvp in all)
                Debug.Log($"[InventoryDebug] {kvp.Key}: {kvp.Value}");
        }

        [ContextMenu("Clear Inventory")]
        public void ClearInventory()
        {
            InventorySystem.Instance.Clear();
            Debug.Log("[InventoryDebug] Inventory cleared");
        }
    }
}
