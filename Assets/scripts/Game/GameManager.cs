using UnityEngine;

/// <summary>
/// Lives in the Gameplay scene. Call Win() or Lose() from your level logic
/// (e.g. a goal trigger or the death handler) to move to the end scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>True once the Shaman boss has been defeated. The Win Zone is
    /// locked until this is true.</summary>
    public bool BossDefeated { get; private set; }

    void Awake()
    {
        Instance = this;
        BossDefeated = false;
    }

    public void MarkBossDefeated()
    {
        BossDefeated = true;
        Debug.Log("Boss defeated - the throne is now open.");
    }

    public void Win() => GameFlow.LoadWin();
    public void Lose() => GameFlow.LoadLose();
}
