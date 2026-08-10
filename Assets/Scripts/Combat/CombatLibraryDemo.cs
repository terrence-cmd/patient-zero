using UnityEngine;

/// <summary>
/// Demonstration-only: dumps the assigned move/stage libraries to the Console.
/// Attach to any GameObject, assign the library assets, enter Play Mode.
/// Does not simulate combat — it only proves the abstracted data loads.
/// </summary>
public class CombatLibraryDemo : MonoBehaviour
{
    [SerializeField] private MoveLibrary moveLibrary;
    [SerializeField] private StageLibrary stageLibrary;
    [SerializeField] private bool logOnStart = true;

    private void Start()
    {
        if (logOnStart)
            LogLibraries();
    }

    [ContextMenu("Log Combat Libraries")]
    public void LogLibraries()
    {
        if (moveLibrary == null)
            Debug.LogWarning("[CombatLibraryDemo] MoveLibrary is not assigned.");
        else
        {
            Debug.Log($"[CombatLibraryDemo] Move library '{moveLibrary.libraryId}' " +
                      $"({moveLibrary.Count} moves):");
            foreach (MoveDefinition move in moveLibrary.Enumerate())
                Debug.Log($"  • {move}");
        }

        if (stageLibrary == null)
            Debug.LogWarning("[CombatLibraryDemo] StageLibrary is not assigned.");
        else
        {
            Debug.Log($"[CombatLibraryDemo] Stage library '{stageLibrary.libraryId}' " +
                      $"({stageLibrary.Count} stages):");
            foreach (StageDefinition stage in stageLibrary.Enumerate())
                Debug.Log($"  • {stage}");
        }
    }
}
