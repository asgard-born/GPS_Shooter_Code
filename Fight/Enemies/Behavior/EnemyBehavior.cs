using Fight.Enemies.Abstract;
using Fight.Enemies.State;

namespace Fight.Enemies.Behavior
{
    public class EnemyBehavior
    {
        public Enemy Enemy => enemy;

        protected Enemy enemy;
        protected EnemyState currentState;

        public EnemyState CurrentState => currentState;

        public EnemyBehavior(Enemy enemy, EnemyState firstState)
        {
            this.enemy = enemy;

            ChangeState(firstState);
        }

        public void ChangeState(EnemyState newState)
        {
            currentState = newState;
            currentState.Init(this);
        }

        public void Process()
        {
            currentState.Process();
        }

        public void OnDeath()
        {
            ChangeState(new DeathEnemyState());
        }
    }
}