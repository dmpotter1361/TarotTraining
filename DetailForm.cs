namespace TarotTraining;

/// <summary>
/// A simple read-only popup used for both the per-card "Card Details" description
/// and the right-click suit cheat sheets. (Replaces the original Form2.)
/// </summary>
public sealed class DetailForm : Form
{
    public DetailForm(string title, string body, Image? background, Icon? icon = null)
    {
        Text = title;
        ClientSize = new Size(784, 461);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackgroundImage = background;
        BackgroundImageLayout = ImageLayout.Stretch;
        if (icon is not null) Icon = icon;

        // A multiline TextBox only breaks lines on CR-LF; the card data uses bare LF,
        // so normalize any mix of line endings to CR-LF or it renders as one run-on block.
        string normalized = body.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

        var box = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = true,
            Font = new Font("Segoe UI", 12f),
            Location = new Point(12, 12),
            Size = new Size(ClientSize.Width - 24, ClientSize.Height - 24),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Text = normalized,
        };
        box.Select(0, 0);
        Controls.Add(box);
    }
}
