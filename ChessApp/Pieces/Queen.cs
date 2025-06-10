using ChessEngine;

namespace ChessApp.Pieces;

public class Queen : Piece
{
    public Queen(int position, ChessBoard.PieceColor color, EventHandler onClick) : base(position, color, onClick)
    {
        Source = color == ChessBoard.PieceColor.White ? "wqueen.png" : "bqueen.png";
        PieceType = 4;
        PieceTypeBoard = ChessBoard.PieceType.Queen;
    }

    public override ulong GetLegalMoves(ref ChessBoard board)
    {
        return board.GenerateQueenMoves(Position, Color);
    }

    public override bool Move(ulong to, ref ChessBoard board)
    {
        if (board.MakeMove(Position, to, ChessBoard.PieceType.Queen, Color))
        {
            Position = to;
            return true;
        }

        return false;
    }
}