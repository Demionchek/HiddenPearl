namespace AI.States
{
    public class DoNothing : BaseStateAI
    {
        public override void EnterState()
        {
            baseEnemy.IgnoreEverything = true;
            //animatonController.SetAnimatorSpeed(0);
            animatonController.StopAllCoroutines();
            baseEnemy.StopAllCoroutines();
        }
    }
}