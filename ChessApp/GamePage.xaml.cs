using ChessApp.Utils;
using ChessEngine;


namespace ChessApp;

public partial class GamePage : ContentPage
{

    private ChessBoard.PieceColor _playerColor;
    private VerticalStackLayout _board = null!;
    private Menu _menu;
    public GamePage(ChessBoard.PieceColor playerColor, int mode)
    {
        InitializeComponent();
        _playerColor = playerColor;
        _menu = new Menu();
        if (mode == 0)
        {
            _board = new GameBoard(playerColor, _menu);
        }
        else if (mode == 1)
        {
            _board = new BotGameBoard(playerColor, _menu);
        }
        else if  (mode == 2)
        {
            _board = new MultiplayerGameBoard(_menu);
        }
        
        HorizontalStackLayout pageContent = new HorizontalStackLayout();
        pageContent.VerticalOptions = pageContent.VerticalOptions with { Alignment = LayoutAlignment.Center };
        pageContent.HorizontalOptions = pageContent.HorizontalOptions with { Alignment = LayoutAlignment.Center };
        pageContent.Padding = new Thickness(20, 20, 20, 20);
        pageContent.Add(_board);
        pageContent.Add(_menu); 
        Content = pageContent;
    }

}