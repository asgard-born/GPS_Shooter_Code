namespace Fight.Enemies.State.Enums
{
    public enum AttackStep : byte
    {
        In,
        Out,
        StartAttack,
        ContinueAttack,
        WaitForAttack,
        Rotating,
    }
}