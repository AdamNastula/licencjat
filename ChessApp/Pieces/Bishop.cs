using ChessEngine;

namespace ChessApp.Pieces;

public class Bishop : Piece
{
    public Bishop(int position, ChessBoard.PieceColor color, EventHandler onClick) : base(position, color, onClick)
    {
        Source = color == ChessBoard.PieceColor.White ? "wbishop.png" : "bbishop.png";
        PieceType = 3;
        PieceTypeBoard = ChessBoard.PieceType.Bishop;
    }

    public override ulong GetLegalMoves(ref ChessBoard board)
    {
        return board.GenerateBishopMoves(Position, Color);
    }

    public override bool Move(ulong to, ref ChessBoard board)
    {
        if (board.MakeMove(Position, to, ChessBoard.PieceType.Bishop, Color))
        {
            Position = to;
            return true;
        }

        return false;
    }
}