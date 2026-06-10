using UnityEngine;

namespace Spells
{
    public class AmuletSpell : SpellBase
    {
        [SerializeField] private GameObject shieldVisual;

        protected override int ManaCost => 1;
        protected override bool IsCostReserved => true;

        protected override void OnActivate()
        {
            if (shieldVisual) shieldVisual.SetActive(true);
        }

        protected override void OnCancel()
        {
            if (shieldVisual) shieldVisual.SetActive(false);
        }

        protected override void OnAbsorb()
        {
            if (shieldVisual) shieldVisual.SetActive(false);
        }

        // Возвращает true если щит поглотил удар
        public bool TryAbsorb() => Absorb();
    }
}
