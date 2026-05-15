public class OrcController : EnemyControllerBase
{
    protected override void Awake()
    {
        base.Awake();
        SetAttackAnimationMode(AttackAnimationMode.Directional);
    }
}
