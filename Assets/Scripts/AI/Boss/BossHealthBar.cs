using System;
using UnityEngine;
using UnityEngine.UI;

namespace AI.BossPatterns
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Image HealthBar;
        [SerializeField] private int BossHealthMax = 6;
        private int bossHealthCurrent;

        public event Action OnDeath;
        public event Action OnHit;

        private void Start()
        {
            BossBody.HitEvent += TakeDamage;
            bossHealthCurrent = BossHealthMax;
            HealthBar.fillAmount = bossHealthCurrent / BossHealthMax;
        }

        private void OnDisable()
        {
            BossBody.HitEvent -= TakeDamage;
        }

        private void TakeDamage()
        {
            bossHealthCurrent--;
            HealthBar.fillAmount = (float)bossHealthCurrent / (float)BossHealthMax;

            OnHit?.Invoke();

            if (bossHealthCurrent <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}