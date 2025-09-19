using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AI.BossPatterns
{
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private Image HealthBar;
        [SerializeField] private int BossHealthMax = 6;
        private int bossHealthCurrent;

        public event Action OnDeath;
        public event Action OnHit;

        [Inject]
        private PlayerController player;

        private void Start()
        {
            BossBody.HitEvent += TakeDamage;
            bossHealthCurrent = BossHealthMax;
            HealthBar.fillAmount = bossHealthCurrent / BossHealthMax;
            player.OnDeath += RecoverHealth;
        }

        private void OnDisable()
        {
            BossBody.HitEvent -= TakeDamage;
        }

        public void RecoverHealth()
        {
            StartCoroutine(RecoverHealthCoroutine());
        }

        private IEnumerator RecoverHealthCoroutine()
        {
            yield return new WaitForSeconds(1f);
            bossHealthCurrent = BossHealthMax;
            HealthBar.fillAmount = (float)bossHealthCurrent / (float)BossHealthMax;
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