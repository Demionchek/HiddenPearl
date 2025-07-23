using AI.States;

namespace AI
{
    public class IdleStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            if (baseEnemy.patrolPoints.Count > 1)
            {
                baseEnemy.ChangeState<PatrolStateAI>();
            }
        }
    }
}