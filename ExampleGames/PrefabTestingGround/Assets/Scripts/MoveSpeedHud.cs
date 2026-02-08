using FrinkyEngine.Core.Components;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.UI;
using PrefabTestingGround.Scripts;
using System.Numerics;

namespace PrefabTestingGround.Scripts;

public class MoveSpeedHud : Component
{
    private float Speed;
    private float SpeedPercent;
    private CharacterControllerComponent? CharacterControllerComponent;

    public override void Start()
    {
        base.Start();

        CharacterControllerComponent = Entity.GetComponent<CharacterControllerComponent>();
    }

    private void DrawHud(UiContext ctx)
    {
        using var panel = ctx.Panel("HUD Sample", new UiPanelOptions
        {
            Position = new Vector2(16f, 16f),
            Size = new Vector2(300f, 0f),
            HasTitleBar = false,
            NoBackground = false,
            Movable = true,
            AutoResize = true,
            Resizable = false,
            Scrollbar = false
        });

        if (!panel.IsVisible)
            return;

        ctx.Text("Player Speed", 16);
        ctx.ProgressBar("health_bar", SpeedPercent, overlayText: $"{(int)(Speed*100)} cm/s");
    }
    public override void Update(float dt)
    {
        if(CharacterControllerComponent != null)
        {
            var Velocity = CharacterControllerComponent.GetVelocity();
            Velocity.Y = 0;
            Speed = Velocity.Length();
            SpeedPercent = Speed / (CharacterControllerComponent.MoveSpeed * 8);
        }
         
        UI.Draw(DrawHud);
    }
}