using System.Media;
using System.Text;

namespace TarotTraining;

/// <summary>
/// The tarot flash-card trainer. Shows a card (with its printed name covered),
/// offers four keyword choices, and tracks your score. (Replaces the original Form1.)
/// </summary>
public sealed class MainForm : Form
{
    // --- Slate & Gold palette (sits over the grey/white marble background) ---
    private static readonly Color Charcoal      = Color.FromArgb(51, 51, 61);
    private static readonly Color CharcoalHover  = Color.FromArgb(69, 69, 80);
    private static readonly Color CharcoalDown   = Color.FromArgb(40, 40, 48);
    private static readonly Color Gold           = Color.FromArgb(200, 161, 75);
    private static readonly Color GoldHover      = Color.FromArgb(216, 179, 94);
    private static readonly Color GoldDown       = Color.FromArgb(180, 145, 66);
    private static readonly Color GoldBorderDark = Color.FromArgb(168, 133, 56);
    private static readonly Color DarkText       = Color.FromArgb(42, 42, 42);
    private static readonly Color MarbleText     = Color.FromArgb(43, 43, 48);
    private static readonly Color CorrectGreen   = Color.FromArgb(21, 128, 61);

    /// <summary>Gives a button the Slate &amp; Gold look. Primary = the filled gold call-to-action.</summary>
    private static void StyleButton(Button b, bool primary = false)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.UseVisualStyleBackColor = false;
        b.FlatAppearance.BorderSize = 1;
        if (primary)
        {
            b.BackColor = Gold;
            b.ForeColor = DarkText;
            b.FlatAppearance.BorderColor = GoldBorderDark;
            b.FlatAppearance.MouseOverBackColor = GoldHover;
            b.FlatAppearance.MouseDownBackColor = GoldDown;
        }
        else
        {
            b.BackColor = Charcoal;
            b.ForeColor = Gold;
            b.FlatAppearance.BorderColor = Gold;
            b.FlatAppearance.MouseOverBackColor = CharcoalHover;
            b.FlatAppearance.MouseDownBackColor = CharcoalDown;
        }
    }

    private readonly TarotDeck _deck;
    private readonly Random _rng = new();
    private readonly Dictionary<string, Image> _imageCache = new();
    private readonly Image? _marble;

    // The card-image panel and the veil that hides the card's printed name while guessing.
    private readonly Panel _cardPanel = new();
    private readonly MysticVeil _nameCover = new();
    private System.Windows.Forms.Timer? _slideTimer;

    /// <summary>Where the veil rests (flush to the card's lower band); computed so it stays correct under DPI scaling.</summary>
    private int VeilHomeTop => _cardPanel.Height - _nameCover.Height;

    private readonly RadioButton[] _choices = new RadioButton[4];
    private readonly Button _checkButton = new();
    private readonly Button _nextButton = new();
    private readonly Button _detailButton = new();
    private readonly Button _musicButton = new();
    private readonly Button _resetButton = new();
    private readonly Label _scoreLabel = new();

    private readonly RadioButton[] _suitButtons;
    private readonly Dictionary<RadioButton, string> _suitOf = new();

    private SoundPlayer? _music;
    private bool _musicPlaying;

    private Card? _currentCard;
    private int _rounds, _correct, _wrong;

    public MainForm(TarotDeck deck)
    {
        _deck = deck;
        _marble = LoadResourceImage("Marble.png");

        // Scale the whole fixed layout by DPI so it holds together at any display
        // scaling / font size (set before controls are added).
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "Tarot Training";
        ClientSize = new Size(726, 588);
        StartPosition = FormStartPosition.CenterScreen;
        BackgroundImage = _marble;
        BackgroundImageLayout = ImageLayout.Stretch;
        TryLoadIcon();

        BuildCardPanel();
        BuildChoicePanel();
        _suitButtons = BuildSuitGroup();

        StartMusic();
        UpdateScore();
        NewRound();
    }

    // ---- UI construction --------------------------------------------------

    private void BuildCardPanel()
    {
        _cardPanel.Location = new Point(12, 12);
        _cardPanel.Size = new Size(290, 510);
        _cardPanel.BorderStyle = BorderStyle.Fixed3D;
        _cardPanel.BackColor = Color.Transparent;
        _cardPanel.BackgroundImageLayout = ImageLayout.Stretch;

        // The veil sits over the lower name band of the card and slides away on reveal.
        _nameCover.Size = new Size(290, 120);
        _nameCover.Location = new Point(-2, VeilHomeTop);
        _cardPanel.Controls.Add(_nameCover);
        _nameCover.BringToFront();

        Controls.Add(_cardPanel);
    }

    private void BuildChoicePanel()
    {
        var panel = new Panel
        {
            Location = new Point(308, 12),
            Size = new Size(408, 510),
            BorderStyle = BorderStyle.Fixed3D,
            BackColor = Color.Transparent,
        };

        for (int i = 0; i < 4; i++)
        {
            var rb = new RadioButton
            {
                Location = new Point(3, 3 + i * 86),
                Size = new Size(398, 80),
                Font = new Font("Segoe UI", 14.25f),
                ForeColor = MarbleText,
                UseVisualStyleBackColor = true,
            };
            _choices[i] = rb;
            panel.Controls.Add(rb);
        }

        _checkButton.Location = new Point(3, 347);
        _checkButton.Size = new Size(398, 40);
        _checkButton.Font = new Font("Segoe UI", 15.75f, FontStyle.Bold);
        _checkButton.Text = "Check";
        _checkButton.Click += OnCheck;
        StyleButton(_checkButton, primary: true);
        panel.Controls.Add(_checkButton);

        _scoreLabel.Location = new Point(4, 394);
        _scoreLabel.Size = new Size(211, 66);
        _scoreLabel.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
        _scoreLabel.ForeColor = MarbleText;
        panel.Controls.Add(_scoreLabel);

        _detailButton.Location = new Point(3, 463);
        _detailButton.Size = new Size(91, 40);
        _detailButton.Font = new Font("Segoe UI", 8.25f);
        _detailButton.Text = "Card\r\nDetails";
        _detailButton.Click += OnCardDetails;
        StyleButton(_detailButton);
        panel.Controls.Add(_detailButton);

        _musicButton.Location = new Point(100, 463);
        _musicButton.Size = new Size(66, 40);
        _musicButton.Font = new Font("Segoe UI", 8.25f);
        _musicButton.Text = "Stop\r\nMusic";
        _musicButton.Click += OnToggleMusic;
        StyleButton(_musicButton);
        panel.Controls.Add(_musicButton);

        _nextButton.Location = new Point(269, 463);
        _nextButton.Size = new Size(132, 40);
        _nextButton.Font = new Font("Segoe UI", 15.75f, FontStyle.Bold);
        _nextButton.Text = "Next";
        _nextButton.Click += OnNext;
        StyleButton(_nextButton, primary: true);
        panel.Controls.Add(_nextButton);

        Controls.Add(panel);
    }

    private RadioButton[] BuildSuitGroup()
    {
        var group = new GroupBox
        {
            Location = new Point(12, 528),
            Size = new Size(703, 48),
            BackColor = Color.Transparent,
            ForeColor = MarbleText,
            Text = "Select Suit - Right Click to see short descriptions",
        };

        // (text, x, suit-filter, default-checked)
        (string text, int x, string filter, bool def)[] defs =
        {
            ("Major Arcana", 6, "Major Arcana", false),
            ("Pentacles", 133, "Pentacles", false),
            ("Swords", 231, "Swords", false),
            ("Wands", 317, "Wands", false),
            ("Cups", 398, "Cups", false),
            ("All", 467, "All", true),
        };

        var buttons = new List<RadioButton>();
        foreach (var (text, x, filter, def) in defs)
        {
            var rb = new RadioButton
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, 17),
                Font = new Font("Segoe UI", 12f),
                ForeColor = MarbleText,
                Checked = def,
                UseVisualStyleBackColor = true,
            };
            _suitOf[rb] = filter;
            rb.MouseDown += OnSuitRightClick;
            group.Controls.Add(rb);
            buttons.Add(rb);
        }

        _resetButton.Location = new Point(519, 16);
        _resetButton.Size = new Size(178, 26);
        _resetButton.Font = new Font("Segoe UI", 9f);
        _resetButton.Text = "Change Suit/Reset";
        _resetButton.Click += OnReset;
        StyleButton(_resetButton);
        group.Controls.Add(_resetButton);

        Controls.Add(group);
        return buttons.ToArray();
    }

    // ---- Game flow --------------------------------------------------------

    private string SelectedSuit()
    {
        foreach (var rb in _suitButtons)
            if (rb.Checked)
                return _suitOf[rb];
        return "All";
    }

    /// <summary>Deals a fresh card and four keyword choices.</summary>
    private void NewRound()
    {
        _nextButton.Enabled = false;
        _checkButton.Enabled = true;
        CoverName();

        foreach (var rb in _choices)
        {
            rb.Checked = false;
            rb.ForeColor = MarbleText;
        }

        var pool = _deck.CardsFor(SelectedSuit());
        var picks = PickUnique(pool, Math.Min(4, pool.Count));
        _currentCard = picks[0];

        ShowCardImage(_currentCard);

        // Lay the four keyword options out in a random order, keeping the answer in the set.
        var options = picks.OrderBy(_ => _rng.Next()).ToList();
        for (int i = 0; i < _choices.Length; i++)
            _choices[i].Text = i < options.Count ? options[i].Keywords : string.Empty;
    }

    private List<Card> PickUnique(IReadOnlyList<Card> pool, int count)
    {
        var chosen = new List<Card>();
        while (chosen.Count < count)
        {
            var card = pool[_rng.Next(pool.Count)];
            if (!chosen.Contains(card))
                chosen.Add(card);
        }
        return chosen;
    }

    private void OnCheck(object? sender, EventArgs e)
    {
        if (_currentCard is null) return;

        var picked = _choices.FirstOrDefault(c => c.Checked);
        if (picked is null)
        {
            MessageBox.Show("Please pick an option", "Tarot Training",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        bool right = picked.Text == _currentCard.Keywords;
        if (right) _correct++; else _wrong++;
        _rounds++;

        _checkButton.Enabled = false;
        _nextButton.Enabled = true;
        RevealName();

        foreach (var rb in _choices)
            if (rb.Text == _currentCard.Keywords)
                rb.ForeColor = CorrectGreen;

        UpdateScore();
    }

    private void OnNext(object? sender, EventArgs e) => NewRound();

    private void OnReset(object? sender, EventArgs e)
    {
        _rounds = _correct = _wrong = 0;
        UpdateScore();
        NewRound();
    }

    private void OnCardDetails(object? sender, EventArgs e)
    {
        if (_currentCard is null) return;
        new DetailForm(_currentCard.Name, _currentCard.Description, _marble, Icon).Show();
    }

    private void OnSuitRightClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right || sender is not RadioButton rb) return;

        string filter = _suitOf[rb];
        var cards = _deck.CardsFor(filter);
        var sb = new StringBuilder();
        string? lastSuit = null;
        foreach (var c in cards)
        {
            if (filter == "All" && c.Suit != lastSuit)
            {
                if (lastSuit is not null) sb.AppendLine();
                lastSuit = c.Suit;
            }
            sb.AppendLine($"{c.Name} = {c.Keywords}");
        }

        string title = filter == "All" ? "All Cards" : filter;
        new DetailForm(title, sb.ToString(), _marble, Icon).Show();
    }

    private void UpdateScore() =>
        _scoreLabel.Text = $"Played: {_rounds}\r\nCorrect: {_correct}\r\nIncorrect: {_wrong}";

    // ---- Assets -----------------------------------------------------------

    private void ShowCardImage(Card card)
    {
        _cardPanel.BackgroundImage = LoadResourceImage($"{card.Name}.png");
    }

    // ---- Name veil (cover while guessing, slide away on reveal) ------------

    private void CoverName()
    {
        _slideTimer?.Stop();
        _nameCover.Top = VeilHomeTop;
        _nameCover.Show();
        _nameCover.BringToFront();
    }

    private void RevealName() => StartSlide(toTop: _cardPanel.Height + 6);

    /// <summary>Drops the veil downward off the card like a falling curtain, then hides it.</summary>
    private void StartSlide(int toTop)
    {
        _slideTimer?.Stop();
        _slideTimer = new System.Windows.Forms.Timer { Interval = 15 };
        _slideTimer.Tick += (_, _) =>
        {
            int step = Math.Max(8, (toTop - _nameCover.Top) / 4); // ease-out
            if (_nameCover.Top + step >= toTop)
            {
                _slideTimer!.Stop();
                _nameCover.Hide();
                _nameCover.Top = VeilHomeTop; // reset for next round (stays hidden)
            }
            else
            {
                _nameCover.Top += step;
            }
        };
        _slideTimer.Start();
    }

    /// <summary>
    /// The "Mystic Veil" that hides the card name: a deep velvet band with a faint
    /// gold moon-and-stars motif and a small caption. Opaque, so nothing shows through.
    /// </summary>
    private sealed class MysticVeil : Panel
    {
        private static readonly Color Velvet = Color.FromArgb(24, 22, 32);

        public MysticVeil()
        {
            DoubleBuffered = true;
            BackColor = Velvet;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var rect = ClientRectangle;
            using (var bg = new SolidBrush(Velvet))
                g.FillRectangle(bg, rect);

            // Faint gold edge line where the veil meets the card art.
            using (var edge = new Pen(Color.FromArgb(120, 200, 161, 75)))
                g.DrawLine(edge, rect.Left + 2, rect.Top + 1, rect.Right - 2, rect.Top + 1);

            // Moon — star — moon motif.
            using var motifFont = new Font("Segoe UI Symbol", 20f);
            using var motifBrush = new SolidBrush(Color.FromArgb(180, 200, 161, 75));
            const string motif = "☾    ✦    ☽";
            SizeF ms = g.MeasureString(motif, motifFont);
            float motifY = rect.Top + 16;
            g.DrawString(motif, motifFont, motifBrush, (Width - ms.Width) / 2f, motifY);

            // Small caption beneath it.
            using var capFont = new Font("Segoe UI", 9.5f, FontStyle.Italic);
            using var capBrush = new SolidBrush(Color.FromArgb(150, 222, 210, 180));
            const string caption = "revealed when ready";
            SizeF cs = g.MeasureString(caption, capFont);
            g.DrawString(caption, capFont, capBrush, (Width - cs.Width) / 2f, motifY + ms.Height + 2);
        }
    }

    private Image? LoadResourceImage(string fileName)
    {
        if (_imageCache.TryGetValue(fileName, out var cached))
            return cached;

        string path = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        if (!File.Exists(path))
            return null;

        // Read through memory so we never hold a lock on the file on disk.
        var image = Image.FromStream(new MemoryStream(File.ReadAllBytes(path)));
        _imageCache[fileName] = image;
        return image;
    }

    private void TryLoadIcon()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Resources", "TarotTesting.ico");
        if (File.Exists(path))
            Icon = new Icon(path);
    }

    // ---- Background music -------------------------------------------------

    private void StartMusic()
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Resources", "wandering-6394.wav");
            if (!File.Exists(path)) { _musicButton.Text = "Play\r\nMusic"; return; }
            _music = new SoundPlayer(path);
            _music.PlayLooping();
            _musicPlaying = true;
            _musicButton.Text = "Stop\r\nMusic";
        }
        catch
        {
            _musicButton.Text = "Play\r\nMusic";
        }
    }

    private void OnToggleMusic(object? sender, EventArgs e)
    {
        if (_music is null) { StartMusic(); return; }

        if (_musicPlaying)
        {
            _music.Stop();
            _musicPlaying = false;
            _musicButton.Text = "Play\r\nMusic";
        }
        else
        {
            _music.PlayLooping();
            _musicPlaying = true;
            _musicButton.Text = "Stop\r\nMusic";
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _slideTimer?.Stop();
        _slideTimer?.Dispose();
        _music?.Stop();
        _music?.Dispose();
        base.OnFormClosed(e);
    }
}
