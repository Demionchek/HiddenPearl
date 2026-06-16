using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public string Id          { get; private set; }
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite Icon        { get; private set; }
        [field: SerializeField] public bool   IsStackable { get; private set; } = true;
        [field: SerializeField] public int    MaxStack    { get; private set; } = 99;
    }
}
