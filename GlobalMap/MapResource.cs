using Enums;
using SingletonsPreloaders;
using UnityEngine;

namespace GlobalMap
{
    public class MapResource : MonoBehaviour
    {
        [SerializeField] private ItemName _resourceName;
        public ItemName ResourceName => _resourceName;

        [SerializeField] private int _amount = 1;
        public int amount => _amount;

        private bool _isTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggered)
                return;

            var playerGlobal = other.gameObject.GetComponent<PlayerGlobal>();

            if (playerGlobal != null)
            {
                for (int i = 0; i < _amount; i++)
                {
                    GlobalPlayer.Instance.PlayerInventory.AddItem(_resourceName);
                }
            }

            _isTriggered = true;

            Destroy(gameObject, 0.5f);
        }
    }
}