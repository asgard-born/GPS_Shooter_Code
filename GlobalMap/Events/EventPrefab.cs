using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Fight.UI.Essences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GlobalMap.Events
{
    public class EventPrefab : SerializedMonoBehaviour
    {
        public Image eventImage;
        public Button button;
        public List<DifficultySprite> difficultyImages;

        public event Action<FightConfig, Transform> OnMouseDownEvent;
        private FightConfig fightConfig;
        private Camera cameraMain;

        private void Awake()
        {
            cameraMain = Camera.main;
            SetRandomRotation();
            button.onClick.AddListener(OnMouseUp);
        }

        private void OnMouseUp()
        {
            OnMouseDownEvent?.Invoke(fightConfig, transform);
        }
        
        protected void Update()
        {
            eventImage.transform.LookAt(eventImage.transform.position + cameraMain.transform.rotation * Vector3.forward);
        }

        public void Init(FightConfig currentConfig)
        {
            fightConfig = currentConfig;
            eventImage.sprite = difficultyImages.Where(x => x.difficulty == currentConfig.Difficulty).Select(x => x.sprite).First();
        }

        private void SetRandomRotation()
        {
            var rotationY = Random.Range(0, 360);
            var rotationVector = new Vector3(0, rotationY, 0);
            transform.rotation = Quaternion.Euler(rotationVector);
        }
    }
}