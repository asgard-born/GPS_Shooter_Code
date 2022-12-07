using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fight.UI
{
    public class PointerControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<PointerEventData> PointerDownEvent;
        public event Action<PointerEventData> PointerUpEvent;

        public void OnPointerDown(PointerEventData eventData) => PointerDownEvent?.Invoke(eventData);
        public void OnPointerUp(PointerEventData eventData) => PointerUpEvent?.Invoke(eventData);
    }
}