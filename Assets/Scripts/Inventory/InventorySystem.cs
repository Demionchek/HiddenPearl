using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        private Dictionary<string, int> _items = new Dictionary<string, int>();

        private const string SaveKey = "Inventory";

        public event Action<string, int> OnItemChanged; // itemId, newCount

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public bool Add(string itemId, int amount = 1)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;

            _items.TryGetValue(itemId, out int current);
            _items[itemId] = current + amount;

            OnItemChanged?.Invoke(itemId, _items[itemId]);
            return true;
        }

        public bool Remove(string itemId, int amount = 1)
        {
            if (!_items.TryGetValue(itemId, out int current) || current < amount)
                return false;

            int next = current - amount;
            if (next <= 0)
                _items.Remove(itemId);
            else
                _items[itemId] = next;

            OnItemChanged?.Invoke(itemId, next);
            return true;
        }

        public int GetCount(string itemId)
        {
            _items.TryGetValue(itemId, out int count);
            return count;
        }

        public bool Has(string itemId, int amount = 1) => GetCount(itemId) >= amount;

        public IReadOnlyDictionary<string, int> GetAll() => _items;

        public void Clear()
        {
            foreach (var key in new List<string>(_items.Keys))
                OnItemChanged?.Invoke(key, 0);

            _items.Clear();
        }

        [Serializable]
        private class InventoryData
        {
            public List<string> keys   = new List<string>();
            public List<int>    values = new List<int>();
        }

        public void Save()
        {
            var data = new InventoryData();
            foreach (var kvp in _items)
            {
                data.keys.Add(kvp.Key);
                data.values.Add(kvp.Value);
            }

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void Load()
        {
            _items.Clear();

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<InventoryData>(json);
            for (int i = 0; i < data.keys.Count; i++)
                _items[data.keys[i]] = data.values[i];
        }

        public void ResetSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            Clear();
        }
    }
}
