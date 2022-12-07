using System;
using GlobalMap.Enums;

namespace GlobalMap.Events.Essences
{
    [Serializable]
    public class FightTypeEventPrefab
    {
        public FightType fightType;
        public EventPrefab eventPrefab;
    }
}