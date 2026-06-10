using System;
using Mana;
using UnityEngine;
using Zenject;

namespace Spells
{
    public abstract class SpellBase : MonoBehaviour
    {
        [Inject] protected ManaController manaController;

        public bool IsActive { get; protected set; }
        public event Action<bool> OnSpellChanged;

        protected abstract int ManaCost { get; }
        protected abstract bool IsCostReserved { get; }

        public bool TryActivate()
        {
            if (IsActive) return false;

            if (IsCostReserved)
            {
                if (!manaController.ReserveMana(ManaCost)) return false;
            }
            else
            {
                if (!manaController.SpendMana(ManaCost)) return false;
            }

            IsActive = true;
            OnActivate();
            OnSpellChanged?.Invoke(true);
            return true;
        }

        public void Cancel()
        {
            if (!IsActive) return;
            IsActive = false;

            if (IsCostReserved)
                manaController.BurnReservedMana(ManaCost);

            OnCancel();
            OnSpellChanged?.Invoke(false);
        }

        // Для резервных заклинаний — поглощение эффекта (мана сгорает, заклинание завершается)
        protected bool Absorb()
        {
            if (!IsActive) return false;
            IsActive = false;

            if (IsCostReserved)
                manaController.BurnReservedMana(ManaCost);

            OnAbsorb();
            OnSpellChanged?.Invoke(false);
            return true;
        }

        protected virtual void OnActivate() { }
        protected virtual void OnCancel() { }
        protected virtual void OnAbsorb() { }
    }
}
