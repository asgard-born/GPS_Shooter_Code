using Fight.Enemies.Abstract;
using Fight.Enemies.Behavior;
using Fight.Enemies.State.Enums;
using UnityEngine;

namespace Fight.Enemies.State
{
    public class AttackEnemyState : EnemyState
    {
        private AttackStep currentStep;
        private float idleTimeCounter;

        public override void Init(EnemyBehavior context)
        {
            base.Init(context);
            currentStep = AttackStep.In;
        }

        public override void Process()
        {
            switch (currentStep)
            {
                case AttackStep.In:
                    enemy.LookToLerp(enemy.PlayerController.transform.position);
                    currentStep = AttackStep.Rotating;

                    break;

                case AttackStep.Rotating:
                    if (!enemy.isRotating)
                    {
                        currentStep = AttackStep.StartAttack;
                    }

                    break;

                case AttackStep.StartAttack:
                    enemy.StartAttack();
                    currentStep = AttackStep.ContinueAttack;

                    break;

                case AttackStep.ContinueAttack:
                    if (!enemy.IsAttacking)
                    {
                        currentStep = AttackStep.WaitForAttack;
                    }

                    break;

                case AttackStep.WaitForAttack:
                    if (CompleteIdle(enemy.AttackCooldown))
                    {
                        currentStep = AttackStep.StartAttack;
                    }

                    break;
            }
        }

        private bool CompleteIdle(float waitTimer)
        {
            if (idleTimeCounter < waitTimer)
            {
                idleTimeCounter += Time.fixedDeltaTime;
                enemy.Idle();

                return false;
            }
            else
            {
                idleTimeCounter = 0;

                return true;
            }
        }

        private bool CompleteWaiting(float waitTimer)
        {
            if (idleTimeCounter < waitTimer)
            {
                idleTimeCounter += Time.fixedDeltaTime;

                return false;
            }
            else
            {
                idleTimeCounter = 0;

                return true;
            }
        }
    }
}