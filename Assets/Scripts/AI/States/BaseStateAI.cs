using Animations;

namespace AI
{
    public abstract class BaseStateAI
    {
        public BaseEnemy baseEnemy;
        public EnemyAnimationController animatonController;

        public BaseStateAI prevState;

        public virtual void EnterState() { }
        public virtual void ExitState() { }
        public virtual void StateUpdate() { }
        public virtual void StateFixedUpdate() { }
        public virtual void StateLateUpdate() { }
        public virtual void StateOnEnable() { }
        public virtual void StateOnDisable() { }
    }
}