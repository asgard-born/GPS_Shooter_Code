using System;
using Fight.Enemies.Enums;
using UnityEngine;
using Weapons;

namespace Fight.Enemies.Concrete
{
    public class EnemyBodyPart : MonoBehaviour
    {
        [Range(.5f, 5f)] public float DamageMultiplier = 1f;
        public BodyPartType Part;
        public event Action<HitInfo, EnemyBodyPart> OnHit;

        public void Hit(HitInfo hitInfo)
        {
            OnHit?.Invoke(hitInfo, this);
        }
    }
}