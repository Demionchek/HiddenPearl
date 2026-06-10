using System;
using UnityEngine;

namespace Mana
{
    public class ManaController : MonoBehaviour
    {
        [SerializeField] private float manaRegenPerSecond = 0.05f;

        private int _maxMana = 1;
        private float _currentMana;
        private int _reservedMana;

        public int MaxMana => _maxMana;
        public float CurrentManaRaw => _currentMana;
        public int CurrentMana => Mathf.FloorToInt(_currentMana);
        public int ReservedMana => _reservedMana;
        public int AvailableMana => CurrentMana - _reservedMana;

        public event Action<float, int> OnManaChanged; // currentManaRaw, reservedMana

        private void Awake()
        {
            _currentMana = _maxMana;
        }

        private void Update()
        {
            if (_currentMana < _maxMana)
            {
                _currentMana = Mathf.Min(_currentMana + manaRegenPerSecond * Time.deltaTime, _maxMana);
                OnManaChanged?.Invoke(_currentMana, _reservedMana);
            }
        }

        public bool CanSpend(int amount) => AvailableMana >= amount;

        public bool SpendMana(int amount)
        {
            if (!CanSpend(amount)) return false;
            _currentMana -= amount;
            OnManaChanged?.Invoke(_currentMana, _reservedMana);
            return true;
        }

        public bool ReserveMana(int amount)
        {
            if (!CanSpend(amount)) return false;
            _reservedMana += amount;
            OnManaChanged?.Invoke(_currentMana, _reservedMana);
            return true;
        }

        public void BurnReservedMana(int amount)
        {
            int burn = Mathf.Min(amount, _reservedMana);
            _reservedMana -= burn;
            _currentMana = Mathf.Max(_currentMana - burn, 0f);
            OnManaChanged?.Invoke(_currentMana, _reservedMana);
        }

        public void ReleaseReservedMana(int amount)
        {
            _reservedMana = Mathf.Max(_reservedMana - amount, 0);
            OnManaChanged?.Invoke(_currentMana, _reservedMana);
        }
    }
}
