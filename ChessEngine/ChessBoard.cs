namespace ChessEngine;

public class ChessBoard
{
    public enum PieceColor { White, Black }
    
    public struct PieceMoves
    {
        public UInt64 From;
        public UInt64 Moves;
        public PieceType PieceType;
    }
     public enum PieceType
     {
         Pawn,
         Rook,
         Knight,
         Bishop,
         Queen,
         King
     }
     
     private readonly UInt64[] _piecesBitboards = new UInt64[12];
     private const uint whitePawns = 0;
     private const uint whiteRooks = 1;
     private const uint whiteKnights = 2;
     private const uint whiteBishops = 3;
     private const uint whiteQueens = 4;
     private const uint whiteKing = 5;
     private const uint blackPawns = 6;
     private const uint blackRooks = 7;
     private const uint blackKnights = 8;
     private const uint blackBishops = 9;
     private const uint blackQueens = 10;
     private const uint blackKing = 11;

     private readonly UInt64 _whitePawnsStartPositions = 65280;
     private readonly UInt64 _blackPawnsStartPostions = 65280UL << 40;
     private readonly UInt64[] _columns = new UInt64[8];
     private readonly UInt64[] _rows = new UInt64[8];
     private readonly UInt64 _notAbFile;
     private readonly UInt64 _notGhFile;
     
     private PieceColor _sideToMove = PieceColor.White;
     private UInt64 _freeSquares = 0UL;
     private UInt64 _occupiedSquares = 0UL;
     private UInt64 _blackOccupiedSquares = 0UL;
     private UInt64 _whiteOccupiedSquares = 0UL;
     private UInt64 _whiteAttackedSquares = 0UL;
     private UInt64 _blackAttackedSquares = 0UL;
     private UInt64 _whiteCheckingLine = 0Ul;
     private UInt64 _blackCheckingLine = 0Ul;

     private bool _whiteKingChecked = false;
     private bool _blackKingChecked = false;
     
     private bool _whiteKingMoved = false;
     private bool _blackKingMoved = false;
     private bool _whiteHRookMoved = false;
     private bool _whiteARookMoved = false;
     private bool _blackHRookMoved = false;
     private bool _blackARookMoved = false;
     
     public readonly int BlackLongCastle = 1;
     public readonly int BlackShortCastle = 2;
     public readonly int WhiteLongCastle = 4;
     public readonly int WhiteShortCastle = 8;
     private int _castlingRights;
 
     private readonly UInt64 _whiteShortCastleSquares = 6UL;
     private readonly UInt64 _blackShortCastleSquares = 432345564227567616UL;
     private readonly UInt64 _whiteLongCastleSquares = 112UL;
     private readonly UInt64 _blackLongCastleSquares = 8070450532247928832;

     private bool _whitePromotion = false;
     private bool _blackPromotion = false;
     private bool _whiteDoubleCheck = false;
     private bool _blackDoubleCheck = false;
     private bool _gameEnded = false;
     private int _timeSinceEnPassant = 0;
     private UInt64 _promotionTargetSquare = 0UL;
     private UInt64 _attackingLine = 0UL;
     private UInt64 _blackEnPassantSquare = 0UL;
     private UInt64 _whiteEnPassantSquare = 0UL;
     private UInt64 _whitePinningSquares = 0UL;
     private UInt64 _blackPinningSquares = 0UL;
     private UInt64 _whiteVisibleSquares = 0UL;
     private UInt64 _blackVisibleSquares = 0UL;
     private PieceColor? _winner = null;
     private readonly List<PieceMoves> _blackMoves = new List<PieceMoves>(40);
     private readonly List<PieceMoves> _whiteMoves = new List<PieceMoves>(40);
     // premia za kazde widoczne pole dla gonca i wiezy
     // skoczek premia
     
     public ChessBoard()
     {
         _piecesBitboards[whitePawns] = 255UL << 8;   // inicjalizacja bialych pionow
         _piecesBitboards[whiteRooks] = 129UL;        // inicjalizacja bialych wiez
         _piecesBitboards[whiteKnights] = 66UL;       // inicjalizacja bialych skoczkow
         _piecesBitboards[whiteBishops] = 36UL;       // inicjalizacja bialych goncow
         _piecesBitboards[whiteQueens] = 16UL;        // inicjalizacja bialych hetmanow
         _piecesBitboards[whiteKing] = 8UL;           // inicjalizacja bialego krola
         
         _piecesBitboards[blackPawns] = 255UL << 48;  // inicjalizacja czarnych pionow
         _piecesBitboards[blackRooks] = 129UL << 56;  // inicjalizacja czarnych wiez
         _piecesBitboards[blackKnights] = 66UL << 56; // inicjalizacja czarnych skoczkow
         _piecesBitboards[blackBishops] = 36UL << 56; // inicjalizacja czarnych goncow
         _piecesBitboards[blackQueens] = 16UL << 56;  // inicjalizacja czarnych hetmanow
         _piecesBitboards[blackKing] = 8UL << 56;     // inicjalizacja czarnego krola
 
         // ustawianie masek dla kolumn
         UInt64 currentColumn = 9259542123273814144UL; // maska skrajnie lewej kolumny
         for (int i = 0; i < 8; i++)
         {
             _columns[i] = currentColumn;
             currentColumn >>= 1;
         }
 
         // ustawianie masek dla rzedow
         UInt64 currentRow = 255; // maska dla dolnego rzedu
         for (int i = 0; i < 8; i++)
         {
             _rows[i] = currentRow;
             currentRow <<= 8;
         }
         
         _notAbFile = ~(_columns[0] | _columns[1]);
         _notGhFile = ~(_columns[6] | _columns[7]);
         
         // inicjalizacja zajetych pol
         foreach (var piecesBitboard in _piecesBitboards)
         { 
             _occupiedSquares |= piecesBitboard;
         }
 
         for (int i = 0; i <= 5; i++)
         {
             _whiteOccupiedSquares |= _piecesBitboards[i];
             _blackOccupiedSquares |= _piecesBitboards[i + 6];
         }
         
         // inicjalizacja wolnych pol
         _freeSquares = ~_occupiedSquares;
         
         // inicjalizacja praw do roszady
         _castlingRights = BlackLongCastle | WhiteLongCastle | WhiteShortCastle | BlackShortCastle;
         
         GenerateAttackedAndVisibleSquares();
     }

     public ChessBoard(ChessBoard engine)
     {
        _piecesBitboards = new UInt64[12];
        _piecesBitboards[whitePawns] = engine._piecesBitboards[whitePawns];
        _piecesBitboards[whiteRooks] = engine._piecesBitboards[whiteRooks];
        _piecesBitboards[whiteKnights] = engine._piecesBitboards[whiteKnights];
        _piecesBitboards[whiteBishops] = engine._piecesBitboards[whiteBishops];
        _piecesBitboards[whiteQueens] = engine._piecesBitboards[whiteQueens];
        _piecesBitboards[whiteKing] = engine._piecesBitboards[whiteKing];
        _piecesBitboards[blackPawns] = engine._piecesBitboards[blackPawns];
        _piecesBitboards[blackRooks] = engine._piecesBitboards[blackRooks];
        _piecesBitboards[blackKnights] = engine._piecesBitboards[blackKnights];
        _piecesBitboards[blackBishops] = engine._piecesBitboards[blackBishops];
        _piecesBitboards[blackQueens] = engine._piecesBitboards[blackQueens];
        _piecesBitboards[blackKing] = engine._piecesBitboards[blackKing];
        _sideToMove = engine._sideToMove;
        _freeSquares = engine._freeSquares;
        _occupiedSquares = engine._occupiedSquares;
        _blackOccupiedSquares = engine._blackOccupiedSquares; 
        _whiteOccupiedSquares = engine._whiteOccupiedSquares;
        _whiteAttackedSquares = engine._whiteAttackedSquares;
        _blackAttackedSquares = engine._blackAttackedSquares;
        _whiteCheckingLine = engine._whiteCheckingLine;
        _blackCheckingLine = engine._blackCheckingLine;

        _whiteKingChecked = engine._whiteKingChecked;
        _blackKingChecked = engine._blackKingChecked;
     
        _whiteKingMoved = engine._whiteKingMoved;
        _blackKingMoved = engine._blackKingMoved;
        _whiteHRookMoved = engine._whiteHRookMoved;
        _whiteARookMoved = engine._whiteARookMoved;
        _blackHRookMoved = engine._blackHRookMoved;
        _blackARookMoved = engine._blackARookMoved;
        _castlingRights = engine._castlingRights;

        _attackingLine = engine._attackingLine;
        _blackEnPassantSquare = engine._blackEnPassantSquare;
        _whiteEnPassantSquare = engine._whiteEnPassantSquare;
        _timeSinceEnPassant = engine._timeSinceEnPassant;
        _whitePinningSquares = engine._whitePinningSquares;
        _blackPinningSquares = engine._blackPinningSquares;
        _whiteVisibleSquares = engine._whiteVisibleSquares;
        _blackVisibleSquares = engine._blackVisibleSquares;
        _gameEnded = engine._gameEnded;
        _winner = engine._winner;
        _whitePromotion = engine._whitePromotion;
        _blackPromotion = engine._blackPromotion;
        _blackMoves = engine._blackMoves;
        _whiteMoves = engine._whiteMoves;
     }

     public List<PieceMoves> GetMoves(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _whiteMoves;
         }
         
         return _blackMoves;
     }
     
     public UInt64 GetPawns(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _piecesBitboards[whitePawns];
         }
         
         return _piecesBitboards[blackPawns];
     }
     
     public UInt64 GetRooks(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _piecesBitboards[whiteRooks];
         }
         
         return _piecesBitboards[blackRooks];
     }
     
     public UInt64 GetKnights(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _piecesBitboards[whiteKnights];
         }
         
         return _piecesBitboards[blackKnights];
     }
     
     public UInt64 GetBishops(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _piecesBitboards[whiteBishops];
         }
         
         return _piecesBitboards[blackBishops];
     }

     public UInt64 GetQueens(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _piecesBitboards[whiteQueens];
         }
         
         return _piecesBitboards[blackQueens];
     }
     
     public UInt64 GetKing(PieceColor color)
     {
         if (color == PieceColor.White)
         {
             return _piecesBitboards[whiteKing];
         }
         
         return _piecesBitboards[blackKing];
     }
     
     public PieceColor? GetWinner()
     {
         return _winner;
     }

     public bool GameEnded()
     {
         return _gameEnded;
     }
     public PieceColor GetSideToMove()
     {
         return _sideToMove;
     }

     public int GetCastlingRights()
     {
         return _castlingRights;
     }

     public bool IsWhiteKingChecked()
     {
         return _whiteKingChecked;
     }

     public bool IsBlackKingChecked()
     {
         return _blackKingChecked;
     }

     public void PrintBoard()
     {
         Utils.PrintBitboard(_occupiedSquares);
     }
     
     public bool MakeMove(UInt64 from, UInt64 to, PieceType piece, PieceColor color)
     {
         int ownPiecesIndex = color == PieceColor.White ? 0 : 6;
         int enemyPiecesIndex = color == PieceColor.White ? 6 : 0;
         Func<UInt64> moveGenerator = () => 0Ul;
         ref UInt64 pieceBitboard = ref _piecesBitboards[0];
         _whiteKingChecked = false;
         _blackKingChecked = false;
         _whitePromotion = false;
         _blackPromotion = false;
         _promotionTargetSquare = 0UL;
         if (_gameEnded)
         {
             return false;
         }
         
         if (_sideToMove != color)
         {
             return false;
         }
 
         // wybiernie odpowiedniego bitboarda dla bierki i odpowiedniego generatora ruchow
         switch (piece)
         {
             case PieceType.Pawn:
                 pieceBitboard = ref _piecesBitboards[ownPiecesIndex];
                 if (color == PieceColor.White)
                 {
                     moveGenerator = () => GenerateWhitePawnMoves(from);
                 }
                 else
                 {
                     moveGenerator = () => GenerateBlackPawnMoves(from);
                 }
                 
                 break;
 
             case PieceType.Rook:
                 pieceBitboard = ref _piecesBitboards[ownPiecesIndex + 1];
                 moveGenerator = () => GenerateRookMoves(from, color);
                 break;
 
             case PieceType.Knight:
                 pieceBitboard = ref _piecesBitboards[ownPiecesIndex + 2];
                 moveGenerator = () => GenerateKnightMoves(from, color);
                 break;
 
             case PieceType.Bishop:
                 pieceBitboard = ref _piecesBitboards[ownPiecesIndex + 3];
                 moveGenerator = () => GenerateBishopMoves(from, color);
                 break;
 
             case PieceType.Queen:
                 pieceBitboard = ref _piecesBitboards[ownPiecesIndex + 4];
                 moveGenerator = () => GenerateQueenMoves(from, color);
                 break;
 
             case PieceType.King:
                 pieceBitboard = ref _piecesBitboards[ownPiecesIndex + 5];
                 moveGenerator = () => GenerateKingMoves(from, color);
                 break;
         }
         
         // sprawdzanie czy bierka jest na danym polu i czy moze sie ruszyc na wskazane pole
         if (((pieceBitboard & from) != from) || ((moveGenerator() & to) != to))
         {
             return false;
         }
         
         // ruszenie swoja bierka
         pieceBitboard &= ~from;
         pieceBitboard |= to;
 
         // zbicie figury przeciwnika
         for (int i = 0; i < 6; i++)
         {
             if ((_piecesBitboards[enemyPiecesIndex + i] & to) == to)
             {
                 _piecesBitboards[enemyPiecesIndex + i] &= ~to;
                 break;
             }
         }

         // obsluga promocji pionow 
         if (piece is PieceType.Pawn)
         {
             if (color == PieceColor.White)
             {
                 if ((to & _rows[7]) != 0)
                 {
                     _whitePromotion = true;
                     _promotionTargetSquare = to;
                 }
             }
             else
             {
                 if ((to & _rows[0]) != 0)
                 {
                     _blackPromotion = true;
                     _promotionTargetSquare = to;
                 }
             }
         }
 
         // aktualizacja maski wolnych i zajetych pol
         _whiteOccupiedSquares = 0UL;
         _blackOccupiedSquares = 0UL;
         for (int i = 0; i < 6; i++)
         {
             _whiteOccupiedSquares |= _piecesBitboards[i];
             _blackOccupiedSquares |= _piecesBitboards[i + 6];
         }
 
         _occupiedSquares = _whiteOccupiedSquares | _blackOccupiedSquares;
         _freeSquares = ~_occupiedSquares;
         
         // do roszady - jesli krol sie ruszyl to nie mozna roszowac
         if (piece == PieceType.King)
         {
             if (color == PieceColor.White)
             {
                 _whiteKingMoved = true;
             }
             else
             {
                 _blackKingMoved = true;
             }
         }
 
         // do roszady - jesli wieza sie ruszyla to nie mozna roszowac
         if (piece == PieceType.Rook)
         {
             if ((from & _columns[0]) == from)
             {
                 if (color == PieceColor.White)
                 {
                     _whiteARookMoved = true;
                 }
                 else
                 {
                     _blackARookMoved = true;
                 }
             }
             else if ((from & _columns[7]) == from)
             {
                 if (color == PieceColor.White)
                 {
                     _whiteHRookMoved = true;
                 }
                 else
                 {
                     _blackHRookMoved = true;
                 }
             }
         }
         
         // do bicia w przelocie
         if (piece == PieceType.Pawn)
         {
             if (color == PieceColor.White)
             {
                 if ((from & _rows[1]) == from && (to & _rows[3]) == to)
                 {
                     _whiteEnPassantSquare = (to >> 8);
                 }
             }
             else
             {
                 if ((from & _rows[6]) == from && (to & _rows[4]) == to)
                 {
                     _blackEnPassantSquare = (to << 8);
                 }
             }

             _timeSinceEnPassant = 0;
         }

         if (_timeSinceEnPassant > 0)
         {
             _whiteEnPassantSquare = 0Ul;
             _blackEnPassantSquare = 0Ul;
         }

         _timeSinceEnPassant++;
         
         // zwiazane figury
         GeneratePins();
         
         // szachowanie
         GenerateAttackedAndVisibleSquares();
         if ((_piecesBitboards[whiteKing] & _blackAttackedSquares) != 0)
         {
             _whiteKingChecked = true;
             GenerateAttackedAndVisibleSquares();
             if (_whiteAttackedSquares == 0UL)
             {
                 _gameEnded = true;
                 _winner = PieceColor.Black;
             }
         }

         GenerateAttackedAndVisibleSquares();
         if ((_piecesBitboards[blackKing] & _whiteAttackedSquares) != 0)
         {
             _blackKingChecked = true;
             GenerateAttackedAndVisibleSquares();
             if (_blackAttackedSquares == 0UL)
             {
                 _gameEnded = true;
                 _winner = PieceColor.White;
             }
         }
         
         _sideToMove = _sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
         return true;
     }
     
     public UInt64 GenerateWhitePawnMoves(UInt64 pawnPosition)
     {
         UInt64 moves = 0Ul;
         UInt64 currentPosition = pawnPosition & _piecesBitboards[whitePawns];
         UInt64 currentMove;
         bool pinned = ((currentPosition & _blackPinningSquares) != 0);
         if (currentPosition == 0 || _whiteDoubleCheck)
         {
             return moves;
         }
         
         // jeden ruch do przodu
         currentMove = (currentPosition << 8) & _freeSquares;
         moves |= currentMove;
         if (_whiteKingChecked)
         {
             if ((currentMove & _blackCheckingLine) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _blackPinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         // dwa ruchy do przodu jesli to mozliwe
         currentMove = ((currentPosition & _whitePawnsStartPositions) << 16) & _freeSquares;
         if (moves != 0)
         {
             moves |= currentMove;
         }
         
         if (_whiteKingChecked)
         {
             if ((currentMove & _blackCheckingLine) == 0)
             {
                 moves &= ~currentMove;
             }
         }

         if (pinned)
         {
             if ((currentMove & _blackPinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         // bicie w lewo
         currentMove = ((currentPosition & ~_columns[0]) << 9) & (_blackOccupiedSquares | _blackEnPassantSquare);
         _whiteVisibleSquares |= ((currentPosition & ~_columns[0]) << 9);
         moves |= currentMove;
         if ((currentMove & _piecesBitboards[blackKing]) != 0)
         {
             _attackingLine = (currentMove | currentPosition);
         }
         
         if (_whiteKingChecked)
         {
             if ((currentMove & _blackCheckingLine) == 0)
             {
                 moves &=  ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _blackPinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         // bicie w prawo
         currentMove = ((currentPosition & ~_columns[7]) << 7) & (_blackOccupiedSquares | _blackEnPassantSquare); 
         _whiteVisibleSquares |= ((currentPosition & ~_columns[7]) << 7);
         moves |= currentMove;
         if ((currentMove & _piecesBitboards[blackKing]) != 0)
         {
             _attackingLine = (currentMove | currentPosition);
         }
         
         if (_whiteKingChecked)
         {
             if ((currentMove & _blackCheckingLine) == 0)
             {
                 moves &=  ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _blackPinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         return moves;
     }
     
     public UInt64 GenerateBlackPawnMoves(UInt64 pawnPosition)
     {
         UInt64 moves = 0UL;
         UInt64 currentPosition = pawnPosition & _piecesBitboards[blackPawns];
         UInt64 currentMove = (currentPosition >> 8) & _freeSquares;
         bool pinned = ((currentPosition & _whitePinningSquares) != 0);
         if (currentPosition == 0 || _blackDoubleCheck)
         {
             return moves;
         }
         
         // jeden ruch do przodu
         moves |= currentMove;
         if (_blackKingChecked)
         {
             if ((currentMove & _whiteCheckingLine) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _whitePinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         // dwa ruchy do przodu jesli to mozliwe
         currentMove = ((currentPosition & _blackPawnsStartPostions) >> 16) & _freeSquares;
         if (moves != 0)
         {
             moves |= currentMove;
         }
         
         if (_blackKingChecked)
         {
             if ((currentMove & _whiteCheckingLine) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _whitePinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         // bicie w prawo
         currentMove = ((currentPosition & ~_columns[7]) >> 9) & (_whiteOccupiedSquares | _whiteEnPassantSquare);
         _blackVisibleSquares |= ((currentPosition & ~_columns[7]) >> 9);
         moves |= currentMove;
         if ((currentMove & _piecesBitboards[whiteKing]) != 0)
         {
             _attackingLine = (currentMove | currentPosition);
         }
         
         if (_blackKingChecked)
         {
             if ((currentMove & _whiteCheckingLine) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _whitePinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         // bicie w lewo
         currentMove = ((currentPosition & ~_columns[0]) >> 7) & (_whiteOccupiedSquares | _whiteEnPassantSquare);
         _blackVisibleSquares |= ((currentPosition & ~_columns[0]) >> 7);
         moves |= currentMove;
         if ((currentMove & _piecesBitboards[whiteKing]) != 0)
         {
             _attackingLine = (currentMove | currentPosition);
         }
         
         if (_blackKingChecked)
         {
             if ((currentMove & _whiteCheckingLine) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         if (pinned)
         {
             if ((currentMove & _whitePinningSquares) == 0)
             {
                 moves &= ~currentMove;
             }
         }
         
         return moves;
     }
 
     public UInt64 GenerateKnightMoves(UInt64 knightPosition, PieceColor knightColor)
     {
         UInt64 moves = 0UL;
         UInt64 ownOccupiedSquares = knightColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 currentPosition = knightPosition & (knightColor == PieceColor.White ? _piecesBitboards[whiteKnights] : _piecesBitboards[blackKnights]);
         UInt64 enemyKing = knightColor == PieceColor.White ? _piecesBitboards[blackKing] : _piecesBitboards[whiteKing];
         UInt64 freeSquares = ~ownOccupiedSquares;
         UInt64 enemyCheckingLine = knightColor == PieceColor.White ? _blackCheckingLine : _whiteCheckingLine;
         UInt64 enemyPinningSquares = knightColor == PieceColor.White ? _blackPinningSquares : _whitePinningSquares;
         bool kingChecked = knightColor == PieceColor.White ? _whiteKingChecked : _blackKingChecked;
         UInt64 ownVisibleSquares = 0Ul;
         bool doubleCheck = knightColor == PieceColor.White ? _whiteDoubleCheck : _blackDoubleCheck;
         if (currentPosition == 0 || doubleCheck)
         {
             return moves;
         }
         
         if ((currentPosition & enemyPinningSquares) != 0)
         {
             return moves;
         }

         UInt64 currentMove = (((currentPosition & _notAbFile) >> 6) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }

         currentMove = (((currentPosition & _notGhFile) << 6) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }

         currentMove = (((currentPosition & _notGhFile) >> 10) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }
         
         currentMove = (((currentPosition & _notAbFile) << 10) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }
         
         currentMove = (((currentPosition & (~_columns[7])) << 15) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }
         
         currentMove = (((currentPosition & (~_columns[0])) >> 15) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }

         currentMove = (((currentPosition & (~_columns[0])) << 17) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }
         
         currentMove = (((currentPosition & (~_columns[7])) >> 17) & freeSquares);
         ownVisibleSquares |= currentMove;
         if (kingChecked)
         {
             if ((currentMove & enemyCheckingLine) != 0)
             {
                 moves |= currentMove;
             }
         }
         else
         {
             moves |= currentMove;   
         }
         if ((currentMove & enemyKing) != 0)
         {
             _attackingLine = currentPosition;
         }

         if (knightColor == PieceColor.White)
         {
             _whiteVisibleSquares |= ownVisibleSquares;
         }
         else
         {
             _blackVisibleSquares |= ownVisibleSquares;
         }
         
         return moves;
     }
     public UInt64 GenerateRookMoves(UInt64 rookPosition, PieceColor rookColor)
     {
         if ((rookPosition & (rookColor == PieceColor.White ? _piecesBitboards[whiteRooks] : _piecesBitboards[blackRooks])) == 0)
         {
             return 0UL;
         }
         
         return GenerateVerticalAndHorizontalMoves(rookPosition, rookColor);
     }
 
     public UInt64 GenerateBishopMoves(UInt64 bishopPosition, PieceColor bishopColor)
     {
         if ((bishopPosition & (bishopColor == PieceColor.White ? _piecesBitboards[whiteBishops] : _piecesBitboards[blackBishops])) == 0)
         {
             return 0UL;
         }
         
         return GenerateDiagonalMoves(bishopPosition, bishopColor);
     }
 
     public UInt64 GenerateQueenMoves(UInt64 queenPosition, PieceColor queenColor)
     {
         bool doubleChecked = queenColor == PieceColor.White ? _whiteDoubleCheck : _blackDoubleCheck;
         if ((queenPosition & (queenColor == PieceColor.White ? _piecesBitboards[whiteQueens] : _piecesBitboards[blackQueens])) == 0 || doubleChecked)
         {
             return 0UL;
         }
         
         UInt64 moves = GenerateDiagonalMoves(queenPosition, queenColor);
         if (_attackingLine != 0)
         {
             if (queenColor == PieceColor.White)
             {
                 _whiteCheckingLine |= _attackingLine;
             }
             else
             {
                 _blackCheckingLine |= _attackingLine;
             }
         }
         moves |= GenerateVerticalAndHorizontalMoves(queenPosition, queenColor);
         return moves;
     }
 
     public UInt64 GenerateKingMoves(UInt64 kingPosition, PieceColor kingColor)
     {
         UInt64 moves = 0UL;
         UInt64 currentPosition = kingPosition & (kingColor == PieceColor.White ? _piecesBitboards[whiteKing] : _piecesBitboards[blackKing]);
         UInt64 freeSquares = kingColor == PieceColor.White ? ~_blackVisibleSquares : ~_whiteVisibleSquares;
         UInt64 ownOccupiedSquares = kingColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 enemyOccupiedSquares = kingColor == PieceColor.White ? _blackOccupiedSquares : _whiteOccupiedSquares;
         UInt64 enemyCheckingLine = kingColor == PieceColor.White ? _blackCheckingLine : _whiteCheckingLine;
         UInt64 availableSquares = freeSquares & ~ownOccupiedSquares & ((~enemyCheckingLine) | (enemyCheckingLine & enemyOccupiedSquares));
         bool kingChecked = kingColor == PieceColor.White ? _whiteKingChecked : _blackKingChecked;
         if (currentPosition == 0)
         {
             return moves;
         }
         
         // zwykle ruchy o jedno pole w kazda strone
         moves |= (((currentPosition & (~_columns[0])) << 1) & availableSquares);
         moves |= (((currentPosition & (~_columns[7])) >> 1) & availableSquares);
         moves |= (((currentPosition & (~_columns[7])) << 7) & availableSquares);
         moves |= (((currentPosition & (~_columns[0])) << 9) & availableSquares);
         moves |= ((currentPosition << 8) & availableSquares);
         moves |= (((currentPosition & (~_columns[0])) >> 7) & availableSquares);
         moves |= (((currentPosition & (~_columns[7])) >> 9) & availableSquares);
         moves |= ((currentPosition >> 8) & availableSquares);
         
         // roszada
         if (!kingChecked)
         {
            moves |= CanShortCastle(kingColor);
            moves |= CanLongCastle(kingColor);
         }
         
         return moves;
     }
 
     private ulong CanLongCastle(PieceColor kingColor)
     {
         UInt64 move = 0;
         
         if (kingColor == PieceColor.White)
         {
             _castlingRights &= ~WhiteLongCastle;
             if (_whiteKingMoved || _whiteARookMoved)
             {
                 return move;
             }
 
             if ((_whiteLongCastleSquares & _freeSquares) != _whiteLongCastleSquares)
             {
                 return move;
             }
 
             _castlingRights |= WhiteLongCastle;
             move = 32UL;
         }
         else
         {
             _castlingRights &= ~BlackLongCastle;
             if (_blackKingMoved || _blackARookMoved)
             {
                 return move;
             }
 
             if ((_blackLongCastleSquares & _freeSquares) != _blackLongCastleSquares)
             {
                 return move;
             }
             
             _castlingRights |= BlackLongCastle;
             move = (32UL << 56);
         }
 
         return move;
     }
 
     private UInt64 CanShortCastle(PieceColor kingColor)
     {
         UInt64 move = 0;
         
         if (kingColor == PieceColor.White)
         {
             _castlingRights &= ~WhiteShortCastle;
             if (_whiteKingMoved || _whiteHRookMoved)
             {
                 return move;
             }
 
             if ((_whiteShortCastleSquares & _freeSquares) != _whiteShortCastleSquares)
             {
                 return move;
             }
 
             _castlingRights |= WhiteShortCastle;
             move = 2UL;
         }
         else
         {
             _castlingRights &= ~BlackShortCastle;
             if (_blackKingMoved || _blackHRookMoved)
             {
                 return move;
             }
 
             if ((_blackShortCastleSquares & _freeSquares) != _blackShortCastleSquares)
             {
                 return move;
             }
 
             _castlingRights |= BlackShortCastle;
             move = (2UL << 56);
         }
 
         return move;
     }

     public void Promote(PieceType piece)
     {
         switch (piece)
         {
             case PieceType.Queen:
                 if (_whitePromotion)
                 {
                     _piecesBitboards[whiteQueens] |= _promotionTargetSquare;
                 }

                 if (_blackPromotion)
                 {
                     _piecesBitboards[blackQueens] |= _promotionTargetSquare;
                 }
                 
                 break;
             
             case PieceType.Knight:
                 if (_whitePromotion)
                 {
                     _piecesBitboards[whiteKnights] |= _promotionTargetSquare;
                 }

                 if (_blackPromotion)
                 {
                     _piecesBitboards[blackKnights] |= _promotionTargetSquare;
                 }

                 break;
             
             case PieceType.Bishop:
                 if (_whitePromotion)
                 {
                     _piecesBitboards[whiteBishops] |= _promotionTargetSquare;
                 }

                 if (_blackPromotion)
                 {
                     _piecesBitboards[blackBishops] |= _promotionTargetSquare;
                 }

                 break;
             
             case PieceType.Rook:
                 if (_whitePromotion)
                 {
                     _piecesBitboards[whiteRooks] |= _promotionTargetSquare;
                 }

                 if (_blackPromotion)
                 {
                     _piecesBitboards[blackRooks] |= _promotionTargetSquare;
                 }

                 break;
         }

         if (_whitePromotion)
         {
             _piecesBitboards[whitePawns] &= (~_promotionTargetSquare);
         }

         if (_blackPromotion)
         {
             _piecesBitboards[blackPawns] &= (~_promotionTargetSquare);
         }
         
         _blackPromotion = false;
         _whitePromotion = false;
         _promotionTargetSquare = 0Ul;
     }
     
     public void ShortCastle(PieceColor side)
     {
         if (side == PieceColor.White)
         {
             _piecesBitboards[whiteRooks] &= (~1UL);
             _piecesBitboards[whiteRooks] |= 4UL;
             _whiteHRookMoved = true;
         }
         else
         {
             _piecesBitboards[blackRooks] &= (~(1UL << 56));
             _piecesBitboards[blackRooks] |= (4UL << 56);
             _blackHRookMoved = true;
         }
     }

     public UInt64 GetEnPassantSquare()
     {
         if (_sideToMove == PieceColor.White)
         {
             return _blackEnPassantSquare;
         }
         
         return _whiteEnPassantSquare;
     }
     
     public void EnPassant(UInt64 targetSquare, PieceColor pawnColor)
     {
         if (pawnColor == PieceColor.White)
         {
             targetSquare >>= 8;
             _piecesBitboards[blackPawns] &= (~targetSquare);
         }
         else
         {
             targetSquare <<= 8;
             _piecesBitboards[whitePawns] &= (~targetSquare);
         }
     }
     
     public void LongCastle(PieceColor side)
     {
         if (side == PieceColor.White)
         {
             _piecesBitboards[whiteRooks] &= (~128UL);
             _piecesBitboards[whiteRooks] |= 16UL;
             _whiteARookMoved = true;
         }
         else
         {
             _piecesBitboards[blackRooks] &= (~(128UL << 56));
             _piecesBitboards[blackRooks] |= (16UL << 56);
             _blackARookMoved = true;
         }
     }
 
     private UInt64 GenerateDiagonalMoves(UInt64 piecePosition, PieceColor pieceColor)
     {
         UInt64 moves = 0UL;
         UInt64 ownOccupiedSquares = pieceColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 enemyOccupiedSquares = pieceColor == PieceColor.White ? _blackOccupiedSquares : _whiteOccupiedSquares;
         UInt64 currentPosition = piecePosition;
         UInt64 currentLine = 0UL;
         UInt64 enemyKing = pieceColor == PieceColor.White ? _piecesBitboards[blackKing] : _piecesBitboards[whiteKing];
         UInt64 enemyPinningSquares = pieceColor == PieceColor.White ? _blackPinningSquares : _whitePinningSquares;
         UInt64 enemyCheckingLine = pieceColor == PieceColor.White ? _blackCheckingLine : _whiteCheckingLine;
         UInt64 ownVisibleSquares = 0UL;
         bool kingChecked = pieceColor == PieceColor.White ? _whiteKingChecked : _blackKingChecked;
         bool pinned = ((piecePosition & enemyPinningSquares) != 0);
         bool doubleCheck = pieceColor == PieceColor.White ? _whiteDoubleCheck : _blackDoubleCheck;
         if (doubleCheck)
         {
             return 0UL;
         }
         
         _attackingLine = 0Ul;
         // lewy dolny rog
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_rows[0] | _columns[0])) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 7;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }

             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }

             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & (_rows[0] | _columns[0])) == 0)
                     {
                         currentPosition >>= 7;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }
                 
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         ownVisibleSquares |= currentLine;
         
         // prawy dolny rog
         currentPosition = piecePosition;
         currentLine = 0UL;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_rows[0] | _columns[7])) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 9;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
             
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
 
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & (_rows[0] | _columns[7])) == 0)
                     {
                         currentPosition >>= 9;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }
                 
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         
         // prawy gorny rog
         currentPosition = piecePosition;
         currentLine = 0UL;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_columns[7] | _rows[7])) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 7;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
             
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
 
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & (_columns[7] | _rows[7])) == 0)
                     {
                         currentPosition <<= 7;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }

                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         
         // lewy gorny rog
         currentPosition = piecePosition;
         currentLine = 0UL;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_columns[0] | _rows[7])) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 9;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
 
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & (_columns[0] | _rows[7])) == 0)
                     {
                         currentPosition <<= 9;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }

                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         if (pieceColor == PieceColor.White)
         {
             _whiteVisibleSquares |= ownVisibleSquares;
         }
         else
         {
             _blackVisibleSquares |= ownVisibleSquares;
         }
         
         return moves;
     }
 
     private UInt64 GenerateVerticalAndHorizontalMoves(UInt64 piecePosition, PieceColor pieceColor)
     {
         UInt64 moves = 0UL;
         UInt64 ownOccupiedSquares = pieceColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 enemyOccupiedSquares = pieceColor == PieceColor.White ? _blackOccupiedSquares : _whiteOccupiedSquares;
         UInt64 currentPosition = piecePosition;
         UInt64 currentLine = 0UL;
         UInt64 enemyKing = pieceColor == PieceColor.White ? _piecesBitboards[blackKing] : _piecesBitboards[whiteKing];
         UInt64 enemyPinningSquares = pieceColor == PieceColor.White ? _blackPinningSquares : _whitePinningSquares;
         UInt64 enemyCheckingLine = pieceColor == PieceColor.White ? _blackCheckingLine : _whiteCheckingLine;
         UInt64 ownVisibleSquares = 0Ul;
         bool kingChecked = pieceColor == PieceColor.White ? _whiteKingChecked : _blackKingChecked;
         bool pinned = ((piecePosition & enemyPinningSquares) != 0);
         bool doubleCheck = pieceColor == PieceColor.White ? _whiteDoubleCheck : _blackDoubleCheck;
         if (doubleCheck)
         {
             return 0UL;
         }
         
         _attackingLine = 0Ul;
         // w dol
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _rows[0]) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 8;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
 
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & _rows[0]) == 0)
                     {
                         currentPosition >>= 8;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }
                 
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         
         // w gore
         currentPosition = piecePosition;
         currentLine = 0UL;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _rows[7]) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 8;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
             
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & _rows[7]) == 0)
                     {
                         currentPosition <<= 8;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }
                 
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         
         // w lewo
         currentPosition = piecePosition;
         currentLine = 0UL;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _columns[0]) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 1;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
 
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & _columns[0]) == 0)
                     {
                         currentPosition <<= 1;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }

                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         
         // w prawo
         currentPosition = piecePosition;
         currentLine = 0UL;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _columns[7]) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 1;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 ownVisibleSquares |= currentPosition;
                 break;
             }
             
             if (kingChecked)
             {
                 if ((currentPosition & enemyCheckingLine) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
             
             if (pinned)
             {
                 if ((currentPosition & enemyPinningSquares) == 0)
                 {
                     ownVisibleSquares |= currentPosition;
                     continue;
                 }
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 ownVisibleSquares |= currentPosition;
                 if ((currentPosition & enemyKing) != 0)
                 {
                     _attackingLine |= currentLine;
                     _attackingLine |= piecePosition;
                     while ((currentPosition & _columns[7]) == 0)
                     {
                         currentPosition <<= 1;
                         ownVisibleSquares |= currentPosition;
                     }
                     
                     ownVisibleSquares |= currentPosition;
                 }

                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         ownVisibleSquares |= currentLine;
         moves |= currentLine;
         if (pieceColor == PieceColor.White)
         {
             _whiteVisibleSquares |= ownVisibleSquares;
         }
         else
         {
             _blackVisibleSquares |= ownVisibleSquares;
         }
         
         return moves;
     }
 
     private UInt64 GenerateVerticalAndHorizontalPins(UInt64 piecePosition, PieceColor pieceColor)
     {
         UInt64 currentPosition = piecePosition;
         UInt64 currentLine = 0UL;
         UInt64 ownOccupiedSquares = pieceColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 enemyOccupiedSquares = pieceColor == PieceColor.White ? _blackOccupiedSquares : _whiteOccupiedSquares;
         UInt64 enemyKing = pieceColor == PieceColor.White ? _piecesBitboards[blackKing] : _piecesBitboards[whiteKing];
         UInt64 pins = 0UL;
         bool breakFlag = false;
         
         // w dol
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _rows[0]) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 8;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     
                     break;
                 }
                 
                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }

         currentLine = 0UL;
         currentPosition = piecePosition;
         breakFlag = false;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _rows[7]) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 8;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }
         
         currentLine = 0UL;
         currentPosition = piecePosition;
         breakFlag = false;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _columns[0]) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 1;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }
         
         currentLine = 0UL;
         currentPosition = piecePosition;
         breakFlag = false;
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & _columns[7]) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 1;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }

         if (pins != 0Ul)
         {
             pins |= piecePosition;
         }
         
         return pins;
     }

     private UInt64 GenerateDiagonalPins(UInt64 piecePosition, PieceColor pieceColor)
     {
         UInt64 currentPosition = piecePosition;
         UInt64 currentLine = 0UL;
         UInt64 ownOccupiedSquares = pieceColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 enemyOccupiedSquares = pieceColor == PieceColor.White ? _blackOccupiedSquares : _whiteOccupiedSquares;
         UInt64 enemyKing = pieceColor == PieceColor.White ? _piecesBitboards[blackKing] : _piecesBitboards[whiteKing];
         UInt64 pins = 0UL;
         bool breakFlag = false;

         // lewy dolny rog
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_rows[0] | _columns[0])) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 7;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
             
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }
         
         currentLine = 0UL;
         currentPosition = piecePosition;
         breakFlag = false;
         // prawy dolny rog
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_rows[0] | _columns[7])) != 0)
             {
                 break;   
             }
             
             currentPosition >>= 9;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }
         
         currentLine = 0UL;
         currentPosition = piecePosition;
         breakFlag = false;
         // prawy gorny
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_columns[7] | _rows[7])) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 7;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }
         
         currentLine = 0UL;
         currentPosition = piecePosition;
         breakFlag = false;
         // lewy gorny
         for (int i = 1; i <= 7; i++)
         {
             if ((currentPosition & (_columns[0] | _rows[7])) != 0)
             {
                 break;   
             }
             
             currentPosition <<= 9;
             if ((currentPosition & ownOccupiedSquares) != 0 || currentPosition == 0)
             {
                 break;
             }
             
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 if (breakFlag)
                 {
                     if ((currentPosition & enemyKing) != 0)
                     {
                         pins |= currentLine;
                     }
                     break;
                 }

                 currentLine |= currentPosition;
                 breakFlag = true;
             }
             
             currentLine |= currentPosition;
         }
         
         if (pins != 0Ul)
         {
             pins |= piecePosition;
         }
         
         return pins;
     }

     private void GeneratePins()
     {
         _whitePinningSquares = 0UL;
         _blackPinningSquares = 0UL;
         UInt64 pieces = _piecesBitboards[whiteRooks];
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }

             _whitePinningSquares |= GenerateVerticalAndHorizontalPins(i, PieceColor.White);
         }
         
         pieces = _piecesBitboards[whiteBishops];
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }

             _whitePinningSquares |= GenerateDiagonalPins(i, PieceColor.White);
         }
         
         pieces = _piecesBitboards[whiteQueens];
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }

             _whitePinningSquares |= GenerateDiagonalPins(i, PieceColor.White);
             _whitePinningSquares |= GenerateVerticalAndHorizontalPins(i, PieceColor.White);
         }
         
         pieces = _piecesBitboards[blackRooks];
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }

             _blackPinningSquares |= GenerateVerticalAndHorizontalPins(i, PieceColor.Black);
         }
         
         pieces = _piecesBitboards[blackBishops];
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }

             _blackPinningSquares |= GenerateDiagonalPins(i, PieceColor.Black);
         }

         pieces = _piecesBitboards[blackQueens];
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }

             _blackPinningSquares |= GenerateDiagonalPins(i, PieceColor.Black);
             _blackPinningSquares |= GenerateVerticalAndHorizontalPins(i, PieceColor.Black);
         }
     }
     
     private void GenerateAttackedAndVisibleSquares()
     {
         _whiteVisibleSquares = 0UL;
         _blackVisibleSquares = 0UL;
         _whiteAttackedSquares = 0Ul;
         _blackAttackedSquares = 0UL;
         _whiteCheckingLine = 0UL;
         _blackCheckingLine = 0UL;
         _blackCheckingLine = 0UL;
         _whiteCheckingLine = 0UL;
         _attackingLine = 0UL;
         _whiteDoubleCheck = false;
         _blackDoubleCheck = false;
         _whiteMoves.Clear();
         _blackMoves.Clear();
         UInt64 moves;
         UInt64 pieces = _piecesBitboards[whitePawns];
         for (UInt64 i = 256; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             moves = GenerateWhitePawnMoves(i);
             _whiteMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Pawn, From = i});
             _whiteAttackedSquares |= moves;
             if (_attackingLine != 0UL)
             {
                 if (_whiteCheckingLine != 0UL)
                 {
                     _blackDoubleCheck = true;
                 }
             }
             
             _whiteCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[whiteRooks];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }

             moves = GenerateRookMoves(i, PieceColor.White);
             _whiteAttackedSquares |= moves;
             _whiteMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Rook, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_whiteCheckingLine != 0UL)
                 {
                     _blackDoubleCheck = true;
                 }
             }
             
             _whiteCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[whiteKnights];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             moves = GenerateKnightMoves(i, PieceColor.White);
             _whiteAttackedSquares |= moves;
             _whiteMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Knight, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_whiteCheckingLine != 0UL)
                 {
                     _blackDoubleCheck = true;
                 }
             }
             
             _whiteCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[whiteBishops];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }

             moves = GenerateBishopMoves(i, PieceColor.White);
             _whiteAttackedSquares |= moves;
             _whiteMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Bishop, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_whiteCheckingLine != 0UL)
                 {
                     _blackDoubleCheck = true;
                 }
             }
             
             _whiteCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[whiteQueens];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             moves = GenerateQueenMoves(i, PieceColor.White);
             _whiteAttackedSquares |= moves;
             _whiteMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Queen, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_whiteCheckingLine != 0UL)
                 {
                     _blackDoubleCheck = true;
                 }
             }
             
             _whiteCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[blackPawns];
         _attackingLine = 0UL;
         for (UInt64 i = (128UL << 48); i <= pieces; i >>= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             moves = GenerateBlackPawnMoves(i);
             _blackAttackedSquares |= moves;
             _blackMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Pawn, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_blackCheckingLine != 0UL)
                 {
                     _whiteDoubleCheck = true;
                 }
             }
             
             _blackCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[blackRooks];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             moves = GenerateRookMoves(i, PieceColor.Black);
             _blackAttackedSquares |= moves;
             _blackMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Rook, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_blackCheckingLine != 0UL)
                 {
                     _whiteDoubleCheck = true;
                 }
             }
             
             _blackCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[blackKnights];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
            
             moves = GenerateKnightMoves(i, PieceColor.Black);
             _blackAttackedSquares |= moves;
             _blackMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Knight, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_blackCheckingLine != 0UL)
                 {
                     _whiteDoubleCheck = true;
                 }
             }
             
             _blackCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[blackBishops];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             moves = GenerateBishopMoves(i, PieceColor.Black);
             _blackAttackedSquares |= moves;
             _blackMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Bishop, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_blackCheckingLine != 0UL)
                 {
                     _whiteDoubleCheck = true;
                 }
             }
             
             _blackCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }
         
         pieces = _piecesBitboards[blackQueens];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i <= pieces; i <<= 1)
         {
             if (i == 0UL)
             {
                 break;
             }
             
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             moves = GenerateQueenMoves(i, PieceColor.Black);
             _blackAttackedSquares |= moves;
             _blackMoves.Add(new PieceMoves(){Moves = moves, PieceType = PieceType.Queen, From = i});
             if (_attackingLine != 0UL)
             {
                 if (_whiteCheckingLine != 0UL)
                 {
                     _whiteDoubleCheck = true;
                 }
             }
             
             _blackCheckingLine |= _attackingLine;
             _attackingLine = 0UL;
         }

         // wykrywanie pol widocznych przez bialego krola
         pieces = _piecesBitboards[whiteKing];
         _whiteVisibleSquares |= ((pieces & (~_columns[0])) << 1);
         _whiteVisibleSquares |= ((pieces & (~_columns[7])) >> 1);
         _whiteVisibleSquares |= ((pieces & (~_columns[7])) << 7);
         _whiteVisibleSquares |= ((pieces & (~_columns[0])) << 9);
         _whiteVisibleSquares |= pieces << 8;
         _whiteVisibleSquares |= ((pieces & (~_columns[0])) >> 7);
         _whiteVisibleSquares |= ((pieces & (~_columns[7])) >> 9);
         _whiteVisibleSquares |= pieces >> 8;

         // wykrywanie pol widocznych przez czarnego krola
         pieces = _piecesBitboards[blackKing];
         _blackVisibleSquares |= ((pieces & (~_columns[0])) << 1);
         _blackVisibleSquares |= ((pieces & (~_columns[7])) >> 1);
         _blackVisibleSquares |= ((pieces & (~_columns[7])) << 7);
         _blackVisibleSquares |= ((pieces & (~_columns[0])) << 9);
         _blackVisibleSquares |= pieces << 8;
         _blackVisibleSquares |= ((pieces & (~_columns[0])) >> 7);
         _blackVisibleSquares |= ((pieces & (~_columns[7])) >> 9);
         _blackVisibleSquares |= pieces >> 8;
         _blackAttackedSquares |= GenerateKingMoves(_piecesBitboards[blackKing], PieceColor.Black);
         _whiteAttackedSquares |= GenerateKingMoves(_piecesBitboards[whiteKing], PieceColor.White);
     }

     private readonly double[] _rookPositionsValues = [
         0.0, 0.0, 0.2, 0.3, 0.3, 0.2, 0.0, 0.0,
         0.0, 0.0, 0.2, 0.3, 0.3, 0.2, 0.0, 0.0,
         0.0, 0.0, 0.1, 0.1, 0.1, 0.1, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.1, 0.1, 0.1, 0.1, 0.0, 0.0,
         0.0, 0.0, 0.2, 0.3, 0.3, 0.2, 0.0, 0.0,
         0.0, 0.0, 0.2, 0.3, 0.3, 0.2, 0.0, 0.0
     ];
     private readonly double[] _knightPositionsValues = [
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         -0.1, 0.0, 0.3, 0.3, 0.3, 0.3, 0.0, -0.1,
         -0.1, 0.0, 0.3, 0.5, 0.5, 0.3, 0.0, -0.1,
         -0.1, 0.0, 0.3, 0.5, 0.5, 0.3, 0.0, -0.1,
         -0.1, 0.0, 0.3, 0.3, 0.3, 0.3, 0.0, -0.1,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
     ];
     private readonly double[] _bishopPositionsValues = [
         0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.1,
         0.0, 0.3, 0.0, 0.0, 0.0, 0.0, 0.3, 0.0,
         0.0, 0.1, 0.3, 0.0, 0.0, 0.3, 0.1, 0.0,
         0.0, 0.0, 0.1, 0.2, 0.2, 0.1, 0.0, 0.0,
         0.0, 0.0, 0.1, 0.2, 0.2, 0.1, 0.0, 0.0,
         0.0, 0.1, 0.3, 0.0, 0.0, 0.3, 0.1, 0.0,
         0.0, 0.3, 0.0, 0.0, 0.0, 0.0, 0.3, 0.0,
         0.1, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.1
     ];
     private readonly double[] _queenPositionsValues = [
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
     ];
     private readonly double[] _kingPositionsValues = [
         0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.2, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.3, 0.3, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.4, 0.4, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.4, 0.4, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.3, 0.3, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.2, 0.0, 0.0, 0.0,
         0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0, 0.0
     ];
     private readonly double[] _whitePawnsPositionsValues = [
         1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5,
         0.5, 0.5, 0.52, 0.55, 0.55, 0.52, 0.5, 0.5,
         0.4, 0.4, 0.42, 0.45, 0.45, 0.42, 0.4, 0.4,
         0.3, 0.3, 0.32, 0.35, 0.35, 0.32, 0.3, 0.3,
         0.2, 0.2, 0.22, 0.25, 0.25, 0.22, 0.2, 0.2,
         0.1, 0.1, 0.12, 0.15, 0.15, 0.12, 0.1, 0.1,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
     ];
     private readonly double[] _blackPawnsPositionsValues = [
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
         0.1, 0.1, 0.12, 0.15, 0.15, 0.12, 0.1, 0.1,
         0.2, 0.2, 0.22, 0.25, 0.25, 0.22, 0.2, 0.2,
         0.3, 0.3, 0.32, 0.35, 0.35, 0.32, 0.3, 0.3,
         0.4, 0.4, 0.42, 0.45, 0.45, 0.42, 0.4, 0.4,
         0.5, 0.5, 0.52, 0.55, 0.55, 0.52, 0.5, 0.5,
         1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5, 1.5
     ];

     private readonly double _pawnValue = 1.0;
     private readonly double _rookValue = 5.0;
     private readonly double _knightValue = 3.0;
     private readonly double _bishopValue = 3.0;
     private readonly double _queenValue = 9.0;
     private readonly double _kingValue = 1.0;
    
     public double EvaluatePosition()
     {
         double evaluation = 0.0;
         UInt64 pieces = _piecesBitboards[whitePawns];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }

             evaluation += _pawnValue + _whitePawnsPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[whiteRooks];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation += _rookValue + _rookPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[whiteKnights];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation += _knightValue + _knightPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[whiteBishops];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation += _bishopValue + _bishopPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[whiteQueens];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation += _queenValue + _queenPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[blackPawns];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation -= _pawnValue + _blackPawnsPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[blackRooks];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation -= _rookValue + _rookPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[blackKnights];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation -= _knightValue + _knightPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[blackBishops];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
                 
             evaluation -= _bishopValue + _bishopPositionsValues[63 - i];
         }
         
         pieces = _piecesBitboards[blackQueens];
         for (int i = 0; i < 64 ; i++)
         {
             if ((pieces & (1UL << i)) == 0)
             {
                 continue;
             }
             
             evaluation -= _queenValue + _queenPositionsValues[63 - i];
         }

         for (int i = 0; i < 64; i++)
         {
             if ((1UL << i) == _piecesBitboards[whiteKing])
             {
                 evaluation += _kingValue + _kingPositionsValues[63 - i];
                 break;
             }
         }
         
         for (int i = 0; i < 64; i++)
         {
             if ((1UL << i) == _piecesBitboards[blackKing])
             {
                 evaluation -= _kingValue + _kingPositionsValues[63 - i];
                 break;
             }
         }
         
         return evaluation;
     }
}