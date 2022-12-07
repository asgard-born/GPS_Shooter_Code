using System;
using System.Collections.Generic;
using System.Linq;
using Fight.UI.Essences;
using Sirenix.Utilities;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Fight.UI
{
    public class FightWindowLobby : TransitionWindow
    {
        [SerializeField] private GameObject[] waves;
        [SerializeField] private Image eventImage;
        [SerializeField] private List<DifficultySprite> zombieDifficultyImages;
        [SerializeField] private Button start;
        [SerializeField] private Button cancel;

        [SerializeField] private List<DifficultyColor> difficultyColors;

        [SerializeField] private Image primaryImage;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Image secondaryImage;
        [SerializeField] private Button secondaryButton;

        [SerializeField] private Image RewardFirst;
        [SerializeField] private Image RewardSecond;
        [SerializeField] private Image RewardThird;

        [SerializeField] private TextMeshProUGUI energyCount;
        [SerializeField] private TextMeshProUGUI fightType;
        [SerializeField] private TextMeshProUGUI difficulty;
        [SerializeField] private TextMeshProUGUI grenadesCount;
        [SerializeField] private TextMeshProUGUI aidsCount;

        private bool isOpen;
        private string weaponType;

        public event Action OnClose;

        private void OnEnable()
        {
            cancel.onClick.AddListener(Hide);
        }

        public void Show(FightData fightData, Action<FightData> onStart, Action onCancel)
        {
            if (isOpen) return;

            waves.ForEach(x => x.SetActive(false));

            for (var i = 0; i < fightData.Waves.Length; i++)
            {
                waves[i].SetActive(true);
            }

            difficulty.text = fightData.Difficulty.ToString();
            difficulty.color = difficultyColors.Where(x => x.difficulty == fightData.Difficulty).Select(x => x.color).First();
            eventImage.sprite = zombieDifficultyImages.Where(x => x.difficulty == fightData.Difficulty).Select(x => x.sprite).First();

            isOpen = true;

            start.onClick.AddListener(() => StartWithActualFightData(onStart, fightData));
            cancel.onClick.AddListener(() => onCancel());

            energyCount.text = fightData.EnergyForStartMission.ToString();
            fightType.text = fightData.FightType.ToString();

            gameObject.SetActive(true);

            TransitIn();
        }

        private void Hide()
        {
            if (!isOpen) return;

            isOpen = false;

            start.onClick.RemoveAllListeners();
            cancel.onClick.RemoveAllListeners();

            TransitOut();

            OnClose?.Invoke();
        }

        private void StartWithActualFightData(Action<FightData> onStart, FightData playerConfigs)
        {
            // TODO ВКТР сейчас оружие создается в другом методе, реализовать выбор оружия из лобби 
            onStart?.Invoke(playerConfigs);
        }
    }
}