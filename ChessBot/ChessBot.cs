using ChessEngine;
namespace ChessBot;

public class ChessBot
{
    public struct Move
    {
        public UInt64 From;
        public UInt64 To;
        public ChessBoard.PieceType Piece;
        public double Evaluation;
    }

    private const int searchDepth = 3;
    private ChessBoard.PieceColor _color;
    
    public ChessBot(ChessBoard.PieceColor color)
    {
        _color = color;
    }

    public Move Play(ref ChessBoard board)
    { 
        Move move = MiniMax(board, 1, _color);
        board.MakeMove(move.From, move.To, move.Piece, _color);
        return move;
    }

    private Move MiniMax(ChessBoard board, int depth, ChessBoard.PieceColor color)
    {
        if (depth >= searchDepth)
        {
            return new Move { Evaluation = board.EvaluatePosition() };
        }
        
        List<Move> moves = GenerateMoves(board, color);
        ChessBoard simulatedBoard = new ChessBoard(board);
        Move bestMove = new Move();
        double bestEvaluation = color == ChessBoard.PieceColor.White ? double.MinValue : double.MaxValue;
        int i = 0;
        foreach (Move move in moves)
        {
            simulatedBoard.MakeMove(move.From, move.To, move.Piece, color);
            Move evaluationMove = MiniMax(simulatedBoard, depth + 1, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White);
            if (color == ChessBoard.PieceColor.White)
            {
                if (evaluationMove.Evaluation > bestEvaluation)
                {
                    bestEvaluation = evaluationMove.Evaluation;
                    bestMove = move;
                    bestMove.Evaluation = evaluationMove.Evaluation;
                }
            }
            else
            {
                if (evaluationMove.Evaluation < bestEvaluation)
                {
                    bestEvaluation = evaluationMove.Evaluation;
                    bestMove = move;
                    bestMove.Evaluation = evaluationMove.Evaluation;
                }
            }
            
            simulatedBoard = new ChessBoard(board);
        }
        
        bestMove.Evaluation = bestEvaluation;
        return bestMove;
    }
    
    private void PrintMoves(List<Move> moves)
    {
        foreach (Move move in moves)
        {
            Utils.PrintBitboard(move.From);
            Console.WriteLine("---");
            Utils.PrintBitboard(move.To);
            Console.WriteLine(move.Piece);
            Console.WriteLine("\n");
            Console.WriteLine("\n");
            Console.WriteLine("\n");
        }
    }
    
    private List<Move> GenerateMoves(ChessBoard board, ChessBoard.PieceColor color)
    {
        UInt64 pawns = board.GetPawns(color);
        UInt64 rooks = board.GetRooks(color);
        UInt64 knights = board.GetKnights(color);
        UInt64 bishops = board.GetBishops(color);
        UInt64 queens = board.GetQueens(color);
        UInt64 king = board.GetKing(color);
        UInt64 currentMoves;
        Func<UInt64, UInt64> pawnGenerator = color == ChessBoard.PieceColor.White ? board.GenerateWhitePawnMoves : board.GenerateBlackPawnMoves;
        List<Move> moves = new List<Move>(300);
        List<ChessBoard.PieceMoves> piecesMoves = board.GetMoves(color);

        foreach (var move in piecesMoves)
        {
            for (UInt64 i = 1Ul; i < move.Moves; i<<=1)
            {
                if (i == 0)
                {
                    break;
                }

                if ((i & (move.Moves)) != 0)
                {
                    moves.Add(new Move(){From = move.From, To = i, Piece = move.PieceType});       
                }
            }
        }
        
        currentMoves = board.GenerateKingMoves(king, color);
        for (UInt64 j = 1; j < currentMoves; j <<= 1)
        {
            if (j == 0)
                break;
            
            if ((j & currentMoves) != 0)
                moves.Add(new Move { From = king, To = j, Piece = ChessBoard.PieceType.King });
        }
        
        return moves;
    }
}