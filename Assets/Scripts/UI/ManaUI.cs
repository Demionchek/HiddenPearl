using System.Collections.Generic;
using Mana;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class ManaUI : MonoBehaviour
    {
        [Tooltip("One Image per mana unit. Set Image type to Filled.")]
        [SerializeField] private List<Image> manaSectors;

        [Tooltip("Optional overlay Images to indicate reserved mana (same count as manaSectors).")]
        [SerializeField] private List<Image> reservedOverlays;

        [Inject] private ManaController manaController;

        private void Start()
        {
            manaController.OnManaChanged += UpdateUI;
            UpdateUI(manaController.CurrentManaRaw, manaController.ReservedMana);
        }

        private void UpdateUI(float currentMana, int reservedMana)
        {
            int max = manaController.MaxMana;

            for (int i = 0; i < manaSectors.Count; i++)
            {
                bool sectorExists = i < max;
                manaSectors[i].gameObject.SetActive(sectorExists);

                if (!sectorExists) continue;

                // Fill: fraction of this unit's mana that's present
                float fill = Mathf.Clamp01(currentMana - i);
                manaSectors[i].fillAmount = fill;

                // Reserved overlay — show when this mana unit is reserved
                if (reservedOverlays != null && i < reservedOverlays.Count && reservedOverlays[i] != null)
                {
                    bool isReserved = i < reservedMana;
                    reservedOverlays[i].gameObject.SetActive(isReserved);
                }
            }
        }

        private void OnDestroy()
        {
            if (manaController != null)
                manaController.OnManaChanged -= UpdateUI;
        }
    }
}
