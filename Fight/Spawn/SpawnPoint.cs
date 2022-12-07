using System.Collections;
using UnityEngine;

namespace Fight.Spawn
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private float delay;
        private bool canSpawn;
        private Coroutine updatingDelayRoutine;

        public bool CanSpawn => canSpawn;

        public void OnSpawn()
        {
            if (updatingDelayRoutine != null)
            {
                StopCoroutine(updatingDelayRoutine);
            }

            canSpawn = false;
            updatingDelayRoutine = StartCoroutine(UpdateDelayRoutine());
        }

        private IEnumerator UpdateDelayRoutine()
        {
            var timer = delay;
            
            while (timer > 0)
            {
                timer -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            canSpawn = true;
        }

        private void OnDisable()
        {
            if (updatingDelayRoutine != null)
            {
                StopCoroutine(updatingDelayRoutine);
            }
        }
    }
}