namespace TarotTraining;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

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
