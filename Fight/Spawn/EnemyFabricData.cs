using Fight.PlayerSettings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Spawn
{
    public struct EnemyFabricData
    {
        public Waypoint[] startPoints;
        public Waypoint[] spawnPoints;
        public PlayerController playerController;
        public Camera camera;
        public Transform enemyContainer;
        [ShowIf("spawnFirstWaveEntirely")] public int firstCountToSpawn;
    }
}