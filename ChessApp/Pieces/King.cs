using ChessEngine;

namespace ChessApp.Pieces;

public class King : Piece
{
    public King(int position, ChessBoard.PieceColor color, EventHandler onClick) : base(position, color, onClick)
    {
        Source = color == ChessBoard.PieceColor.White ? "wking.png" : "bking.png";
    }

    public override ulong GetLegalMoves(ref ChessBoard board)
    {
        return board.GenerateKingMoves(Position, Color);
    }

    public override bool Move(ulong to, ref ChessBoard board)
    {
        if (board.MakeMove(Position, to, ChessBoard.PieceType.King, Color))
        {
            Position = to;
            return true;
        }

        return false;
    }
}