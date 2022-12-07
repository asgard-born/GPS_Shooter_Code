using System.Collections;
using Fight.Enemies.Abstract;
using Fight.Enemies.Enums;
using UnityEngine;

namespace Fight.Enemies.Concrete
{
    public class ZombieHuman : Enemy
    {
        private static readonly int isWalkingHash = Animator.StringToHash("is_walking");
        private static readonly int isRunningHash = Animator.StringToHash("is_running");
        private static readonly int movingTypeHash = Animator.StringToHash("moving_type");
        private static readonly int idleHash = Animator.StringToHash("idle");
        private static readonly int impactHash = Animator.StringToHash("impact");
        private static readonly int attackHash = Animator.StringToHash("attack");

        private CapsuleCollider collider;

        public override void MoveToPoint(MovingType newMovingType = MovingType.None)
        {
            if (newMovingType != MovingType.None)
            {
                if (newMovingType == MovingType.Walking)
                {
                    Walk();
                }
                else if (newMovingType == MovingType.Running)
                {
                    Run();
                }
            }
            else
            {
                if (MovingType == MovingType.Walking)
                {
                    Walk();
                }
                else if (MovingType == MovingType.Running)
                {
                    Run();
                }
            }
        }

        public override void Walk()
        {
            Animator.SetBool(isRunningHash, false);
            Animator.SetBool(isWalkingHash, true);

            var movingNumber = Random.Range(0, 2);
            Animator.SetInteger(movingTypeHash, movingNumber);
            
            NavMeshAgent.speed = WalkSpeed;
            NavMeshAgent.destination = CurrentPoint.Position;
            isMovingToPoint = true;
        }

        public override void Run()
        {
            Animator.SetBool(isWalkingHash, false);
            Animator.SetBool(isRunningHash, true);

            var movingNumber = Random.Range(0, 2);
            Animator.SetInteger(movingTypeHash, movingNumber);
            
            NavMeshAgent.speed = RunSpeed;
            NavMeshAgent.destination = CurrentPoint.Position;
            isMovingToPoint = true;
        }

        public override void Idle()
        {
            isMovingToPoint = false;
            
            Animator.SetBool(isWalkingHash, false);
            Animator.SetBool(isRunningHash, false);
            Animator.SetBool(attackHash, false);
            Animator.SetBool(idleHash, true);
        }

        protected override void AnimateAttack()
        {
            transform.LookAt(playerController.transform);
            Animator.SetBool(attackHash, true);
        }

        protected override void PlayHitAnim()
        {
            Animator.SetBool(impactHash, true);
            StartCoroutine(StartHitCooldown());
        }

        private IEnumerator StartHitCooldown()
        {
            IsHitting = true;
            var prevSpeed = NavMeshAgent.speed;
            NavMeshAgent.speed = 0;

            yield return new WaitForSeconds(HitCooldown);

            NavMeshAgent.speed = prevSpeed;
            IsHitting = false;
            Animator.SetBool(impactHash, false);
        }
    }
}