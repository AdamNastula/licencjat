using ChessEngine;

namespace ChessApp.Utils;

public class HistoryElement : ScrollView
{
    private readonly Label _whitePlayerHistory;
    private readonly Label _blackPlayerHistory;
    private const double MyWidth = 300d;

    public HistoryElement()
    {
        WidthRequest = MyWidth;
        MaximumHeightRequest = 250;
        MinimumHeightRequest = 250;
        var content = new HorizontalStackLayout();
        
        Content = content;
        
        _whitePlayerHistory = new Label
        {
            FontSize = 20,
            HorizontalTextAlignment = TextAlignment.Start,
            WidthRequest = MyWidth / 2
        };
            
        _blackPlayerHistory = new Label
        {
            FontSize = 20,
            HorizontalTextAlignment = TextAlignment.End,
            WidthRequest = MyWidth / 2
        };
        
        content.Add(_whitePlayerHistory);
        content.Add(_blackPlayerHistory);
    }

    public void AddMove(ChessBoard.PieceColor side)
    {
        var sideToAdd = side == ChessBoard.PieceColor.White ? _whitePlayerHistory : _blackPlayerHistory;
        sideToAdd.Text += "move";
        sideToAdd.Text += '\n';
    }
}