namespace TarotTraining;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Match the manifest's PerMonitorV2 awareness so the UI scales with the user's
        // display scaling / font size instead of overlapping at fixed positions.
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        TarotDeck deck;
        try
        {
            deck = TarotDeck.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load the card data (cards.json).\n\n{ex.Message}",
                "Tarot Training",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm(deck));
    }
}
