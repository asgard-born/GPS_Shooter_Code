using UnityEngine;

namespace Fight.Enemies.Blood
{
    [CreateAssetMenu(fileName = "BloodPool", menuName = "BloodPool")]
    public class BloodPool : ScriptableObject
    {
        [SerializeField] public BloodEffects[] BloodEffectPrefabs;
    }
}