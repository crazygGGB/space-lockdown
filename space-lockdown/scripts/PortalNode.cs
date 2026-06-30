using Godot;

namespace SpaceLockdown;

public partial class PortalNode : Area2D
{
    [Signal]
    public delegate void TeleportTriggeredEventHandler(string targetSceneId, string targetScenePath);

    [Export]
    public string PortalId { get; set; } = "";

    [Export]
    public string TargetSceneId { get; set; } = "SC-00";

    [Export]
    public string TargetScenePath { get; set; } = "";

    [Export]
    public string HintText { get; set; } = "";

    [Export]
    public int ProgressDelta { get; set; }

    [Export]
    public int RequiredProgress { get; set; } = -1;

    [Export]
    public Color NodeColor { get; set; } = new Color(0, 1, 0.53f);

    public bool IsLocked { get; private set; }

    private GameState _gameState = null!;

    public override void _Ready()
    {
        AddToGroup("portal");
        _gameState = GetNode<GameState>("/root/GameState");
        UpdateLockState();
        _gameState.ProgressChanged += _ => UpdateLockState();
    }

    public void UpdateLockState()
    {
        IsLocked = RequiredProgress >= 0 && _gameState.Progress < RequiredProgress;
    }

    public bool IsUnlocked()
    {
        return !IsLocked;
    }

    public void Activate()
    {
        UpdateLockState();
        if (IsLocked)
            return;

        _gameState.AddProgress(ProgressDelta);
        _gameState.IncrementStep();
        _gameState.CurrentSceneId = TargetSceneId;

        if (TargetSceneId == "SC-10")
        {
            _gameState.IsWin = true;
            _gameState.IsGameOver = true;
        }

        EmitSignal(SignalName.TeleportTriggered, TargetSceneId, TargetScenePath);
    }
}
