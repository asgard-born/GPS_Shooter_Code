using Fight.Enums;
using UnityEngine;

namespace Fight
{
    public class Waypoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;
        public WaypointType WaypointType;
        public bool IsFree = true;
        public int[] Waves;
    }
}