using ChessEngine;

namespace ChessApp.Pieces;

public class Knight : Piece
{
    public Knight(int position, ChessBoard.PieceColor color,  EventHandler onClick) : base(position, color, onClick)
    {
        Source = color == ChessBoard.PieceColor.White ? "wknight.png" : "bknight.png";
    }

    public override ulong GetLegalMoves(ref ChessBoard board)
    {
        return board.GenerateKnightMoves(Position, Color);
    }

    public override bool Move(ulong to, ref ChessBoard board)
    {
        if (board.MakeMove(Position, to, ChessBoard.PieceType.Knight, Color))
        {
            Position = to;
            return true;
        }

        return false;
    }
}