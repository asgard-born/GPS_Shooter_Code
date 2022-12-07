using Fight.Enemies.Abstract;
using Fight.Enemies.Behavior;
using Fight.Enemies.State.Enums;

namespace Fight.Enemies.State
{
    public class DeathEnemyState : EnemyState
    {
        private DeathStep currentStep;
        
        public override void Init(EnemyBehavior context)
        {
            base.Init(context);
            currentStep = DeathStep.In;
        }

        public override void Process()
        {
        }
    }
}