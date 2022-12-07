using Fight.Enemies.Abstract;
using Fight.Enemies.Behavior;
using Fight.Enemies.Enums;
using Fight.Enemies.State.Enums;
using UnityEngine;
using UnityEngine.AI;

namespace Fight.Enemies.State
{
    public class MovingEnemyState : EnemyState
    {
        private Enemy enemy;
        private EnemyWaypoints waypoints;
        private NavMeshAgent navMesh;
        private MoveStep currentStep;
        private float waitTimeCounter;
        private float distanceToStop = 0.002f;

        public override void Init(EnemyBehavior context)
        {
            base.Init(context);
            enemy = context.Enemy;
            waypoints = enemy.Waypoints;
            navMesh = enemy.NavMeshAgent;
            currentStep = MoveStep.In;
        }

        public override void Process()
        {
            switch (currentStep)
            {
                case MoveStep.In:
                    context.Enemy.LookToLerp(enemy.PlayerController.transform.position);
                    currentStep = MoveStep.Rotating;

                    break;

                case MoveStep.Rotating:
                    if (!enemy.isRotating)
                    {
                        currentStep = MoveStep.Preparing;
                    }

                    break;

                case MoveStep.Preparing:
                    if (enemy.TrySetNearestAttackPoint())
                    {
                        currentStep = MoveStep.MovingToAttackPoint;
                        enemy.MoveToPoint();
                    }
                    else if (enemy.TrySetNearestWaitingPoint())
                    {
                        currentStep = MoveStep.MovingToWaitPoint;
                        enemy.MoveToPoint();
                    }

                    break;

                case MoveStep.MovingToWaitPoint:
                    if (navMesh.pathPending)
                    {
                        break;
                    }

                    if (navMesh.pathStatus == NavMeshPathStatus.PathComplete && navMesh.remainingDistance <= distanceToStop)
                    {
                        enemy.Idle();
                        currentStep = MoveStep.WaitForAttack;
                    }

                    break;

                case MoveStep.WaitForAttack:
                    if (CompleteWaiting())
                    {
                        currentStep = MoveStep.PrepareToMoveForAttack;
                    }

                    break;

                case MoveStep.PrepareToMoveForAttack:
                    if (enemy.TrySetNearestAttackPoint())
                    {
                        currentStep = MoveStep.MovingToAttackPoint;
                        enemy.MoveToPoint(MovingType.Walking);
                    }
                    else
                    {
                        currentStep = MoveStep.WaitForAttack;
                    }

                    break;

                case MoveStep.MovingToAttackPoint:
                    if (navMesh.pathPending)
                        break;

                    if (navMesh.pathStatus == NavMeshPathStatus.PathComplete && navMesh.remainingDistance <= distanceToStop)
                    {
                        currentStep = MoveStep.Out;
                    }

                    if (navMesh.speed == 0)
                    {
                        
                    }

                    break;

                case MoveStep.Out:
                    enemy.Idle();
                    context.ChangeState(new AttackEnemyState());

                    break;
            }
        }

        private bool CompleteWaiting()
        {
            if (waitTimeCounter < enemy.RunCooldown)
            {
                waitTimeCounter += Time.fixedDeltaTime;

                return false;
            }
            else
            {
                waitTimeCounter = 0;

                return true;
            }
        }
    }
}