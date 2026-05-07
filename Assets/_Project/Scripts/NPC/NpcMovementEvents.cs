using System;

public static class NpcMovementEvents
{
    public static event Action NextNpc;
    public static event Action EndNpc;
    public static event Action NpcArrivedCenter;

    public static void RaiseNextNpc()
    {
        NextNpc?.Invoke();
    }

    public static void RaiseEndNpc()
    {
        EndNpc?.Invoke();
    }

    public static void RaiseNpcArrivedCenter()
    {
        NpcArrivedCenter?.Invoke();
    }
}
