using System.Numerics;
using FrinkyEngine.Core.Assets;
using FrinkyEngine.Core.ECS;
using FrinkyEngine.Core.UI;

namespace PrefabTestingGround.Scripts;

[ComponentCategory("UI")]
public class HudSampleComponent : Component
{
    public bool Visible { get; set; } = true;
    public float BaseFontPixels { get; set; } = 18f;
    public float Health01 { get; set; } = 1f;
    public bool AutoDrainHealth { get; set; } = true;
    public float DrainPerSecond { get; set; } = 0.15f;
    public string ImageTexturePath { get; set; } = "Textures/grid_lightchecker.PNG";

    private float _stamina01 = 1f;
    private float _pulseTime;
    private bool _imageLoadAttempted;
    private UiImageHandle _image;

    public override void Update(float dt)
    {
        if (!Visible)
            return;

        BaseFontPixels = Math.Clamp(BaseFontPixels, 10f, 48f);
        Health01 = Math.Clamp(Health01, 0f, 1f);
        DrainPerSecond = Math.Clamp(DrainPerSecond, 0f, 1f);

        if (AutoDrainHealth)
            Health01 = MathF.Max(0f, Health01 - DrainPerSecond * MathF.Max(0f, dt));

        _pulseTime += dt;
        _stamina01 = 0.5f + 0.5f * MathF.Sin(_pulseTime * 2f);

        EnsureImageLoaded();
        UI.Draw(DrawHud);
    }

    private void DrawHud(UiContext ctx)
    {
        using var panel = ctx.Panel("HUD Sample", new UiPanelOptions
        {
            Position = new Vector2(16f, 16f),
            HasTitleBar = false,
            NoBackground = false,
            Movable = true,
            AutoResize = true,
            Resizable = false,
            Scrollbar = false
        });

        if (!panel.IsVisible)
            return;

        ctx.Text("Frinky UI Sample", BaseFontPixels + 8f);
        ctx.Text(
            "Immediate mode wrapper + dynamic font sizes",
            BaseFontPixels - 4f,
            new UiStyle { TextColor = new Vector4(0.78f, 0.83f, 0.92f, 1f) });

        ctx.Separator();

        ctx.ProgressBar("health_bar", Health01, overlayText: $"Health {(int)(Health01 * 100f)}%");
        ctx.ProgressBar("stamina_bar", _stamina01, overlayText: $"Stamina {(int)(_stamina01 * 100f)}%");
        ctx.Spacer(height: 4f);

        using (var actionRow = ctx.Horizontal("action_row", 2))
        {
            if (actionRow.IsVisible)
            {
                ctx.NextCell();
                if (ctx.Button("damage_btn", "Take 10 Damage", BaseFontPixels - 4f))
                    Health01 = MathF.Max(0f, Health01 - 0.1f);

                ctx.NextCell();
                if (ctx.Button("heal_btn", "Heal 10", BaseFontPixels - 4f))
                    Health01 = MathF.Min(1f, Health01 + 0.1f);
            }
        }

        var autoDrain = AutoDrainHealth;
        if (ctx.Checkbox("auto_drain_toggle", "Auto Drain Health", ref autoDrain, BaseFontPixels - 4f))
            AutoDrainHealth = autoDrain;

        var drain = DrainPerSecond;
        if (ctx.SliderFloat("drain_slider", "Drain / sec", ref drain, 0f, 1f, BaseFontPixels - 4f))
            DrainPerSecond = drain;

        var health = Health01;
        if (ctx.SliderFloat("health_slider", "Manual Health", ref health, 0f, 1f, BaseFontPixels - 4f))
            Health01 = health;

        var fontSize = BaseFontPixels;
        if (ctx.SliderFloat("font_slider", "Font Size", ref fontSize, 10f, 48f, BaseFontPixels - 4f))
            BaseFontPixels = fontSize;

        if (_image.IsValid)
        {
            ctx.Spacer(height: 6f);
            ctx.Text("Image Widget", BaseFontPixels - 4f);
            ctx.Image(_image, new Vector2(96f, 96f));
        }

        ctx.Spacer(height: 6f);
        var capture = UI.InputCapture;
        ctx.Text(
            $"Input Capture  Mouse:{capture.WantsMouse}  Keyboard:{capture.WantsKeyboard}  Text:{capture.WantsTextInput}",
            BaseFontPixels - 6f,
           
            new UiStyle { TextColor = new Vector4(0.70f, 0.76f, 0.86f, 1f) });
    }

    private void EnsureImageLoaded()
    {
        if (_imageLoadAttempted)
            return;

        _imageLoadAttempted = true;

        if (string.IsNullOrWhiteSpace(ImageTexturePath))
            return;

        var texture = AssetManager.Instance.LoadTexture(ImageTexturePath);
        if (texture.Id != 0)
            _image = UiImageHandle.FromTexture(texture);
    }
}
