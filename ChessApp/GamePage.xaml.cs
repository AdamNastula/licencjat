using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessApp.Utils;
using ChessEngine;

namespace ChessApp;

public partial class GamePage : ContentPage
{

    private ChessBoard.PieceColor _playerColor;
    private GameBoard _board;
    
    public GamePage(ChessBoard.PieceColor playerColor)
    {
        InitializeComponent();
        _playerColor = playerColor;
        _board = new GameBoard(playerColor);
        
        HorizontalStackLayout pageContent = new HorizontalStackLayout();
        pageContent.VerticalOptions = pageContent.VerticalOptions with { Alignment = LayoutAlignment.Center };
        pageContent.HorizontalOptions = pageContent.HorizontalOptions with { Alignment = LayoutAlignment.Center };
        pageContent.Padding = new Thickness(20, 20, 20, 20);
        
        pageContent.Add(_board);
        pageContent.Add(new Menu()); 
        Content = pageContent;
    }

}