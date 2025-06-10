using ChessEngine;

namespace ChessApp.Pieces;

public class Pawn : Piece
{
    public Pawn(int position, ChessBoard.PieceColor color, EventHandler onClick) : base(position, color, onClick)
    {
        Source = color == ChessBoard.PieceColor.White ? "wpawn.png" : "bpawn.png";
        PieceType = 6;
        PieceTypeBoard = ChessBoard.PieceType.Pawn;
    }

    public override ulong GetLegalMoves(ref ChessBoard board)
    {
        if (Color == ChessBoard.PieceColor.White)
        {
            return board.GenerateWhitePawnMoves(Position);
        }
        
        return board.GenerateBlackPawnMoves(Position);
    }

    public override bool Move(ulong to, ref ChessBoard board)
    {
        if (board.MakeMove(Position, to, ChessBoard.PieceType.Pawn, Color))
        {
            Position = to;
            return true;
        }

        return false;
    }
}