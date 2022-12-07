using System;
using GlobalMap.Enums;
using UnityEngine;

namespace Fight.UI.Essences
{
    [Serializable]
    public struct DifficultySprite
    {
        public Difficulty difficulty;
        public Sprite sprite;
    }
}