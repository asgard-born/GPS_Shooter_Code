using Fight.Enemies.Behavior;

namespace Fight.Enemies.Abstract
{
    public abstract class EnemyState
    {
        protected EnemyBehavior context;
        protected Enemy enemy;

        public abstract void Process();

        public virtual void Init(EnemyBehavior context)
        {
            this.context = context;
            enemy = context.Enemy;
        }
    }
}