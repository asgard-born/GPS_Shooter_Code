using System;
using System.Linq;
using Fight.Enemies.Enums;
using Sirenix.OdinInspector;

namespace Fight.Spawn
{
    [Serializable]
    public class EnemyTypeCount
    {
        public EnemyType enemyType;

        [PropertyRange(0, "allCount"), OnValueChanged("Recalculate")]
        public int count;

        private EnemiesWave enemiesWave;

#if UNITY_EDITOR
        //need for Odin Inspector
        private int allCount;

        public void Recalculate()
        {
            if (enemiesWave.enemiesTypesCount.Count == 1)
            {
                count = enemiesWave.enemiesCount;
            }
            else
            {
                var sum = enemiesWave.enemiesTypesCount.Where(e => e.enemyType != enemyType).Sum(e => e.count);

                if (sum + count > enemiesWave.enemiesCount)
                {
                    count = enemiesWave.enemiesCount - sum;
                }
            }

            for (var i = enemiesWave.enemiesTypesCount.Count - 1; i >= 0; i--)
            {
                var element = enemiesWave.enemiesTypesCount[i];

                if (element == this) continue;

                if (element.enemyType == enemyType)
                {
                    enemiesWave.enemiesTypesCount.Remove(this);
                }
            }
        }
#endif

        public void Init(EnemiesWave enemiesWave)
        {
            this.enemiesWave = enemiesWave;
#if UNITY_EDITOR
            allCount = enemiesWave.enemiesCount;
#endif
        }
    }
}