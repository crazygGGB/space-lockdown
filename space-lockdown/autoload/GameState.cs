using Godot;

namespace SpaceLockdown;

public partial class GameState : Node
{
    [Signal]
    public delegate void ProgressChangedEventHandler(int newProgress);

    [Signal]
    public delegate void SceneChangedEventHandler(string sceneId);

    [Signal]
    public delegate void GameRestartedEventHandler();

    public static GameState Instance { get; private set; } = null!;

    private string _currentSceneId = "SC-00";
    private int _progress;

    [Export]
    public int MaxProgress { get; set; } = 10;

    public int Steps { get; set; }
    public Godot.Collections.Dictionary Visited { get; set; } = new();
    public bool IsGameOver { get; set; }
    public bool IsWin { get; set; }

    public string CurrentSceneId
    {
        get => _currentSceneId;
        set
        {
            _currentSceneId = value;
            Visited[value] = true;
            EmitSignal(SignalName.SceneChanged, value);
        }
    }

    public int Progress
    {
        get => _progress;
        set
        {
            _progress = Mathf.Clamp(value, 0, MaxProgress);
            EmitSignal(SignalName.ProgressChanged, _progress);
        }
    }

    public override void _Ready()
    {
        Instance = this;
        Visited[_currentSceneId] = true;
    }

    public void AddProgress(int delta)
    {
        Progress = Mathf.Clamp(Progress + delta, 0, MaxProgress);
    }

    public void IncrementStep()
    {
        Steps++;
    }

    public bool CheckWin(string targetSceneId)
    {
        return targetSceneId == "SC-10";
    }

    public void Reset()
    {
        _currentSceneId = "SC-00";
        _progress = 0;
        Steps = 0;
        Visited = new() { { "SC-00", true } };
        IsGameOver = false;
        IsWin = false;
        EmitSignal(SignalName.GameRestarted);
    }
}
