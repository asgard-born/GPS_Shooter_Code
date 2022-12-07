using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fight.Enemies;
using Fight.Enemies.Abstract;
using Fight.Enemies.Enums;
using Fight.Enemies.State;
using Fight.PlayerSettings;
using Fight.Spawn;
using Fight.UI;
using Sirenix.Utilities;
using UnityEngine;
using static Fight.Enemies.Enums.EnemyType;
using Random = UnityEngine.Random;

namespace Fight
{
    public class WavesController : MonoBehaviour
    {
        public FightData fightData;
        public Waypoint[] startPoints;
        public Waypoint[] spawnPoints;
        public PlayerController playerController;
        public Camera camera;
        public Transform enemyContainer;

        private EnemiesWave currentWave;
        private EnemyWaypoints enemyWaypoints;

        private int waveKillCount;
        private int totalKillCount;

        private ConcurrentDictionary<EnemyType, int> killedEnemies = new ConcurrentDictionary<EnemyType, int>();
        private ConcurrentDictionary<EnemyType, int> spawnedEnemiesCount = new ConcurrentDictionary<EnemyType, int>();
        private ConcurrentDictionary<EnemyType, int> delayedSpawnEnemiesCount = new ConcurrentDictionary<EnemyType, int>();
        private List<Enemy> spawnedEnemies = new List<Enemy>();

        private int delayedCount;
        private int currentWaveNumber;

        private EnemyFabric enemyFabric;
        private EnemyWaypoints waypoints;
        private Waypoint[] spawnPointsForCurrentWave;

        private FightWindow fightWindow;
        public event Action OnWaveFinished;
        public event Action OnLastWaveFinished;

        public struct Contract
        {
            public FightData fightData;
            public FightWindow fightWindow;
            public Waypoint[] startPoints;
            public Waypoint[] spawnPoints;
            public PlayerController playerController;
            public Camera camera;
            public Transform enemyContainer;
        }

        public IEnumerator Init(Contract ctx)
        {
            fightData = ctx.fightData;
            fightWindow = ctx.fightWindow;
            startPoints = ctx.startPoints;
            spawnPoints = ctx.spawnPoints;
            playerController = ctx.playerController;
            camera = ctx.camera;
            enemyContainer = ctx.enemyContainer;

            playerController.OnFirstAttack += OnFirstAttack;

            enemyFabric = gameObject.AddComponent<EnemyFabric>();

            waypoints = new EnemyWaypoints
            {
                StartWaypoints = startPoints,
                SpawnWaypoints = spawnPoints,
                WaitingWaypoints = playerController.WaitingWaypoints,
                AttackWaypoints = playerController.AttackWaypoints
            };

            var enemyFabricData = new EnemyFabricData
            {
                playerController = playerController,
                camera = camera,
                enemyContainer = enemyContainer,
            };

            // wait for enemyWaypoints != null
            yield return new WaitForSeconds(.3f);

            enemyFabric.Init(enemyFabricData);
        }

        public void StartFirstWave()
        {
            currentWave = fightData.Waves.First();
            currentWaveNumber = 1;
            spawnPointsForCurrentWave = waypoints.SpawnWaypoints.Where(x => x.Waves.Contains(currentWaveNumber)).ToArray();
            SpawnImmediately();
            UpdateWaypointsForIdleEnemies();
        }

        private bool OnFirstAttack()
        {
            if (!spawnedEnemies.IsNullOrEmpty())
            {
                foreach (var enemy in spawnedEnemies)
                {
                    enemy.ChangeState(new MovingEnemyState());
                }

                if (!delayedSpawnEnemiesCount.IsEmpty)
                {
                    StartCoroutine(SpawnDelayed());
                }

                return true;
            }

            return false;
        }

        private void FinishWave()
        {
            totalKillCount += waveKillCount;
            waveKillCount = 0;

            spawnedEnemies.Clear();
            spawnedEnemiesCount.Clear();
            delayedSpawnEnemiesCount.Clear();

            if (currentWaveNumber == fightData.Waves.Length)
            {
                OnLastWaveFinished?.Invoke();
            }
            else
            {
                OnWaveFinished?.Invoke();
                StartCoroutine(StartNewWaveDelayed());
            }
        }

        private IEnumerator StartNewWaveDelayed()
        {
            currentWaveNumber += 1;
            var currentWaveId = currentWaveNumber - 1;
            delayedCount = 0;

            currentWave = fightData.Waves[currentWaveId];

            spawnPointsForCurrentWave = waypoints.SpawnWaypoints.Where(x => x.Waves.Contains(currentWaveNumber)).ToArray();

            foreach (var enemyTypeAndCount in currentWave.enemiesTypesCount)
            {
                for (var i = 0; i < enemyTypeAndCount.count; i++)
                {
                    delayedSpawnEnemiesCount.AddOrUpdate(enemyTypeAndCount.enemyType, 1, (_, count) => count + 1);
                    delayedCount++;
                }
            }

            yield return StartCoroutine(fightWindow.ShowCounter(currentWave.startDelay));

            StartCoroutine(SpawnDelayed());
        }

        private void SpawnImmediately()
        {
            var spawnIndex = 0;

            var spawnLimit = currentWave.enemiesCount;

            if (fightData.hasSpawnLimitForFirstWave)
            {
                spawnLimit = Math.Min(fightData.immediateSpawnEnemies, currentWave.enemiesCount);
            }

            for (var i = 0; i < currentWave.enemiesCount; i++)
            {
                var enemyType = FindTypeForSpawn();

                while (enemyType == None)
                {
                    enemyType = FindTypeForSpawn();
                }

                var freeStartPoints = waypoints.StartWaypoints.Where(p => p.IsFree).ToArray();
                var hasFreeStartpoint = freeStartPoints.Length > 0;

                var hasNoLimit = spawnIndex < spawnLimit;

                if (hasFreeStartpoint && hasNoLimit)
                {
                    var startPointRandomIndex = Random.Range(0, freeStartPoints.Length);
                    var startPoint = freeStartPoints[startPointRandomIndex];

                    SpawnEnemy(enemyType, startPoint, false);

                    startPoint.IsFree = false;
                    spawnIndex++;
                }
                else
                {
                    delayedSpawnEnemiesCount.AddOrUpdate(enemyType, 1, (id, count) => count + 1);
                    delayedCount++;
                }
            }
        }

        private IEnumerator SpawnDelayed()
        {
            while (delayedCount > 0 && delayedSpawnEnemiesCount.Count > 0)
            {
                var spawnIterationCount = Random.Range(currentWave.iterationSpawnMin, currentWave.iterationSpawnMax + 1);
                var spawnPointIndex = 0;

                for (var i = 0; i < spawnIterationCount; i++)
                {
                    if (delayedSpawnEnemiesCount.Count == 0) break;

                    var randomIndexEnemyType = Random.Range(0, delayedSpawnEnemiesCount.Count);
                    KeyValuePair<EnemyType, int> enemyTypeCount;

                    enemyTypeCount = delayedSpawnEnemiesCount.ElementAt(randomIndexEnemyType);

                    if (enemyTypeCount.Value <= 0)
                    {
                        break;
                    }

                    var spawnPoint = spawnPointsForCurrentWave[spawnPointIndex];

                    if (spawnPointIndex == spawnPointsForCurrentWave.Length - 1)
                    {
                        spawnPointIndex = 0;
                    }
                    else
                    {
                        spawnPointIndex++;
                    }

                    SpawnEnemy(enemyTypeCount.Key, spawnPoint, true);

                    var type = enemyTypeCount.Key;
                    delayedSpawnEnemiesCount[type] -= 1;
                    delayedCount--;

                    if (delayedSpawnEnemiesCount[type] == 0)
                    {
                        delayedSpawnEnemiesCount.TryRemove(type, out _);
                    }
                }

                yield return new WaitForSeconds(currentWave.spawnIterationCooldown);
            }
        }

        private void SpawnEnemy(EnemyType enemyType, Waypoint spawnPoint, bool isAttackAtStart)
        {
            var enemy = enemyFabric.SpawnEnemy(enemyType, spawnPoint, isAttackAtStart, playerController, waypoints);
            enemy.OnDeath += OnEnemyDeath;
            AddEnemy(enemy);
        }

        private void OnEnemyDeath(Enemy deadEnemy)
        {
            waveKillCount += 1;
            killedEnemies.AddOrUpdate(deadEnemy.EnemyType, 1, (id, count) => count + 1);
            RemoveEnemy(deadEnemy);
            UpdateWaypointsForIdleEnemies();

            if (waveKillCount == currentWave.enemiesCount)
            {
                FinishWave();
            }
        }

        private void AddEnemy(Enemy enemy)
        {
            spawnedEnemies.Add(enemy);
            spawnedEnemiesCount.AddOrUpdate(enemy.EnemyType, 1, (_, count) => count + 1);
        }

        private void RemoveEnemy(Enemy enemy)
        {
            enemy.ClearPoint();
            enemyFabric.RemoveEnemy(enemy);
            spawnedEnemies.Remove(enemy);
            spawnedEnemiesCount.AddOrUpdate(enemy.EnemyType, 0, (_, count) => count - 1);
        }

        private EnemyType FindTypeForSpawn()
        {
            var randomIndex = Random.Range(0, currentWave.enemiesTypesCount.Count);
            EnemyTypeCount enemyTypeCount = currentWave.enemiesTypesCount[randomIndex];

            var enemyType = enemyTypeCount.enemyType;
            var enemyTypeForSpawn = enemyType;

            if (spawnedEnemiesCount.TryGetValue(enemyType, out var spawnedCountOfThisType))
            {
                var enemyTypeCountInConfig = currentWave.enemiesTypesCount.FirstOrDefault(e => e.enemyType == enemyType);

                if (spawnedCountOfThisType >= enemyTypeCountInConfig.count)
                {
                    enemyTypeForSpawn = None;
                }
            }

            return enemyTypeForSpawn;
        }

        private void UpdateWaypointsForIdleEnemies()
        {
            var movingEnemies = spawnedEnemies.Where(e => e.IsMovingToPoint).ToArray();

            if (movingEnemies.IsNullOrEmpty()) return;

            movingEnemies.Sort((x, y) => x.CompareTo(y));

            foreach (var movingEnemy in movingEnemies)
            {
                movingEnemy.TrySetAnyNearestWaypoint();
            }
        }
    }
}