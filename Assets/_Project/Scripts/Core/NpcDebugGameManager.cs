using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class NpcDebugGameManager : MonoBehaviour
{
    [Header("Debug Controls")]
#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode nextNpcLegacyKey = KeyCode.N;
    [SerializeField] private KeyCode endNpcLegacyKey = KeyCode.E;
#endif

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private Key nextNpcInputSystemKey = Key.N;
    [SerializeField] private Key endNpcInputSystemKey = Key.E;
#endif

    private void Update()
    {
        if (WasNextNpcPressed())
        {
            NpcMovementEvents.RaiseNextNpc();
        }

        if (WasEndNpcPressed())
        {
            NpcMovementEvents.RaiseEndNpc();
        }
    }

    private bool WasNextNpcPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[nextNpcInputSystemKey].wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(nextNpcLegacyKey);
#else
        return false;
#endif
    }

    private bool WasEndNpcPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[endNpcInputSystemKey].wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(endNpcLegacyKey);
#else
        return false;
#endif
    }
}
