namespace AI
{
    public class EmptyStateAI : BaseStateAI
    {
        public override void EnterState()
        {
            //baseEnemy.StartCoroutine(baseEnemy.DetectionRoutine());
        }

        public override void ExitState() { }
        public override void StateUpdate() { }
        public override void StateFixedUpdate() { }



    }
}