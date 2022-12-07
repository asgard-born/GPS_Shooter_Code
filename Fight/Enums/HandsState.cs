namespace Fight.Enums
{
    public enum HandsState : byte
    {
        Idle = 0,
        StartFire = 10,
        Firing = 20,
        EndFiring = 30,
        Serial = 40,
        OnReloading = 50,
        EndReloading = 60,
        ChangingWeapon = 70,
    }
}