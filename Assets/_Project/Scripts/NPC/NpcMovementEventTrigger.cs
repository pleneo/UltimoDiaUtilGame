using UnityEngine;

public class NpcMovementEventTrigger : MonoBehaviour
{
    public void TriggerNextNpc()
    {
        NpcMovementEvents.RaiseNextNpc();
    }

    public void TriggerEndNpc()
    {
        NpcMovementEvents.RaiseEndNpc();
    }
}
