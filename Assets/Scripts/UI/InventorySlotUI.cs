using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image    icon;
        [SerializeField] private TMP_Text countText;

        public void Set(Sprite sprite, int count)
        {
            icon.sprite  = sprite;
            icon.enabled = sprite != null;

            bool showCount = count > 1;
            countText.gameObject.SetActive(showCount);
            if (showCount) countText.text = count.ToString();
        }

        public void Clear()
        {
            icon.sprite  = null;
            icon.enabled = false;
            countText.gameObject.SetActive(false);
        }
    }
}
