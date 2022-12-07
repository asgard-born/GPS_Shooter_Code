using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Fight.Spawn
{
    [Serializable]
    public class EnemiesWave
    {
#if UNITY_EDITOR
        [NonSerialized] public Color editorColor = Color.red;
#endif

#if UNITY_EDITOR
        [GUIColor("editorColor")]
        [Space]
        [Space]
        [Space]
        [OnValueChanged("Recalculate"), MinValue(0)]
#endif
        public int enemiesCount;

#if UNITY_EDITOR
        [GUIColor("editorColor")]
        [OnValueChanged("Recalculate")]
#endif
        public List<EnemyTypeCount> enemiesTypesCount;
        
#if UNITY_EDITOR
        [GUIColor("editorColor")]
#endif
        public int iterationSpawnMin;

#if UNITY_EDITOR
        [GUIColor("editorColor")]
#endif
        public int iterationSpawnMax;

#if UNITY_EDITOR
        [GUIColor("editorColor")]
#endif
        public float spawnIterationCooldown = 5f;

#if UNITY_EDITOR
        [GUIColor("editorColor")]
#endif
        public float spawnEnemyCooldown = 1f;

#if UNITY_EDITOR
        [GUIColor("editorColor")]
#endif
        public int startDelay = 10;
#if UNITY_EDITOR
        public void Recalculate()
        {
            foreach (var typeCount in enemiesTypesCount)
            {
                typeCount.Init(this);

                if (enemiesTypesCount.Count == 1)
                {
                    typeCount.count = enemiesCount;
                }
            }
        }
#endif
    }
}