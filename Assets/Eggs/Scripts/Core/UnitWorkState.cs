namespace Eggs.Gameplay
{
    public enum UnitWorkState
    {
        Idle = 0,
        Gathering = 1,
        Breeding = 2,
        // Defending is retained only for legacy M1A compatibility.
        // Current formal gameplay derives automatic defense eligibility from Idle.
        Defending = 3
    }
}
