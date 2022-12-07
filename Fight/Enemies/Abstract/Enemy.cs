using Fight.Enemies.Blood;
using Fight.Enemies.Concrete;
using Fight.PlayerSettings;
using Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fight.Enemies.Behavior;
using Fight.Enemies.Enums;
using Fight.Enemies.State;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Fight.Enemies.Abstract
{
    public abstract class Enemy : MonoBehaviour, IComparable<Enemy>
    {
        public EnemyType EnemyType;
        [SerializeField] protected MovingType movingType;
        public int MaxHealthValue;
        [Space] public Waypoint CurrentPoint;
        public float Distance;
        public float RunCooldown = .8f;
        public float AttackCooldown = .5f;
        public float HitCooldown = .3f;

        [SerializeField] protected bool isMovingToPoint;

        [SerializeField] private Material dissolveMaterial;
        [SerializeField] private MeshRenderer[] prefabRenderers;
        [SerializeField] private SkinnedMeshRenderer[] skinnedPrefabRenderers;
        [SerializeField] protected int attackDamage = 10;
        public float WalkSpeed = 1.5f;
        public float RunSpeed = 3f;
        [SerializeField] protected float rotateSpeed = .35f;

        [SerializeField] protected Slider healthBar;
        [SerializeField] protected EnemyBodyPart[] bodyParts;
        [SerializeField] protected float disableRigidbodyAndCollidersDelay = 1.5f;
        [SerializeField] protected float hideOnDeathDelay = 4f;
        [SerializeField] protected float dissolveDelay = 1f;
        [SerializeField] protected U10PS_DissolveOverTime dissolveManager;

        [NonSerialized] public NavMeshAgent NavMeshAgent;
        [NonSerialized] public Animator Animator;
        [NonSerialized] public bool isRotating;
        [NonSerialized] public bool IsDead;
        [NonSerialized] public bool IsAttacking;
        [NonSerialized] public bool IsHitting;

        [NonSerialized] protected PlayerController playerController;
        [NonSerialized] protected EnemyWaypoints waypoints;

        protected EnemyBehavior behavior;
        protected Camera camera;
        protected float health;

        [SerializeField] protected Rigidbody[] rigidbodies;
        [SerializeField] protected Collider[] colliders;

        protected Coroutine attackRoutine;

        public PlayerController PlayerController => playerController;
        public EnemyWaypoints Waypoints => waypoints;
        public bool IsMovingToPoint => isMovingToPoint;
        public MovingType MovingType => movingType;

        public event Action<Enemy> OnHit;
        public event Action<Enemy> OnDeath;

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            health = MaxHealthValue;
            NavMeshAgent.speed = RunSpeed;

            foreach (var bodyPart in bodyParts)
            {
                bodyPart.OnHit += Hit;
            }

            rigidbodies = GetComponentsInChildren<Rigidbody>();

            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.isKinematic = true;
            }
        }

        protected void Update()
        {
            healthBar.transform.LookAt(healthBar.transform.position + camera.transform.rotation * Vector3.forward);
        }

        public abstract void Idle();
        public abstract void Walk();
        public abstract void Run();

        public void Init(PlayerController playerController, EnemyWaypoints waypoints)
        {
            this.playerController = playerController;
            this.waypoints = waypoints;
        }

        public void InitBehaviour(EnemyState state, Camera camera)
        {
            behavior = new EnemyBehavior(this, state);
            this.camera = camera;
        }

        public void ChangeState<T>(T newState) where T : EnemyState
        {
            var stateType = typeof(T);

            if (behavior.CurrentState.GetType() == stateType)
            {
                return;
            }

            behavior.ChangeState(newState);
        }

        public void StartAttack()
        {
            IsAttacking = true;

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }

            attackRoutine = StartCoroutine(AttackProcessRoutine());
        }

        public virtual void Hit(HitInfo hitInfo, EnemyBodyPart enemyBodyPart = null)
        {
            if (IsDead) return;

            var damageMultiplier = 1f;

            if (enemyBodyPart != null)
            {
                damageMultiplier = enemyBodyPart.DamageMultiplier;

                switch (enemyBodyPart.Part)
                {
                    case BodyPartType.Head:
                        //TODO HEAD SHOT ВКТР
                        break;
                }
            }

            var damage = (int)(hitInfo.Damage * damageMultiplier);
            ReduceHealth(damage);
            BloodController.Instance.Get(hitInfo);

            healthBar.value = health / MaxHealthValue;

            if (health <= 0)
            {
                Death();
            }
            else
            {
                if (!IsAttacking && !IsHitting)
                {
                    PlayHitAnim();
                }
            }
        }

        public void LookToLerp(Vector3 positionTo)
        {
            isRotating = true;
            StartCoroutine(LookToRoutine(positionTo));
        }

        public void LookToPlayer()
        {
            transform.rotation = Quaternion.LookRotation(playerController.transform.position - transform.position);
        }

        public int CompareTo(Enemy other)
        {
            var otherDistanceToPlayer = Vector3.Distance(other.transform.position, playerController.transform.position);
            var thisDistanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

            if (thisDistanceToPlayer > otherDistanceToPlayer)
            {
                return 1;
            }
            else if (otherDistanceToPlayer > thisDistanceToPlayer)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        protected abstract void AnimateAttack();

        protected abstract void PlayHitAnim();

        protected virtual void Death()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }

            IsDead = true;
            NavMeshAgent.ResetPath();
            NavMeshAgent.enabled = false;
            healthBar.gameObject.SetActive(false);

            if (CurrentPoint != null)
            {
                CurrentPoint.IsFree = true;
            }

            CurrentPoint = null;

            // RAGDOLL
            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.isKinematic = false;
            }

            Animator.enabled = false;

            behavior.ChangeState(new DeathEnemyState());

            OnDeath?.Invoke(this);

            StartCoroutine(HideBodyRoutine());
        }

        public abstract void MoveToPoint(MovingType newMovingType = MovingType.None);

        public bool TrySetAnyNearestWaypoint()
        {
            return TrySetNearestAttackPoint() || TrySetNearestWaitingPoint();
        }

        public bool TrySetNearestAttackPoint()
        {
            return TrySetNearestPoint(waypoints.AttackWaypoints);
        }

        public bool TrySetNearestWaitingPoint()
        {
            return TrySetNearestPoint(waypoints.WaitingWaypoints);
        }

        private bool TrySetNearestPoint(IList<Waypoint> waypoints)
        {
            if (!waypoints.Any(p => p.IsFree)) return false;

            var nearestPoint = FindNearestFreePoint(transform, waypoints);

            if (nearestPoint == null)
            {
                return false;
            }
            
            SetNextPoint(nearestPoint);

            return true;
        }

        private Waypoint FindNearestFreePoint(Transform comparedTransform, IList<Waypoint> points)
        {
            var nearestPoint = points.FirstOrDefault(p => p.IsFree);

            if (nearestPoint == null) return null;

            var previousPointDistance = Vector3.Distance(nearestPoint.Position, comparedTransform.position);

            foreach (var point in points)
            {
                if (!point.IsFree) continue;

                var newPointDistance = Vector3.Distance(point.Position, comparedTransform.position);

                if (newPointDistance < previousPointDistance)
                {
                    nearestPoint = point;
                    previousPointDistance = newPointDistance;
                }
            }

            return nearestPoint;
        }

        public void SetNextPoint(Waypoint newPoint)
        {
            if (CurrentPoint != null)
            {
                CurrentPoint.IsFree = true;
            }

            newPoint.IsFree = false;
            CurrentPoint = newPoint;
        }

        public void ClearPoint()
        {
            if (CurrentPoint != null)
            {
                CurrentPoint.IsFree = true;
            }

            CurrentPoint = null;
        }

        protected virtual void FixedUpdate()
        {
            behavior?.Process();
        }

        private IEnumerator AttackProcessRoutine()
        {
            AnimateAttack();

            yield return new WaitForSeconds(AttackCooldown);

            //TODO ВКТР переделать на Fight Dealer - мидлвейр
            playerController.GetDamage(attackDamage);
            IsAttacking = false;
        }

        private IEnumerator HideBodyRoutine()
        {
            yield return new WaitForSeconds(disableRigidbodyAndCollidersDelay);

            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.isKinematic = true;
            }

            yield return new WaitForSeconds(hideOnDeathDelay);

            foreach (var renderer in prefabRenderers)
            {
                renderer.material = dissolveMaterial;
            }

            foreach (var renderer in skinnedPrefabRenderers)
            {
                renderer.material = dissolveMaterial;
            }

            dissolveManager.dissolve = true;

            yield return new WaitForSeconds(dissolveDelay);

            Destroy(gameObject);
        }

        private void ReduceHealth(int value)
        {
            health -= Mathf.Abs(value);
        }

        private IEnumerator LookToRoutine(Vector3 positionTo)
        {
            var lookRotation = Quaternion.LookRotation(positionTo - transform.position);
            var interpolant = 0f;

            while (interpolant < 1)
            {
                interpolant += Time.fixedDeltaTime * rotateSpeed;

                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, interpolant);

                yield return null;
            }

            isRotating = false;
        }
    }
}