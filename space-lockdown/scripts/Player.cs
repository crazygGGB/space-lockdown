using Godot;

namespace SpaceLockdown;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void InteractPressedEventHandler();

    private const float Speed = 150.0f;
    private static readonly Vector2 CollisionSize = new(12, 18);

    private Vector2 _facingDir = Vector2.Down;
    private bool _isMoving;
    private float _animFrame;

    public override void _Ready()
    {
        AddToGroup("player");
        var shape = new RectangleShape2D { Size = CollisionSize };
        var col = new CollisionShape2D { Shape = shape };
        AddChild(col);
    }

    public override void _Draw()
    {
        float px = -CollisionSize.X / 2;
        float py = -CollisionSize.Y;

        // Body
        DrawRect(new Rect2(px + 2, py + 8, 8, 10), new Color(0.267f, 0.8f, 1.0f));
        // Head
        DrawRect(new Rect2(px + 3, py + 1, 6, 8), new Color(1.0f, 0.8f, 0.6f));
        // Eyes
        int eyeOff = _facingDir.X < 0 ? -1 : _facingDir.X > 0 ? 1 : 0;
        DrawRect(new Rect2(px + 4 + eyeOff, py + 3, 2, 2), new Color(0.1f, 0.1f, 0.1f));
        DrawRect(new Rect2(px + 8 + eyeOff, py + 3, 2, 2), new Color(0.1f, 0.1f, 0.1f));
        // Legs
        float leg = _isMoving ? Mathf.Sin(_animFrame * 12.0f) * 2.0f : 0.0f;
        DrawRect(new Rect2(px + 2 + leg, py + 18, 3, 5), new Color(0.133f, 0.333f, 0.667f));
        DrawRect(new Rect2(px + 7 - leg, py + 18, 3, 5), new Color(0.133f, 0.333f, 0.667f));
        // Arms
        DrawRect(new Rect2(px - 1, py + 10, 2, 5), new Color(1.0f, 0.8f, 0.6f));
        DrawRect(new Rect2(px + 11, py + 10, 2, 5), new Color(1.0f, 0.8f, 0.6f));
        // Backpack
        DrawRect(new Rect2(px + 8, py + 9, 4, 5), new Color(0.133f, 0.333f, 0.667f));
    }

    public override void _PhysicsProcess(double delta)
    {
        var inputDir = Vector2.Zero;
        if (Input.IsActionPressed("move_left"))  inputDir.X -= 1;
        if (Input.IsActionPressed("move_right")) inputDir.X += 1;
        if (Input.IsActionPressed("move_up"))    inputDir.Y -= 1;
        if (Input.IsActionPressed("move_down"))  inputDir.Y += 1;

        _isMoving = inputDir != Vector2.Zero;
        if (_isMoving)
        {
            _facingDir = inputDir.Normalized();
            Velocity = _facingDir * Speed;
            _animFrame += (float)delta;
        }
        else
        {
            Velocity = Vector2.Zero;
            _animFrame = 0f;
        }

        MoveAndSlide();
        QueueRedraw();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            EmitSignal(SignalName.InteractPressed);
        }
    }
}
