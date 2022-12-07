using System;
using Configs;
using UnityEngine;

namespace GlobalMap
{
    [Serializable]
    public class EventAndChance
    {
        public FightConfig fightConfigs;
        [Range(5, 100)]
        public float chancePercentage = 20;
    }
}