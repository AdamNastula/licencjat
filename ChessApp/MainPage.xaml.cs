using System;
using ChessApp.Utils;
using ChessEngine;
using Microsoft.Maui.Controls;

namespace ChessApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void SingleplayerButtonClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new GamePage(ChessBoard.PieceColor.White, 0);
    }

    private void BotButtonClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new GamePage(ChessBoard.PieceColor.White, 1);
    }
    
    private void MultiplayerButtonClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new GamePage(ChessBoard.PieceColor.White, 2);
    }
}