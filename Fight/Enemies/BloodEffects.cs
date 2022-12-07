using System;
using HQFPSTemplate;
using UnityEngine;

namespace Fight.Enemies
{
    [Serializable]
    public struct BloodEffects
    {
        [DatabaseItem] public string Type;
        public GameObject Value;
    }
}