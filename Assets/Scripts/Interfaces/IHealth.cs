using System;
using UnityEngine;

namespace DefaultNamespace
{
    public interface IHealth
    {
        public Action<int> healthChanged { get; set; }

        public int GetHealth();
        public int GetMaxHealth();
        public void SetHealth(int health);
    }
}