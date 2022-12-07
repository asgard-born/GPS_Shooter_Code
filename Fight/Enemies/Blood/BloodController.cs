using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapons;

namespace Fight.Enemies.Blood
{
    public class BloodController : MonoBehaviour
    {
        [SerializeField] private int poolCount;
        [SerializeField] private float effectDuration = 1f;
        [SerializeField] private BloodPool bloodPool;

        private Dictionary<string, List<GameObject>> bloodEffectsPool;

        public static BloodController Instance;

        protected void Awake()
        {
            Instance = this;

            bloodEffectsPool = bloodPool.BloodEffectPrefabs
                .ToDictionary(effect => effect.Type, effect =>
                {
                    var list = new List<GameObject>(poolCount);

                    for (var i = 0; i < poolCount; i++)
                    {
                        var bloodEffect = Instantiate(effect.Value, transform);
                        bloodEffect.SetActive(false);
                        list.Add(bloodEffect);
                    }

                    return list;
                });
        }

        public GameObject Get(HitInfo hitInfo, bool autoreturnToPool = true)
        {
            var effects = bloodEffectsPool[hitInfo.WeaponType];
            GameObject bloodEffect = null;

            for (var i = 0; i < effects.Count; i++)
            {
                bloodEffect = effects[i];
                var wasTaken = false;

                if (!bloodEffect.activeInHierarchy)
                {
                    SetEffect(hitInfo, bloodEffect);
                    FirstToEnd(effects);
                    wasTaken = true;
                }
                else
                {
                    if (i == effects.Count - 1)
                    {
                        bloodEffect = effects[0];
                        SetEffect(hitInfo, bloodEffect);
                        wasTaken = true;
                    }
                }

                if (autoreturnToPool)
                {
                    StartCoroutine(ReturnToPool(bloodEffect));
                }

                if (wasTaken)
                {
                    break;
                }
            }

            return bloodEffect;
        }

        private static void FirstToEnd(IList<GameObject> effects)
        {
            var temp = effects[0];

            effects.Add(temp);
            effects.RemoveAt(0);
        }

        private static void SetEffect(HitInfo hitInfo, GameObject bloodEffect)
        {
            bloodEffect.transform.position = hitInfo.Position;
            bloodEffect.transform.rotation = hitInfo.Rotation;
            bloodEffect.SetActive(true);
        }

        private IEnumerator ReturnToPool(GameObject bloodEffect)
        {
            yield return new WaitForSeconds(.4f);

            bloodEffect.SetActive(false);
        }
    }
}