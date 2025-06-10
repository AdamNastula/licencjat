using ChessEngine;
using CommunityToolkit.Maui.Views;

namespace ChessApp.Utils;

public class EndGamePopup : Popup
{
    public EndGamePopup(ChessBoard.PieceColor winnerColor)
    {
        VerticalStackLayout layout = new VerticalStackLayout();
        layout.Padding = new Thickness(10);
        CanBeDismissedByTappingOutsideOfPopup = false;
        Label winnerLabel = new Label()
        {
            Text = winnerColor == ChessBoard.PieceColor.White ? "Wygral bialy" : "Wygral czarny",
            Margin = new Thickness(10, 10, 10, 10),
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center
        };
        
        Button backToMenuButton = new Button()
        {
            Text = "Powrot do Menu Glownego",
            Margin = new Thickness(10, 10, 10, 10),
        };
        
        backToMenuButton.Clicked += BackToMenuButtonClicked;
        layout.Add(winnerLabel);
        layout.Add(backToMenuButton);
        Content = layout;
    }

    private void BackToMenuButtonClicked(object? sender, EventArgs e)
    {
        Application.Current.MainPage = new MainPage();
    }
}