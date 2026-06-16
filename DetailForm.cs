namespace TarotTraining;

/// <summary>
/// A simple read-only popup used for both the per-card "Card Details" description
/// and the right-click suit cheat sheets. (Replaces the original Form2.)
/// </summary>
public sealed class DetailForm : Form
{
    public DetailForm(string title, string body, Image? background)
    {
        Text = title;
        ClientSize = new Size(784, 461);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackgroundImage = background;
        BackgroundImageLayout = ImageLayout.Stretch;

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
            Text = body,
        };
        box.Select(0, 0);
        Controls.Add(box);
    }
}
