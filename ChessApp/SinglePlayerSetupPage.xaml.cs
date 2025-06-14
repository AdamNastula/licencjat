using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessEngine;

namespace ChessApp;

public partial class SinglePlayerSetupPage : ContentPage
{
    public SinglePlayerSetupPage()
    {
        InitializeComponent();
    }
    
    private void BeginButtonClicked(object? sender, EventArgs e)
    {
        Application.Current!.MainPage = new GamePage(ChessBoard.PieceColor.White, 0);
    }
}