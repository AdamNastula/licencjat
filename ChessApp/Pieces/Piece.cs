using ChessEngine;

namespace ChessApp.Pieces;

public abstract class Piece : ImageButton
{
    public UInt64 Position;
    public int BoardPosition;
    protected ChessBoard.PieceColor Color;
    
    public Piece(int position, ChessBoard.PieceColor color, EventHandler onClick)
    {
        BoardPosition = position;
        Position = 1UL << (63 - BoardPosition);
        Color = color;
        Clicked += onClick;
    }

    public abstract UInt64 GetLegalMoves(ref ChessBoard board);
    public abstract bool Move(UInt64 to, ref ChessBoard board);

    public ChessBoard.PieceColor GetPieceColor()
    {
        return Color;
    }
}