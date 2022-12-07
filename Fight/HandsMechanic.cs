using System;
using System.Linq;
using Fight.Controls;
using Fight.Enums;
using Fight.PlayerSettings;
using HQFPSTemplate;
using HQFPSTemplate.Equipment;
using HQFPSTemplate.Items;
using UnityEngine;

namespace Fight
{
    public class HandsMechanic : MonoBehaviour
    {
        [SerializeField] private Transform handsTransform;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Item[] weapons;
        [SerializeField] private FightControls controls;
        [SerializeField] private Inventory inventory;

        [Space] [Space] [SerializeField] private RectTransform crosshair;
        [SerializeField] private Canvas parentCanvas;
        [SerializeField] private RectTransform parentCanvasRect;
        [SerializeField] private EquipmentSelection equipmentSelection;
        private int weaponsCount;

        private Camera cameraMain;
        private AudioSource audioSource;

        private HandsState currentState;
        private ItemContainer holsterContainer;
        private int _currentWeaponIndex;

        private Coroutine serialFiringRoutine;
        private Coroutine firingRoutine;
        private Coroutine reloadRoutine;
        private bool canFireManual;
        private bool isFirstStrike = true;
        private bool _isSerial;
        public event Func<bool> OnFirstStrike;

        private void Awake()
        {
            cameraMain = Camera.main;
            controls.window.ChangeWeapon.onClick.AddListener(ChangeWeapon);
            controls.window.Reload.onClick.AddListener(TryReload);
            equipmentSelection.OnInitialized += OnContainersInitialized;
        }

        private void Start()
        {
            weaponsCount = inventory.InitialContainers.First(x => x.Name == "Holster").Size;
            equipmentSelection.OnInitialized += OnContainersInitialized;
        }

        private void OnContainersInitialized()
        {
            holsterContainer = equipmentSelection.HolsterContainer;
            holsterContainer.SelectedSlot.Set(_currentWeaponIndex);
        }

        private void ChangeWeapon()
        {
            if (_currentWeaponIndex == weaponsCount - 1)
            {
                _currentWeaponIndex = 0;
            }
            else
            {
                _currentWeaponIndex++;
            }

            holsterContainer.SelectedSlot.Set(_currentWeaponIndex);
        }

        private void LateUpdate()
        {
            switch (currentState)
            {
                case HandsState.Idle:
#if UNITY_EDITOR
                    if (Input.GetMouseButton(1))
                    {
                        // MoveHands();
                        currentState = HandsState.StartFire;
                    }
#else
                    if (controls.StartClickingAttack)
                    {
                        currentState = HandsState.StartFire;
                    }
#endif
                    break;

                case HandsState.StartFire:
                    Fire();

                    break;

                case HandsState.Firing:
                    playerController.Fire(_isSerial);

                    currentState = HandsState.EndFiring;

                    break;

                case HandsState.EndFiring:
                    if (controls.IsHoldingAttack)
                    {
                        currentState = HandsState.Serial;
                    }
                    else
                    {
                        currentState = HandsState.Idle;
                        _isSerial = false;
                    }

                    break;

                case HandsState.Serial:
                    currentState = HandsState.StartFire;
                    _isSerial = true;

                    if (controls.EndClickingAttack)
                    {
                        currentState = HandsState.Idle;

                        if (serialFiringRoutine != null)
                        {
                            StopCoroutine(serialFiringRoutine);
                        }
                    }

                    break;

                case HandsState.OnReloading:
                    if (serialFiringRoutine != null)
                    {
                        StopCoroutine(serialFiringRoutine);
                    }

                    // if (controls.IsHoldingAttack || controls.StartClickingAttack)
                    // {
                    //     MoveHands();
                    // }

                    break;

                case HandsState.EndReloading:
                    if (controls.IsHoldingAttack)
                    {
                        currentState = HandsState.Serial;
                    }
                    else
                    {
                        currentState = HandsState.Idle;
                    }

                    break;

                case HandsState.ChangingWeapon:
                    if (serialFiringRoutine != null)
                    {
                        StopCoroutine(serialFiringRoutine);
                    }

                    if (firingRoutine != null)
                    {
                        StopCoroutine(firingRoutine);
                    }

                    if (reloadRoutine != null)
                    {
                        StopCoroutine(reloadRoutine);
                    }

                    break;
            }

            // if (controls.IsHoldingAttack)
            // {
            //     MoveCrosshair();
            //     MoveHands();
            // }
        }

        public void TryReload()
        {
            playerController.Reload();
        }

        public void TryHeal()
        {
            playerController.Heal();
        }

        private void MoveHands()
        {
            var ray = cameraMain.ScreenPointToRay(Input.mousePosition);
            var point = handsTransform.forward + ray.direction;

            var lookRotation = Quaternion.LookRotation(point);
            var rawAngles = lookRotation.eulerAngles;

            lookRotation = Quaternion.Euler(rawAngles);
            handsTransform.rotation = lookRotation;
        }

        private void OnShoot()
        {
        }

        private void MoveCrosshair()
        {
            Vector2 movePos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvasRect,
                Input.mousePosition, parentCanvas.worldCamera,
                out movePos);

            crosshair.position = parentCanvas.transform.TransformPoint(movePos);
        }

        private void Fire()
        {
            if (isFirstStrike)
            {
                if (OnFirstStrike != null && OnFirstStrike.Invoke())
                {
                    isFirstStrike = false;
                }
            }

            currentState = HandsState.Firing;
        }
    }
}