using System.Numerics;
using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.Input;
using Raylib_cs;

namespace FrinkyGame.Scripts;

/// <summary>
/// Basic WASD + jump driver for CharacterControllerComponent.
/// </summary>
public class CharacterControllerExample : Component
{
    private CharacterControllerComponent? _controller;

    public override void Start()
    {
        _controller = Entity.GetComponent<CharacterControllerComponent>();
    }

    public override void Update(float dt)
    {
        if (_controller == null)
            return;

        var move = Vector2.Zero;

        if (Input.IsKeyDown(KeyboardKey.W))
            move.Y += 1f;
        if (Input.IsKeyDown(KeyboardKey.S))
            move.Y -= 1f;
        if (Input.IsKeyDown(KeyboardKey.A))
            move.X -= 1f;
        if (Input.IsKeyDown(KeyboardKey.D))
            move.X += 1f;

        _controller.SetMoveInput(move);

        if (Input.IsKeyPressed(KeyboardKey.Space))
            _controller.Jump();
    }
}
