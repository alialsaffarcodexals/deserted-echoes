public class GoblinController : EnemyControllerBase
{
    protected override void Awake()
    {
        base.Awake();
        SetAttackAnimationMode(AttackAnimationMode.Directional);
    }
}
