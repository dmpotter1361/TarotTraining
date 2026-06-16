using System.Media;
using System.Text;

namespace TarotTraining;

/// <summary>
/// The tarot flash-card trainer. Shows a card (with its printed name covered),
/// offers four keyword choices, and tracks your score. (Replaces the original Form1.)
/// </summary>
public sealed class MainForm : Form
{
    private readonly TarotDeck _deck;
    private readonly Random _rng = new();
    private readonly Dictionary<string, Image> _imageCache = new();
    private readonly Image? _marble;

    // The card-image panel and the small cover that hides the card's printed name while guessing.
    private readonly Panel _cardPanel = new();
    private readonly Panel _nameCover = new();

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

        _nameCover.Location = new Point(-2, 426);
        _nameCover.Size = new Size(290, 82);
        _nameCover.BorderStyle = BorderStyle.Fixed3D;
        _nameCover.BackgroundImage = _marble;
        _nameCover.BackgroundImageLayout = ImageLayout.None;
        _nameCover.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _cardPanel.Controls.Add(_nameCover);

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
                UseVisualStyleBackColor = true,
            };
            _choices[i] = rb;
            panel.Controls.Add(rb);
        }

        _checkButton.Location = new Point(3, 347);
        _checkButton.Size = new Size(398, 40);
        _checkButton.Font = new Font("Segoe UI", 15.75f);
        _checkButton.Text = "Check";
        _checkButton.UseVisualStyleBackColor = true;
        _checkButton.Click += OnCheck;
        panel.Controls.Add(_checkButton);

        _scoreLabel.Location = new Point(4, 394);
        _scoreLabel.Size = new Size(211, 66);
        _scoreLabel.Font = new Font("Segoe UI", 12f);
        panel.Controls.Add(_scoreLabel);

        _detailButton.Location = new Point(3, 463);
        _detailButton.Size = new Size(91, 40);
        _detailButton.Font = new Font("Segoe UI", 8.25f);
        _detailButton.Text = "Card\r\nDetails";
        _detailButton.UseVisualStyleBackColor = true;
        _detailButton.Click += OnCardDetails;
        panel.Controls.Add(_detailButton);

        _musicButton.Location = new Point(100, 463);
        _musicButton.Size = new Size(66, 40);
        _musicButton.Font = new Font("Segoe UI", 8.25f);
        _musicButton.Text = "Stop\r\nMusic";
        _musicButton.UseVisualStyleBackColor = true;
        _musicButton.Click += OnToggleMusic;
        panel.Controls.Add(_musicButton);

        _nextButton.Location = new Point(269, 463);
        _nextButton.Size = new Size(132, 40);
        _nextButton.Font = new Font("Segoe UI", 15.75f);
        _nextButton.Text = "Next";
        _nextButton.UseVisualStyleBackColor = true;
        _nextButton.Click += OnNext;
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
                Checked = def,
                UseVisualStyleBackColor = true,
            };
            _suitOf[rb] = filter;
            rb.MouseDown += OnSuitRightClick;
            group.Controls.Add(rb);
            buttons.Add(rb);
        }

        _resetButton.Location = new Point(519, 18);
        _resetButton.Size = new Size(178, 23);
        _resetButton.Text = "Change Suit/Reset";
        _resetButton.UseVisualStyleBackColor = true;
        _resetButton.Click += OnReset;
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
        _nameCover.Show();

        foreach (var rb in _choices)
        {
            rb.Checked = false;
            rb.ForeColor = Color.Black;
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
        _nameCover.Hide();

        foreach (var rb in _choices)
            if (rb.Text == _currentCard.Keywords)
                rb.ForeColor = Color.Green;

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
        new DetailForm(_currentCard.Name, _currentCard.Description, _marble).Show();
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
        new DetailForm(title, sb.ToString(), _marble).Show();
    }

    private void UpdateScore() =>
        _scoreLabel.Text = $"Played: {_rounds}\r\nCorrect: {_correct}\r\nIncorrect: {_wrong}";

    // ---- Assets -----------------------------------------------------------

    private void ShowCardImage(Card card)
    {
        _cardPanel.BackgroundImage = LoadResourceImage($"{card.Name}.png");
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
        _music?.Stop();
        _music?.Dispose();
        base.OnFormClosed(e);
    }
}
