using System;
using GlobalMap.Enums;
using UnityEngine;

namespace Fight.UI.Essences
{
    [Serializable]
    public struct DifficultyColor
    {
        public Difficulty difficulty;
        public Color color; 
    }
}