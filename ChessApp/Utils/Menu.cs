using System.Globalization;
using ChessEngine;

namespace ChessApp.Utils;

public class Menu : VerticalStackLayout
{
    private HistoryElement _movesHistory;
    private Label _evaluationLabel;
    public Menu()
    {
        WidthRequest = 8 * GameBoard.SquareSize;
        HorizontalOptions = this.HorizontalOptions with { Alignment = LayoutAlignment.End };
        Add(new Label()
        {
            Text = "Historia ruchów",
            FontAttributes = FontAttributes.Bold,
            FontSize = 30,
            WidthRequest = this.Width,
            HorizontalTextAlignment = TextAlignment.Center,
        });
        
        _movesHistory = new HistoryElement();
        _movesHistory.HorizontalOptions = _movesHistory.HorizontalOptions with { Alignment = LayoutAlignment.Center};
        Add(_movesHistory);
        _evaluationLabel = new Label
        {
            FontSize = 30,
            Text = "0"
        };
        
        _evaluationLabel.HorizontalOptions = _evaluationLabel.HorizontalOptions with { Alignment = LayoutAlignment.Center };
        Add(_evaluationLabel);
        Button backToMenuButton = new Button();
        backToMenuButton.Text = "Wyjdz z gry";
        backToMenuButton.Clicked += BackToMenuButtonClicked;
        backToMenuButton.WidthRequest = 200;
        Add(backToMenuButton);
    }

    public void UpdateStatus(double evaluation, UInt64 from, UInt64 to, ChessBoard.PieceType piece, ChessBoard.PieceColor color)
    {
        _movesHistory.AddMove(from, to, piece, color);
        _evaluationLabel.Text = evaluation.ToString();
    }
    
    private void BackToMenuButtonClicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new MainPage();
    }
}