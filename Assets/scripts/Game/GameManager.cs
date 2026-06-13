using UnityEngine;

/// <summary>
/// Lives in the Gameplay scene. Call Win() or Lose() from your level logic
/// (e.g. a goal trigger or the death handler) to move to the end scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void Win() => GameFlow.LoadWin();
    public void Lose() => GameFlow.LoadLose();
}
