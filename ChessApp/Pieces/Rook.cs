using ChessEngine;

namespace ChessApp.Pieces;

public class Rook : Piece
{
    public Rook(int position, ChessBoard.PieceColor color,  EventHandler onClick) : base(position, color, onClick)
    {
        Source = color == ChessBoard.PieceColor.White ? "wrook.png" : "brook.png";
        PieceType = 1;
        PieceTypeBoard = ChessBoard.PieceType.Rook;
    }
    
    public override ulong GetLegalMoves(ref ChessBoard board)
    {
        return board.GenerateRookMoves(Position, Color);
    }

    public override bool Move(ulong to, ref ChessBoard board)
    {
        if (board.MakeMove(Position, to, ChessBoard.PieceType.Rook, Color))
        {
            Position = to;
            return true;
        }

        return false;
    }
}