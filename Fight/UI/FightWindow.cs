using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fight.UI
{
    public class FightWindow : MonoBehaviour
    {
        public PointerControl AttackPointer;
        public PointerControl ViewPointer;
        public Button Reload;
        public PointerControl GrenadePointer;
        public Button ChangeWeapon;

        public LoosePanel LoosePanel;
        public WinPanel WinPanel;

        public GameObject TimerGroup;
        public TextMeshProUGUI Timer;

        public IEnumerator ShowCounter(int seconds)
        {
            TimerGroup.SetActive(true);
            var showDelay = seconds;

            while (showDelay > 0)
            {
                Timer.text = showDelay.ToString();

                yield return new WaitForSeconds(1);

                showDelay -= 1;
            }
            
            TimerGroup.SetActive(false);
        }
    }
}