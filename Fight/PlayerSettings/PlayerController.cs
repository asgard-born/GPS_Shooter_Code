using System;
using HQFPSTemplate.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Fight.PlayerSettings
{
    public class PlayerController : MonoBehaviour
    {
        //TODO ВКТР актуализировать работы смены оружия
        public Item currentWeapon;
        public Waypoint[] WaitingWaypoints;
        public Waypoint[] AttackWaypoints;

        [SerializeField] private int health = 1000;
        [SerializeField] private HandsMechanic handsMechanic;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private HQFPSTemplate.Player player;
        [SerializeField] private ScreenDamage screenDamage;
        [SerializeField] private Image hitFrame;

        public event Action OnDeath;
        private bool isFirstAttack = true;
        public event Func<bool> OnFirstAttack;
        public HQFPSTemplate.Player Player => player;

        public void Init(Item weapon = null)
        {
            playerHealth.Init(health);
            playerHealth.OnDeath += Death;
            handsMechanic.OnFirstStrike += CheckForFirstAttack;
            // currentWeapon = weapon;
        }

        private void Awake()
        {
            screenDamage.HitFrame = hitFrame;
            screenDamage.MaxHealth = health;
        }

        public void GetDamage(int damage)
        {
            //TODO ВКТР сделать проверку на броню, выдернуть из нее коэффициент защиты и делить на него урон
            //TODO это делать в HitDealer'e
            // добавить в PlayerInventory новый список
            // damage /= PlayerInventory.СписокОдетогоСнаряжения.Броня;

            playerHealth.DecreaseHealth(damage);
        }

        public bool Fire(bool isSerial)
        {
            var hasSucceed = player.UseItem.Try(isSerial, 0);

            if (hasSucceed)
            {
                OnShootSucceed();
            }

            return hasSucceed;
        }

        private void OnShootSucceed()
        {
        }

        public void Reload()
        {
            player.Reload.TryStart();
        }

        public void Heal()
        {
            player.Healing.TryStart();
        }

        private bool CheckForFirstAttack()
        {
            if (isFirstAttack)
            {
                if (OnFirstAttack != null && OnFirstAttack.Invoke())
                {
                    isFirstAttack = false;

                    return true;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            playerHealth.OnDeath -= Death;
        }

        private void Death()
        {
            OnDeath?.Invoke();
        }
    }
}