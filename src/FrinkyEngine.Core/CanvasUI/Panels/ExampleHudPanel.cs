using FrinkyEngine.Core.CanvasUI.Styles;
using Raylib_cs;

namespace FrinkyEngine.Core.CanvasUI.Panels;

/// <summary>
/// Example HUD panel demonstrating CanvasUI features: layout, styling, labels,
/// buttons, events, pseudo-class hover feedback, and dynamic updates.
/// Attach via a component by calling <c>ExampleHudPanel.Create()</c> in <c>Start()</c>.
/// </summary>
public class ExampleHudPanel : Panel
{
    private Label _healthLabel = null!;
    private Panel _healthBar = null!;
    private Panel _healthBarFill = null!;
    private Label _scoreLabel = null!;
    private Label _messageLabel = null!;
    private int _score;
    private float _health = 100f;
    private float _messageTimer;

    public static ExampleHudPanel Create()
    {
        return CanvasUI.RootPanel.AddChild<ExampleHudPanel>();
    }

    public override void OnCreated()
    {
        // Root container fills the screen, doesn't block children from being hit
        Style.Width = Length.Pct(100);
        Style.Height = Length.Pct(100);
        Style.Padding = new Edges(16);

        // --- Top bar: health + score side by side ---
        var topBar = AddChild<Panel>(p =>
        {
            p.Style.FlexDirection = FlexDirection.Row;
            p.Style.AlignItems = AlignItems.Center;
            p.Style.Gap = 16;
        });

        // Health section
        var healthSection = topBar.AddChild<Panel>(p =>
        {
            p.Style.Gap = 4;
        });

        _healthLabel = healthSection.AddChild<Label>(l =>
        {
            l.Text = "Health: 100";
            l.Style.FontSize = 18f;
            l.Style.Color = new Color(200, 220, 200, 255);
        });

        // Health bar background
        _healthBar = healthSection.AddChild<Panel>(p =>
        {
            p.Style.Width = Length.Px(200);
            p.Style.Height = Length.Px(8);
            p.Style.BackgroundColor = new Color(40, 40, 40, 200);
            p.Style.BorderRadius = 4;
            p.Style.Overflow = Overflow.Hidden;
        });

        // Health bar fill
        _healthBarFill = _healthBar.AddChild<Panel>(p =>
        {
            p.Style.Width = Length.Pct(100);
            p.Style.Height = Length.Pct(100);
            p.Style.BackgroundColor = new Color(80, 200, 80, 255);
            p.Style.BorderRadius = 4;
        });

        // Score
        _scoreLabel = topBar.AddChild<Label>(l =>
        {
            l.Text = "Score: 0";
            l.Style.FontSize = 18f;
            l.Style.Color = new Color(255, 220, 100, 255);
        });

        // --- Spacer pushes buttons to the bottom ---
        AddChild<Panel>(p =>
        {
            p.Style.FlexGrow = 1;
        });

        // --- Bottom row: action buttons ---
        var bottomBar = AddChild<Panel>(p =>
        {
            p.Style.FlexDirection = FlexDirection.Row;
            p.Style.JustifyContent = JustifyContent.FlexEnd;
            p.Style.AlignItems = AlignItems.FlexEnd;
            p.Style.Gap = 8;
        });

        // Message label (shows feedback text)
        _messageLabel = bottomBar.AddChild<Label>(l =>
        {
            l.Text = "";
            l.Style.FontSize = 14f;
            l.Style.Color = new Color(180, 180, 180, 255);
            l.Style.FlexGrow = 1;
            l.Style.AlignSelf = AlignItems.Center;
        });

        // Heal button
        var healBtn = bottomBar.AddChild<Button>(b =>
        {
            b.Text = "Heal";
            b.Style.BackgroundColor = new Color(30, 100, 30, 255);
            b.Style.Color = new Color(200, 255, 200, 255);
            b.Style.BorderRadius = 6;
            b.Style.Padding = new Edges(8, 20, 8, 20);
        });
        healBtn.OnClick += _ =>
        {
            _health = MathF.Min(100f, _health + 25f);
            ShowMessage("Healed +25");
        };
        healBtn.OnMouseOver += _ =>
        {
            healBtn.Style.BackgroundColor = new Color(40, 130, 40, 255);
        };
        healBtn.OnMouseOut += _ =>
        {
            healBtn.Style.BackgroundColor = new Color(30, 100, 30, 255);
        };

        // Damage button
        var dmgBtn = bottomBar.AddChild<Button>(b =>
        {
            b.Text = "Take Damage";
            b.Style.BackgroundColor = new Color(120, 30, 30, 255);
            b.Style.Color = new Color(255, 200, 200, 255);
            b.Style.BorderRadius = 6;
            b.Style.Padding = new Edges(8, 20, 8, 20);
        });
        dmgBtn.OnClick += _ =>
        {
            _health = MathF.Max(0f, _health - 15f);
            _score += 10;
            ShowMessage("Ouch! -15 HP, +10 Score");
        };
        dmgBtn.OnMouseOver += _ =>
        {
            dmgBtn.Style.BackgroundColor = new Color(160, 40, 40, 255);
        };
        dmgBtn.OnMouseOut += _ =>
        {
            dmgBtn.Style.BackgroundColor = new Color(120, 30, 30, 255);
        };
    }

    public override void Tick(float dt)
    {
        // Update labels
        _healthLabel.Text = $"Health: {(int)_health}";
        _scoreLabel.Text = $"Score: {_score}";

        // Animate health bar width
        _healthBarFill.Style.Width = Length.Pct(_health);

        // Color the fill based on health
        _healthBarFill.Style.BackgroundColor = _health > 50
            ? new Color(80, 200, 80, 255)
            : _health > 25
                ? new Color(220, 180, 40, 255)
                : new Color(200, 50, 50, 255);

        // Fade out message
        if (_messageTimer > 0f)
        {
            _messageTimer -= dt;
            if (_messageTimer <= 0f)
                _messageLabel.Text = "";
        }
    }

    private void ShowMessage(string text)
    {
        _messageLabel.Text = text;
        _messageTimer = 2f;
    }
}
