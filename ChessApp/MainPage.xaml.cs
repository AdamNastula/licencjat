namespace ChessApp;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void SingleplayerButtonClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new SinglePlayerSetupPage();
    }

    private void MultiplayerButtonClicked(object sender, EventArgs e)
    {
        
    }
}