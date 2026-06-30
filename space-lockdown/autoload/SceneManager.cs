using Godot;
using System.Threading.Tasks;

namespace SpaceLockdown;

public partial class SceneManager : CanvasLayer
{
    [Signal]
    public delegate void TransitionStartedEventHandler();

    [Signal]
    public delegate void TransitionFinishedEventHandler();

    public static SceneManager Instance { get; private set; } = null!;

    private const float FadeDuration = 0.35f;
    private bool _isTransitioning;
    private ColorRect _colorRect = null!;

    public override void _Ready()
    {
        Instance = this;
        _colorRect = new ColorRect
        {
            Name = "ColorRect",
            Color = Colors.Black,
            Modulate = new Color(1, 1, 1, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        AddChild(_colorRect);
        _colorRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }

    public async Task FadeOut()
    {
        if (_isTransitioning)
            return;
        _isTransitioning = true;
        EmitSignal(SignalName.TransitionStarted);

        var tween = CreateTween().SetParallel(false);
        tween.TweenProperty(_colorRect, "modulate:a", 1.0, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task FadeIn()
    {
        if (!_isTransitioning)
            return;

        var tween = CreateTween().SetParallel(false);
        tween.TweenProperty(_colorRect, "modulate:a", 0.0, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
