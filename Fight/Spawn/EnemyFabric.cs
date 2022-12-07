using System.Collections.Generic;
using Fight.Enemies;
using Fight.Enemies.Abstract;
using Fight.Enemies.Enums;
using Fight.Enemies.State;
using Fight.PlayerSettings;
using Preloader;
using UnityEngine;

namespace Fight.Spawn
{
    public class EnemyFabric : MonoBehaviour
    {
        private EnemyFabricData data;
        private Camera camera;
        private List<Enemy> enemies = new List<Enemy>();

        public void Init(EnemyFabricData spawnData)
        {
            data = spawnData;
            camera = spawnData.camera;
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
        }

        public Enemy SpawnEnemy(EnemyType enemyType, Waypoint spawnPoint, bool isAttack, PlayerController playerController, EnemyWaypoints waypoints)
        {
            var enemyPrefab = FightParamsHolder.Instance.Enemies[enemyType];
            var enemy = Instantiate(enemyPrefab, spawnPoint.Position, Quaternion.identity, data.enemyContainer);
            enemy.Init(playerController, waypoints);
            enemy.transform.Rotate(Vector3.up, Random.Range(0, 360));
            data.enemyContainer.SetParent(null);

            EnemyState state;

            if (isAttack)
            {
                state = new MovingEnemyState();
            }
            else
            {
                state = new IdleEnemyState();
            }

            enemy.InitBehaviour(state, camera);

            enemies.Add(enemy);

            return enemy;
        }
    }
}