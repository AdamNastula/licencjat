using System;
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

    public void AddMove(UInt64 from, UInt64 to, ChessBoard.PieceType piece, ChessBoard.PieceColor color)
    {
        var sideToAdd = color == ChessBoard.PieceColor.White ? _whitePlayerHistory : _blackPlayerHistory;
        sideToAdd.Text += DecodeMove(from);
        sideToAdd.Text += " ";
        sideToAdd.Text += DecodeMove(to);
        sideToAdd.Text += '\n';
    }

    private string DecodeMove(UInt64 square)
    {
        UInt64 squareIndex = UInt64.Log2(square);
        char col = (char)('H' - squareIndex % 8);
        char row = (char)('1' + squareIndex / 8);
        return col.ToString() + row.ToString();
    }
}