namespace Fight.Enemies.State.Enums
{
    public enum MoveStep : byte
    {
        In,
        Out,
        Preparing,
        WaitForAttack,
        MovingToAttackPoint,
        MovingToWaitPoint,
        PrepareToMoveForAttack,
        Rotating
    }
}