using System;
using Fight.Spawn;
using GlobalMap.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight
{
    [Serializable]
    public class FightData
    {
        public FightType FightType;
        public Difficulty Difficulty;
        public string[] Weapons;
        public int GrenadesCount;

        [Space] [Space] [Space] [Space]
        public GameObject MapEnemy;

        public int[] Rewards;

        public int EnergyForStartMission;
        
        public bool hasSpawnLimitForFirstWave;
        [ShowIf("hasSpawnLimitForFirstWave")] public int immediateSpawnEnemies = 10;

        [Space] [Space] [Space] [Space]
        public EnemiesWave[] Waves;
    }
}