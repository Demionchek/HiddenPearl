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

        private void Start()
        {
            health = target.GetComponent<IHealth>();

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
            bar.fillAmount = (float)value / (float)health.GetMaxHealth();
        }
    }
}