using System;
using Spells;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Player
{
    public class OxygenController : MonoBehaviour
    {
        [SerializeField] private GameObject oxygenObj;
        [SerializeField] private Image oxygenImage;
        [SerializeField] private float oxygenMax;
        [SerializeField] private float oxygenDamageDelay = 1f;

        private float currOxygen;
        private float lastDamageTime = -1;
        
        [Inject]
        private PlayerController player;

        [Inject]
        private AmuletSpell amuletSpell;

        private void Start()
        {
            currOxygen = oxygenMax;
            oxygenImage.fillAmount = currOxygen / oxygenMax;
            oxygenObj.SetActive(false);
            player.hasDive += SwitchActiveOxigenUI;
        }

        public void SwitchActiveOxigenUI(bool isDive)
        {
            oxygenObj.SetActive(isDive);

            if (!isDive)
            {
                currOxygen = oxygenMax;
            }
        }

        public void FillOxygen() => currOxygen = oxygenMax;
        
        private void Update()
        {
            if (player.isDiving && !amuletSpell.IsActive)
            {
                currOxygen -= Time.deltaTime;

                if (currOxygen <= 0)
                {
                    if (Time.time > lastDamageTime + oxygenDamageDelay)
                    {
                        player.Hit();
                        lastDamageTime = Time.time;
                    }
                }
            }

            if (player.isDiving)
                oxygenImage.fillAmount = currOxygen / oxygenMax;
        }
    }
}