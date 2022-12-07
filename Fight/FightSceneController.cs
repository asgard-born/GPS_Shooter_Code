using System.Collections;
using Enums;
using System.Collections.Generic;
using Audio;
using Fight.PlayerSettings;
using Fight.Spawn;
using Fight.UI;
using HQFPSTemplate;
using HQFPSTemplate.Equipment;
using HQFPSTemplate.Items;
using Preloader;
using UnityEngine;

namespace Fight
{
    public class FightSceneController : MonoBehaviour
    {
        [SerializeField] private FightWindow window;
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PickupManager pickupManager;
        [SerializeField] private Camera camera;

        [SerializeField] [DatabaseItem] protected string weaponGrenade;

        [SerializeField] [Range(0, 12)] protected int grenadesCount = 1;

        private EnemyFabric enemyFabric;
        private WavesController wavesController;
        [SerializeField] private float loadingDelay = 1f;
        [Space, SerializeField] private Waypoint[] startPoints;
        [SerializeField] private Waypoint[] spawnPoints;

        //TODO ВКТР для возможности открытия сцены без переходов из другой
        [SerializeField] private FightParamsHolder fightParamsHolder;

        private void Awake()
        {
            window.gameObject.SetActive(false);
        }

        private void Start()
        {
            // StartCoroutine(Init(fightParamsHolder.FightData)); // for debug
            StartCoroutine(Init(FightParamsHolder.Instance.FightData));
        }

        private IEnumerator Init(FightData fightData)
        {
            PlaySound();

            yield return StartCoroutine(InitPlayer(fightData));
            yield return StartCoroutine(InitEnemies(fightData));
        }

        private IEnumerator InitPlayer(FightData fightData)
        {
            yield return new WaitForSeconds(loadingDelay);

            var items = new List<Item>();

            for (var i = fightData.Weapons.Length - 1; i >= 0; i--)
            {
                var itemName = fightData.Weapons[i];
                var item = pickupManager.Pickup(itemName);

                if (item != null)
                {
                    items.Add(item);
                }
            }

            yield return new WaitForSeconds(.5f);

            playerController.Init();
            playerController.OnDeath += OnLoose;
        }

        private IEnumerator InitEnemies(FightData fightData)
        {
            wavesController = gameObject.AddComponent<WavesController>();

            var contract = new WavesController.Contract
            {
                fightData = fightData,
                fightWindow = window,
                startPoints = startPoints,
                spawnPoints = spawnPoints,
                playerController = playerController,
                camera = camera,
                enemyContainer = enemyContainer
            };

            yield return StartCoroutine(wavesController.Init(contract));

            wavesController.StartFirstWave();
            wavesController.OnLastWaveFinished += OnWin;

            window.gameObject.SetActive(true);
        }

        private static void PlaySound()
        {
            AudioManager.Instance?.PlayFightSound();
        }

        private void OnWin()
        {
            window.WinPanel.gameObject.SetActive(true);
        }

        private void OnLoose()
        {
            window.LoosePanel.gameObject.SetActive(true);
        }

        private void BackToGlobalMap()
        {
            SceneLoader.Instance.LoadScene(SceneType.GlobalMap);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                BackToGlobalMap();
            }
#endif
        }
    }
}