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

    private const int searchDepth = 5;
    private ChessBoard.PieceColor _color;
    private int _positionsQuantity = 0;
    private int _computedBranches = 0;
    
    public ChessBot(ChessBoard.PieceColor color)
    {
        _color = color;
    }

    public Move Play(ref ChessBoard board)
    { 
        ChessBoard newBoard = new ChessBoard(board);
        _positionsQuantity = 0;
        _computedBranches = 0;
        Move move = MiniMax(1, board, _color);
        board.MakeMove(move.From, move.To, move.Piece, _color);
        Console.WriteLine("Znalezione pozycje: " + _positionsQuantity);
        return move;
    }

    private Move MiniMax(int depth, ChessBoard board, ChessBoard.PieceColor color)
    {
        if (depth >= searchDepth)
        {
            _positionsQuantity++;
            return new Move(){Evaluation = board.EvaluatePosition()};
        }

        ChessBoard newBoard = new ChessBoard(board);
        Move bestMove = new Move() { Evaluation = color == ChessBoard.PieceColor.White ? double.MinValue : double.MaxValue };
        var moves = GenerateMoves(board, color);
        if (moves.Count == 0)
        {
            bestMove.Evaluation = color == ChessBoard.PieceColor.White ? double.MinValue : double.MaxValue;
            return bestMove;
        }
    
        foreach (var move in moves)
        {
            if (!newBoard.MakeMove(move.From, move.To, move.Piece, color))
            {
                continue;
            }
        
            ExecuteSpecialMoves(newBoard, color, move);
            Move evaluation = MiniMax(depth + 1, newBoard, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White);
            if (color == ChessBoard.PieceColor.White)
            {
                if (evaluation.Evaluation > bestMove.Evaluation)
                {
                    bestMove = move;
                    bestMove.Evaluation = evaluation.Evaluation;
                }   
            }
            else
            {
                if (evaluation.Evaluation < bestMove.Evaluation)
                {
                    bestMove = move;
                    bestMove.Evaluation = evaluation.Evaluation;
                }   
            }

            if (depth == 1)
            {
                Console.WriteLine("KOLEJNA POZYCJA NR: " + _computedBranches + " z " + (moves.Count - 1));
                _computedBranches++;
            }
            
            newBoard.Copy(board);
        }
    
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
        UInt64 king = board.GetKing(color);
        UInt64 currentMoves;
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

    private void ExecuteSpecialMoves(ChessBoard board, ChessBoard.PieceColor color, Move move)
    {
        UInt64 whiteLongCastleSquare = 32UL;
        UInt64 whiteShortCastleSquare = 2UL;
        UInt64 blackLongCastleSquare = 32UL << 56;
        UInt64 blackShortCastleSquare = 2UL << 56;
        UInt64 whiteKingBeginingSquare = 8UL;
        UInt64 blackKingBeginingSquare = 8UL << 56;
        UInt64 whitePromotionRow = 255UL << 56;
        UInt64 blackPromotionRow = 255UL;
        UInt64 enPassantSquare = board.GetEnPassantSquare();
        
        // roszady
        if (move.Piece is ChessBoard.PieceType.King)
        {
            if (color == ChessBoard.PieceColor.White)
            {
                if (move.From == whiteKingBeginingSquare && move.To == whiteShortCastleSquare)
                {
                    board.ShortCastle(color);
                }
                else if (move.From == whiteKingBeginingSquare && move.To == whiteLongCastleSquare)
                {
                    board.LongCastle(color);
                }
            }
            else
            {
                if (move.From == blackKingBeginingSquare && move.To == blackShortCastleSquare)
                {
                    board.ShortCastle(color);
                }
                else if (move.From == blackKingBeginingSquare && move.To == blackLongCastleSquare)
                {
                    board.LongCastle(color);
                }
            }
        }
        
        if (move.Piece is ChessBoard.PieceType.Pawn)
        {
            // promocja piona
            if (color == ChessBoard.PieceColor.White)
            {
                if ((move.To & whitePromotionRow) != 0)
                {
                    board.Promote(ChessBoard.PieceType.Queen);
                }
            }
            else
            {
                if ((move.To & blackPromotionRow) != 0)
                {
                    board.Promote(ChessBoard.PieceType.Queen);
                }
            }
            
            if (move.To == enPassantSquare)
            {
                board.EnPassant(enPassantSquare, color);
            }
        }
        
    }
}