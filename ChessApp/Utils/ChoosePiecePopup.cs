using ChessApp.Pieces;
using ChessEngine;
using CommunityToolkit.Maui.Views;

namespace ChessApp.Utils;

public class ChoosePiecePopup : Popup
{
    public ChoosePiecePopup()
    {
        VerticalStackLayout layout = new VerticalStackLayout();
        layout.Padding = new Thickness(10);
        CanBeDismissedByTappingOutsideOfPopup = false;
        Button queenButton = new Button()
        {
            Text = "Hetman",
            Margin = new Thickness(10, 10, 10, 10),
        };
        queenButton.Clicked += QueenButtonClicked;

        Button rookButton = new Button()
        {
            Text = "Wieza",
            Margin = new Thickness(10, 10, 10, 10),
        };
        rookButton.Clicked += RookButtonClicked;
        
        Button knightButton = new Button()
        {
            Text = "Skoczek",
            Margin = new Thickness(10, 10, 10, 10),
        };
        knightButton.Clicked += KnightButtonClicked;
        
        Button bishopButton = new Button()
        {
            Text = "Goniec",
            Margin = new Thickness(10, 10, 10, 10),
        };
        bishopButton.Clicked += BishopButtonClicked;
        layout.Add(queenButton);
        layout.Add(rookButton);
        layout.Add(knightButton);
        layout.Add(bishopButton);
        Content = layout;
    }

    async void QueenButtonClicked(object? sender, EventArgs e)
    {
        await CloseAsync(ChessBoard.PieceType.Queen);
    }
    
    async void RookButtonClicked(object? sender, EventArgs e)
    {
        await CloseAsync(ChessBoard.PieceType.Rook);
    }
    
    async void KnightButtonClicked(object? sender, EventArgs e)
    {
        await CloseAsync(ChessBoard.PieceType.Knight);
    }
    
    async void BishopButtonClicked(object? sender, EventArgs e)
    {
        await CloseAsync(ChessBoard.PieceType.Bishop);
    }
}