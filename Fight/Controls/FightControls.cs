using System.Collections;
using Fight.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fight.Controls
{
    public class FightControls : MonoBehaviour
    {
        public FightWindow window;
        [SerializeField] private float holdingTimer = .03f;

        private bool startClickingAttack;
        private bool isHoldingAttack;
        private bool endClickingAttack;
        private bool isClickingAttack;
        private bool isHoldingView;

        public bool StartClickingAttack => startClickingAttack;
        public bool IsHoldingAttack => isHoldingAttack;
        public bool EndClickingAttack => endClickingAttack;
        public bool IsHoldingView => isHoldingView;

        public Vector2 Touch => touch;

        private Vector2 touch;
        private PointerEventData _pointerViewData;

        private void Awake()
        {
            window.AttackPointer.PointerDownEvent += _ => StartCoroutine(OnClickAttack());
            window.AttackPointer.PointerUpEvent += _ => StartCoroutine(OnReleaseAttack());
            window.ViewPointer.PointerDownEvent += OnClickView;
            window.ViewPointer.PointerUpEvent += OnReleaseView;
        }

        private void Update()
        {
            if (isHoldingView)
            {
                var deltaTouch = _pointerViewData.delta;

                touch = new Vector2(deltaTouch.y, deltaTouch.x);
            }
            else
            {
                touch = Vector2.zero;
            }
        }

        private void OnClickView(PointerEventData pointerEventData)
        {
#if UNITY_EDITOR
            if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
#endif
                _pointerViewData = pointerEventData;
                isHoldingView = true;
#if UNITY_EDITOR
            }
#endif
        }

        private void OnReleaseView(PointerEventData pointerEventData)
        {
#if UNITY_EDITOR
            if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
#endif
                isHoldingView = false;
#if UNITY_EDITOR
            }
#endif
        }

        private IEnumerator OnClickAttack()
        {
            startClickingAttack = true;

            yield return new WaitForEndOfFrame();

            startClickingAttack = false;
            isClickingAttack = true;

            StartCoroutine(CheckForHoldingAttack());
        }

        private IEnumerator CheckForHoldingAttack()
        {
            yield return new WaitForSeconds(holdingTimer);

            if (isClickingAttack)
            {
                isHoldingAttack = true;
            }
        }

        private IEnumerator OnReleaseAttack()
        {
            endClickingAttack = true;
            isHoldingAttack = false;
            isClickingAttack = false;

            yield return new WaitForEndOfFrame();

            endClickingAttack = false;
        }
    }
}