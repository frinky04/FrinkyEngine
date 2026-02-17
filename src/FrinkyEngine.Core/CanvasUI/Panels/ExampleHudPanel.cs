namespace FrinkyEngine.Core.CanvasUI.Panels;

/// <summary>
/// Example HUD panel demonstrating CanvasUI features: CSS styling, layout,
/// labels, buttons, events, pseudo-class hover feedback, and dynamic updates.
/// Attach via a component by calling <c>ExampleHudPanel.Create()</c> in <c>Start()</c>.
/// </summary>
public class ExampleHudPanel : Panel
{
    private Label _healthLabel = null!;
    private ProgressBar _healthBar = null!;
    private Label _scoreLabel = null!;
    private Label _ammoLabel = null!;
    private Label _messageLabel = null!;
    private Label _dmgLabel = null!;
    private int _score;
    private float _health = 100f;
    private int _ammo = 30;
    private float _messageTimer;
    private bool _godMode;
    private float _dmgMultiplier = 1f;

    private const string Stylesheet = @"
        /* ── Root ── */
        .hud-root {
            width: 100%;
            height: 100%;
            padding: 20px;
        }

        /* ── Top bar: stats row ── */
        .top-bar {
            flex-direction: row;
            align-items: flex-start;
            gap: 12px;
        }

        .stat-card {
            background-color: rgba(10, 10, 14, 0.72);
            border: 1px #3a3a50;
            border-radius: 8px;
            padding: 10px 16px;
            gap: 6px;
        }

        .stat-card Label {
            font-size: 16px;
            color: #8888aa;
        }

        .stat-value {
            font-size: 28px;
            color: white;
        }

        /* Health bar */
        ProgressBar.health-bar {
            width: 180px;
            height: 6px;
            border-radius: 3px;
        }

        /* Score card accent */
        .score-card { border-color: #eab308; }
        .score-value { color: #fbbf24; }

        /* Ammo card accent */
        .ammo-card { border-color: #38bff8a1; }
        .ammo-value { color: #7dd3fc; }

        /* ── Controls row ── */
        .controls-row {
            flex-direction: row;
            align-items: center;
            gap: 20px;
            padding: 8px 0 8px 16px;
        }

        Checkbox.god-mode {
            color: #9999bb;
            font-size: 16px;
        }
        Checkbox.god-mode:checked {
            color: #4ade80;
        }

        .slider-group {
            flex-direction: row;
            align-items: center;
            gap: 8px;
        }
        .slider-group Label {
            font-size: 16px;
            color: #9999bb;
        }
        Slider.dmg-slider {
            width: 120px;
            height: 20px;
            color: #f87171;
        }
        .dmg-value {
            color: #fecaca;
            font-size: 16px;
        }

        /* ── Spacer ── */
        .spacer { flex-grow: 1; }

        /* ── Bottom bar ── */
        .bottom-bar {
            flex-direction: row;
            align-items: center;
            gap: 10px;
        }

        .message-label {
            font-size: 16px;
            color: #9999bb;
            flex-grow: 1;
        }

        /* ── Buttons ── */
        Button {
            border-radius: 6px;
            padding: 10px 24px;
            font-size: 18px;
            border-width: 1px;
        }

        .btn-heal {
            background-color: rgba(34, 120, 50, 0.85);
            border-color: #4ade80;
            color: #bbf7d0;
        }
        .btn-heal:hover {
            background-color: rgba(45, 160, 65, 0.95);
        }
        .btn-heal:active {
            background-color: rgba(22, 90, 36, 0.95);
        }

        .btn-damage {
            background-color: rgba(140, 30, 30, 0.85);
            border-color: #f87171;
            color: #fecaca;
        }
        .btn-damage:hover {
            background-color: rgba(180, 45, 45, 0.95);
        }
        .btn-damage:active {
            background-color: rgba(100, 20, 20, 0.95);
        }

        .btn-reload {
            background-color: rgba(30, 80, 150, 0.85);
            border-color: #60a5fa;
            color: #bfdbfe;
        }
        .btn-reload:hover {
            background-color: rgba(40, 105, 195, 0.95);
        }
        .btn-reload:active {
            background-color: rgba(20, 60, 115, 0.95);
        }
    ";

    public static ExampleHudPanel Create()
    {
        CanvasUI.LoadStyleSheet(Stylesheet);
        return CanvasUI.RootPanel.AddChild<ExampleHudPanel>();
    }

    public override void OnCreated()
    {
        AddClass("hud-root");

        // ── Top bar: stat cards ──
        var topBar = AddChild<Panel>(p => p.AddClass("top-bar"));

        // Health card
        var healthCard = topBar.AddChild<Panel>(p =>
        {
            p.AddClass("stat-card");
        });
        healthCard.AddChild<Label>(l => l.Text = "HEALTH");
        _healthLabel = healthCard.AddChild<Label>(l =>
        {
            l.Text = "100";
            l.AddClass("stat-value");
        });
        _healthBar = healthCard.AddChild<ProgressBar>(p =>
        {
            p.AddClass("health-bar");
            p.Value = 1f;
        });

        // Score card
        var scoreCard = topBar.AddChild<Panel>(p =>
        {
            p.AddClass("stat-card");
            p.AddClass("score-card");
        });
        scoreCard.AddChild<Label>(l => l.Text = "SCORE");
        _scoreLabel = scoreCard.AddChild<Label>(l =>
        {
            l.Text = "0";
            l.AddClass("stat-value");
            l.AddClass("score-value");
        });

        // Ammo card
        var ammoCard = topBar.AddChild<Panel>(p =>
        {
            p.AddClass("stat-card");
            p.AddClass("ammo-card");
        });
        ammoCard.AddChild<Label>(l => l.Text = "AMMO");
        _ammoLabel = ammoCard.AddChild<Label>(l =>
        {
            l.Text = "30";
            l.AddClass("stat-value");
            l.AddClass("ammo-value");
        });

        // ── Controls row: checkbox + slider ──
        var controlsRow = AddChild<Panel>(p => p.AddClass("controls-row"));

        var godMode = controlsRow.AddChild<Checkbox>(c =>
        {
            c.Text = "God Mode";
            c.AddClass("god-mode");
        });
        godMode.OnChanged += on => _godMode = on;

        var sliderGroup = controlsRow.AddChild<Panel>(p => p.AddClass("slider-group"));
        sliderGroup.AddChild<Label>(l => l.Text = "Damage");
        var dmgSlider = sliderGroup.AddChild<Slider>(s =>
        {
            s.AddClass("dmg-slider");
            s.Min = 0.5f;
            s.Max = 3f;
            s.Value = 0.2f; // maps to 1x
            s.Step = 0.1f;
        });
        _dmgLabel = sliderGroup.AddChild<Label>(l =>
        {
            l.Text = "1.0x";
            l.AddClass("dmg-value");
        });
        dmgSlider.OnChanged += v =>
        {
            _dmgMultiplier = v;
            _dmgLabel.Text = $"{v:F1}x";
        };

        // ── Spacer ──
        AddChild<Panel>(p => p.AddClass("spacer"));

        // ── Bottom bar: message + buttons ──
        var bottomBar = AddChild<Panel>(p => p.AddClass("bottom-bar"));

        _messageLabel = bottomBar.AddChild<Label>(l =>
        {
            l.Text = "";
            l.AddClass("message-label");
        });

        var healBtn = bottomBar.AddChild<Button>(b =>
        {
            b.Text = "Heal";
            b.AddClass("btn-heal");
        });
        healBtn.OnClick += _ =>
        {
            _health = MathF.Min(100f, _health + 25f);
            ShowMessage("+ 25 HP");
        };

        var reloadBtn = bottomBar.AddChild<Button>(b =>
        {
            b.Text = "Reload";
            b.AddClass("btn-reload");
        });
        reloadBtn.OnClick += _ =>
        {
            _ammo = 30;
            ShowMessage("Reloaded");
        };

        var dmgBtn = bottomBar.AddChild<Button>(b =>
        {
            b.Text = "Take Hit";
            b.AddClass("btn-damage");
        });
        dmgBtn.OnClick += _ =>
        {
            float dmg = 15f * _dmgMultiplier;
            if (!_godMode)
                _health = MathF.Max(0f, _health - dmg);
            _ammo = Math.Max(0, _ammo - 3);
            _score += 10;
            ShowMessage(_godMode ? "Blocked! +10 Score" : $"-{(int)dmg} HP  +10 Score");
        };
    }

    public override void Tick(float dt)
    {
        _healthLabel.Text = $"{(int)_health}";
        _scoreLabel.Text = $"{_score}";
        _ammoLabel.Text = $"{_ammo}";

        // Animate health bar
        _healthBar.Value = _health / 100f;
        _healthBar.FillColor = _health > 50
            ? new Raylib_cs.Color(74, 222, 128, 255)   // green
            : _health > 25
                ? new Raylib_cs.Color(250, 204, 21, 255) // yellow
                : new Raylib_cs.Color(248, 113, 113, 255); // red

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
