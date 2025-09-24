using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private Image bar;

        private IHealth health;
        private Image parentBar;

        private int MaxHealth;

        private void Start()
        {
            health = target.GetComponent<IHealth>();
            MaxHealth = health.GetMaxHealth();

            parentBar = bar.transform.parent.GetComponent<Image>();

            if (health == null)
            {
                Debug.LogError("IHealth component doesn't exist on gameobject! " + gameObject.name);
                return;
            }

            bar.fillAmount = health.GetHealth() / health.GetMaxHealth();
            health.healthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int value)
        {
            if (value == MaxHealth)
            {
                bar.enabled = true;
                parentBar.enabled = true;
            }
            bar.fillAmount = (float)value / (float)health.GetMaxHealth();
            if (value == 0)
            {
                bar.enabled = false;
                parentBar.enabled = false;
            }
        }
    }
}