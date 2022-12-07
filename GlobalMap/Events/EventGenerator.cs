using System;
using System.Collections;
using System.Linq;
using Configs;
using GlobalMap.Events.Essences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GlobalMap.Events
{
    public class EventGenerator : SerializedMonoBehaviour
    {
        [SerializeField] private EventAndChance[] fightConfigs;

        [Space] [Space] [Space] [SerializeField]
        private FightTypeEventPrefab[] fightTypesPrefabs;

        [SerializeField] private MoveAvatar _moveAvatar;
        [SerializeField] private Button checkbtn;
        [SerializeField] private Transform _player;
        [SerializeField] private float startCreateEventsDelay = 3f;

        [SerializeField] private float _createEventStep = .2f;
        [SerializeField] private LayerMask eventLayer;
        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private int waterCheckingRadius = 10;

        [Space] [Space] [SerializeField] private int _maxEventsNearby = 4;
        [SerializeField] private float _radiusToCreateMin = 20f;
        [SerializeField] private float _radiusToCreateMax = 50f;
        [SerializeField] private float _checkingNewEventsTimer = 3f;
        [SerializeField] private float _additionalRadiusForChecking = 10f;
        [SerializeField] private float _distanceBetweenEvents = 25f;

        private Coroutine eventsRoutine;
        private Vector3 _lastPointWithMaxEvents;
        private bool _isLastFilled;
        private int previousIndex;
        public event Action<FightConfig, Transform> OnFightClick;

        private void Awake()
        {
            // checkbtn.onClick.AddListener(() => CheckForEventsNearby());
            _moveAvatar.OnAvatarPositioned += () => eventsRoutine = StartCoroutine(CreateEventsRoutine());
        }

        private void OnDisable()
        {
            _isLastFilled = false;
            _lastPointWithMaxEvents = Vector3.zero;
            StopCoroutine(eventsRoutine);
        }

        private IEnumerator CreateEventsRoutine()
        {
            yield return new WaitForSeconds(startCreateEventsDelay);

            while (true)
            {
                var eventsCountNearby = CheckForEventsNearbyForPlayer();

                if (eventsCountNearby < _maxEventsNearby)
                {
                    while (!TryCreateEvent())
                    {
                        yield return null;
                    }

                    _isLastFilled = false;

                    yield return new WaitForSeconds(_createEventStep);
                }
                else
                {
                    if (!_isLastFilled)
                    {
                        _lastPointWithMaxEvents = _player.position;
                        _isLastFilled = true;
                    }

                    yield return new WaitForSeconds(_checkingNewEventsTimer);
                }
            }
        }

        private static Vector2 RandomPointWithinCircle(float radiusToCreateMin, float _radiusToCreateMax)
        {
            var random = Random.Range(radiusToCreateMin, _radiusToCreateMax);
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var x = Mathf.Sin(angle) * random;
            var y = Mathf.Cos(angle) * random;

            return new Vector2(x, y);
        }

        private bool TryCreateEvent()
        {
            var point = RandomPointWithinCircle(_radiusToCreateMin, _radiusToCreateMax);

            var eventPosition = _player.position + new Vector3(point.x, 3, point.y);

            if (CheckForEventsNearby(eventPosition) > 0)
            {
                return false;
            }

            if (CheckForWater(eventPosition))
            {
                return false;
            }

            var createdEvent = CreateEvent(eventPosition);

            Debug.Log($"CREATED EVENT ON: {eventPosition}");

            createdEvent.OnMouseDownEvent += OnFightEventClick;

            return true;
        }

        private bool CheckForWater(Vector3 eventPosition)
        {
            var colliders = Physics.OverlapSphere(eventPosition, waterCheckingRadius, waterLayer);

            if (colliders.Length > 0)
            {
                return true;
            }

            return false;
        }

        private EventPrefab CreateEvent(Vector3 eventPosition)
        {
            var chance = Random.Range(5, 100);
            var orderedConfigs = fightConfigs.OrderByDescending(x => x.chancePercentage).ToArray();

            var allValues = fightConfigs.Sum(x => x.chancePercentage);

            FightConfig currentConfig = null;

            float previousPartOfProbability = 0;

            foreach (var config in orderedConfigs)
            {
                var part = config.chancePercentage / allValues;

                var probability = 100 * part;

                var newProbability = previousPartOfProbability + probability;

                if (chance > previousPartOfProbability && chance <= newProbability)
                {
                    currentConfig = config.fightConfigs;

                    break;
                }

                previousPartOfProbability += probability;
            }

            var prefab = fightTypesPrefabs.Where(x => x.fightType == currentConfig.FightType).Select(x => x.eventPrefab).First();;

            var createdEvent = Instantiate(prefab, eventPosition, Quaternion.identity);
            createdEvent.Init(currentConfig);

            return createdEvent;
        }

        private void OnFightEventClick(FightConfig fightConfig, Transform transform)
        {
            OnFightClick?.Invoke(fightConfig, transform);
        }

        private void Update()
        {
            DebugExtension.DebugWireSphere(_player.position, Color.green, _radiusToCreateMax);
            Debug.DrawRay(_player.position, Vector3.forward * _radiusToCreateMax, Color.blue);
            Debug.DrawLine(_player.position, _player.position - Vector3.forward * _radiusToCreateMax, Color.red);
        }

        private int CheckForEventsNearby(Vector3 pos)
        {
            var results = new Collider[100];

            var checkingPoint = pos;

            return Physics.OverlapSphereNonAlloc(checkingPoint, _distanceBetweenEvents, results, eventLayer);
        }

        private int CheckForEventsNearbyForPlayer()
        {
            var results = new Collider[100];

            var checkingPoint = _player.position;

            if (_isLastFilled)
            {
                var distance = Vector3.Distance(_player.position, _lastPointWithMaxEvents);

                if (distance <= _distanceBetweenEvents)
                {
                    Debug.Log($"<color=green><b>cached point uses</b></color>");
                    checkingPoint = _lastPointWithMaxEvents;
                }
                else
                {
                    Debug.Log($"<color=red><b>new point uses</b></color>");
                }
            }

            return Physics.OverlapSphereNonAlloc(checkingPoint, _radiusToCreateMax + _additionalRadiusForChecking, results, eventLayer);
        }
    }
}