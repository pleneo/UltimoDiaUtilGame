#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class GameplaySystemsSetupMenu
{
    [MenuItem("Ultimo Dia Util/Setup/Create Gameplay Systems")]
    public static void CreateGameplaySystems()
    {
        var systemsObject = GameObject.Find("Systems");
        if (systemsObject == null)
        {
            systemsObject = new GameObject("Systems");
            Undo.RegisterCreatedObjectUndo(systemsObject, "Create Gameplay Systems");
        }

        EnsureComponent<GameManager>(systemsObject);
        EnsureComponent<DayManager>(systemsObject);
        EnsureComponent<CaseManager>(systemsObject);
        EnsureComponent<DocumentManager>(systemsObject);
        EnsureComponent<EconomyManager>(systemsObject);
        EnsureComponent<UIManager>(systemsObject);

        Selection.activeGameObject = systemsObject;
        EditorGUIUtility.PingObject(systemsObject);

        var npcMovementController = Object.FindFirstObjectByType<NpcMovementController>();
        if (npcMovementController == null)
        {
            EditorUtility.DisplayDialog(
                "Gameplay Systems criados",
                "Criei o objeto Systems com os managers principais.\n\nAinda falta ter um NPC na cena com NpcMovementController para a fila animada funcionar.",
                "Ok"
            );
            return;
        }

        EditorUtility.DisplayDialog(
            "Gameplay Systems criados",
            "Criei o objeto Systems com os managers principais.\n\nAgora preencha GameManager > Day Sequence e DayManager > NPC Random Visuals no Inspector.",
            "Ok"
        );
    }

    private static T EnsureComponent<T>(GameObject targetObject) where T : Component
    {
        var component = targetObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return Undo.AddComponent<T>(targetObject);
    }
}
#endif
