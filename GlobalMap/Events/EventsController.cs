using Configs;
using Enums;
using Fight;
using Fight.UI;
using Preloader;
using Stalkers.Static;
using UnityEngine;

namespace GlobalMap.Events
{
    public class EventsController : MonoBehaviour
    {
        [SerializeField] private EventGenerator eventGenerator;
        [SerializeField] private FightWindowLobby fightWindowLobby;
        [SerializeField] private SceneType[] sceneTypes;

        private void Awake()
        {
            eventGenerator.OnFightClick += OnFightEventClicked;
        }

        private void OnCloseFightWindow()
        {
        }

        private void OnFightEventClicked(FightConfig fightConfig, Transform transform)
        {
            // TODO ВКТР МОК, сделать через выбор в лобби, по дефолту - из инвентаря
            var weapons = new[]
            {
                GameHelper.Weapons.ASSAULT_R_F1,
                GameHelper.Weapons.MOLOTOV_COCKTAIL,
                GameHelper.Weapons.HUNTING_RIFLE,
                GameHelper.Weapons.SHOTGUN_R870,
                GameHelper.Weapons.ASSAULT_R_M1A,
                GameHelper.Weapons.SHOTGUN_DOUBLE_BARREL,
                GameHelper.Weapons.ASSAULT_R_AKM,
                GameHelper.Weapons.HANDGUN_REVOLVER,
                GameHelper.Weapons.BOW,
                GameHelper.Weapons.RIFLE_MP5,
            };

            var fightData = new FightData
            {
                FightType = fightConfig.FightType,
                Difficulty = fightConfig.Difficulty,
                EnergyForStartMission = fightConfig.EnergyForStart,
                Weapons = weapons,
                Waves = fightConfig.Waves,
                Rewards = fightConfig.Rewards,
                hasSpawnLimitForFirstWave = fightConfig.hasSpawnLimitForFirstWave,
                immediateSpawnEnemies = fightConfig.immediateSpawnEnemies
            };

            fightWindowLobby.Show(fightData, OnFightStart, OnCloseFightWindow);
        }

        private void OnFightStart(FightData fightData)
        {
            FightParamsHolder.Instance.FightData = fightData;
            var sceneType = sceneTypes[Random.Range(0, sceneTypes.Length)];
            SceneLoader.Instance.LoadScene(sceneType);
        }
    }
}