using System;
using UnityEngine;

namespace Fight.PlayerSettings
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private ScreenDamage screenDamage;
        private int startHealth;
        private int currentHealth;

        public event Action OnDeath;

        public void Init(int health)
        {
            startHealth = health;
            //TODO ВКТР вернуть
            // screenDamage.MaxHealth = startHealth;
            currentHealth = health;
        }

        public void DecreaseHealth(int damage)
        {
            currentHealth -= Mathf.Abs(damage);
            screenDamage.CurrentHealth -= damage;

            if (currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
}