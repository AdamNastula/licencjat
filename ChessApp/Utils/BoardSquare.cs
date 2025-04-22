using ChessApp.Pieces;
using ChessEngine;

namespace ChessApp.Utils;

public class BoardSquare : Border
{
    public enum SquareColor
    {
        Default,
        Movable
    };
    private const UInt16 SquareSize = 100;
    public readonly UInt64 Index;
    public readonly int GridIndex;
    private Piece? _selectedPiece;
    private ChessBoard _board;
    private readonly TapGestureRecognizer _gestureRecognizer = new TapGestureRecognizer();
    private Color _defaultColor;

    public BoardSquare(int index, EventHandler<TappedEventArgs> onTap)
    {
        WidthRequest = SquareSize;
        HeightRequest = SquareSize;
        _defaultColor = (index % 8 + index / 8) % 2 == 0 ? Colors.LightGrey : Colors.Brown;
        BackgroundColor = _defaultColor;
        _gestureRecognizer.Tapped += onTap;
        GestureRecognizers.Add(_gestureRecognizer);
        Index = 1UL << (63 - index);
        GridIndex = index;
    }

    public void SetColor(SquareColor color)
    {
        switch (color)
        {
            case SquareColor.Default:
                BackgroundColor = _defaultColor;
                break;
            
            case SquareColor.Movable:
                BackgroundColor = Colors.Bisque;
                break;
        }
    }

    public void SetContent(Piece piece)
    {
        Content = piece;
    }
}