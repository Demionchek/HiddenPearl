using Spells;
using UnityEngine;

namespace UI
{
    public class SpellButtonUI : MonoBehaviour
    {
        [SerializeField] private SpellBase spell;
        [SerializeField] private GameObject buttonDefault;
        [SerializeField] private GameObject buttonPressed;

        private void Start()
        {
            spell.OnSpellChanged += OnSpellChanged;
            OnSpellChanged(spell.IsActive);
        }

        private void OnSpellChanged(bool isActive)
        {
            buttonDefault.SetActive(!isActive);
            buttonPressed.SetActive(isActive);
        }

        private void OnDestroy()
        {
            if (spell != null)
                spell.OnSpellChanged -= OnSpellChanged;
        }
    }
}
