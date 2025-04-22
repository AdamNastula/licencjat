namespace ChessEngine;

public class ChessBoard
{
  public enum PieceColor { White, Black }
 
     public enum PieceType
     {
         Pawn,
         Rook,
         Knight,
         Bishop,
         Queen,
         King
     }
     
     private const uint WhitePawns = 0;
     private const uint WhiteRooks = 1;
     private const uint WhiteKnights = 2;
     private const uint WhiteBishops = 3;
     private const uint WhiteQueens = 4;
     private const uint WhiteKing = 5;
     private const uint BlackPawns = 6;
     private const uint BlackRooks = 7;
     private const uint BlackKnights = 8;
     private const uint BlackBishops = 9;
     private const uint BlackQueens = 10;
     private const uint BlackKing = 11;

     private readonly UInt64 _whitePawnsStartPositions = 65280;
     private readonly UInt64 _blackPawnsStartPostions = 65280UL << 40;
     private readonly UInt64[] _columns = new UInt64[8];
     private readonly UInt64[] _rows = new UInt64[8];
     private readonly UInt64 _notAbFile;
     private readonly UInt64 _notGhFile;
     
     private readonly UInt64[] _piecesBitboards = new UInt64[12];
     private PieceColor _sideToMove = PieceColor.White;
     private UInt64 _freeSquares = 0UL;
     private UInt64 _occupiedSquares = 0UL;
     private UInt64 _blackOccupiedSquares = 0UL;
     private UInt64 _whiteOccupiedSquares = 0UL;
     private UInt64 _whiteAttackedSquares = 0UL;
     private UInt64 _blackAttackedSquares = 0UL;

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

     private UInt64 _attackingLine = 0UL;
     private UInt64 _enPassantSquare = 0UL;
     
     // TO DO:
     // Wykrywanie XRay attacks i pinow przy pomocy blizniaczych funkcji do poruszania
     // Przeszukiwanie do drugiej napotkanej figury i sprawdzanie czy krol jest w ataku - essa
     public ChessBoard()
     {
         _piecesBitboards[WhitePawns] = 255UL << 8;   // inicjalizacja bialych pionow
         _piecesBitboards[WhiteRooks] = 129UL;        // inicjalizacja bialych wiez
         _piecesBitboards[WhiteKnights] = 66UL;       // inicjalizacja bialych skoczkow
         _piecesBitboards[WhiteBishops] = 36UL;       // inicjalizacja bialych goncow
         _piecesBitboards[WhiteQueens] = 16UL;        // inicjalizacja bialych hetmanow
         _piecesBitboards[WhiteKing] = 8UL;           // inicjalizacja bialego krola
         
         _piecesBitboards[BlackPawns] = 255UL << 48;  // inicjalizacja czarnych pionow
         _piecesBitboards[BlackRooks] = 129UL << 56;  // inicjalizacja czarnych wiez
         _piecesBitboards[BlackKnights] = 66UL << 56; // inicjalizacja czarnych skoczkow
         _piecesBitboards[BlackBishops] = 36UL << 56; // inicjalizacja czarnych goncow
         _piecesBitboards[BlackQueens] = 16UL << 56;  // inicjalizacja czarnych hetmanow
         _piecesBitboards[BlackKing] = 8UL << 56;     // inicjalizacja czarnego krola
 
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
     
     public bool MakeMove(UInt64 from, UInt64 to, PieceType piece, PieceColor color)
     {
         int ownPiecesIndex = color == PieceColor.White ? 0 : 6;
         int enemyPiecesIndex = color == PieceColor.White ? 6 : 0;
         Func<UInt64> moveGenerator = () => 0Ul;
         ref UInt64 pieceBitboard = ref _piecesBitboards[0];
         _enPassantSquare = 0Ul;
         _whiteKingChecked = false;
         _blackKingChecked = false;
 
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
                     _enPassantSquare = to >> 8;
                 }
             }
             else
             {
                 if ((from & _rows[6]) == from && (to & _rows[4]) == to)
                 {
                     _enPassantSquare = to << 8;
                 }
             }
         }

         // szachowanie
         
         GenerateAttackedSquares();
         if ((_piecesBitboards[WhiteKing] & _blackAttackedSquares) != 0)
         {
             _whiteKingChecked = true;
             Console.WriteLine("BIALY KROL POD SZACHEM!");
             Utils.PrintBitboard(_blackAttackedSquares);
         }

         if ((_piecesBitboards[BlackKing] & _whiteAttackedSquares) != 0)
         {
             _blackKingChecked = true;
             Console.WriteLine("CZARNY KROL POD SZACHEM!");
             Utils.PrintBitboard(_whiteAttackedSquares);
         }
         
         _sideToMove = _sideToMove == PieceColor.White ? PieceColor.Black : PieceColor.White;
         return true;
     }

     public UInt64 GenerateWhitePawnMoves(UInt64 pawnPosition)
     {
         UInt64 moves = 0Ul;
         UInt64 currentPosition = pawnPosition & _piecesBitboards[WhitePawns];
         if (currentPosition == 0)
         {
             return moves;
         }
         
         // jeden ruch do przodu
         moves |= (currentPosition << 8) & _freeSquares;
         
         // dwa ruchy do przodu jesli to mozliwe
         if (moves != 0)
         {
             moves |= ((currentPosition & _whitePawnsStartPositions) << 16) & _freeSquares;
         }
         
         // bicie w lewo
         moves |= ((currentPosition & ~_columns[0]) << 9) & _blackOccupiedSquares;
         if (((((currentPosition & ~_columns[0]) << 9) & _blackOccupiedSquares) & _piecesBitboards[BlackKing]) != 0)
         {
             _attackingLine = ((currentPosition & ~_columns[0]) << 9) & _blackOccupiedSquares;
         }
         
         // bicie w prawo
         moves |= ((currentPosition & ~_columns[7]) << 7) & _blackOccupiedSquares;
         if (((((currentPosition & ~_columns[7]) << 7) & _blackOccupiedSquares) & _piecesBitboards[BlackKing]) != 0)
         {
             _attackingLine = ((currentPosition & ~_columns[7]) << 7) & _blackOccupiedSquares;
         }
         
         return moves;
     }
     
     public UInt64 GenerateBlackPawnMoves(UInt64 pawnPosition)
     {
         UInt64 moves = 0UL;
         UInt64 currentPosition = pawnPosition & _piecesBitboards[BlackPawns];
         if (currentPosition == 0)
         {
             return moves;
         }
         
         // jeden ruch do przodu
         moves |= (currentPosition >> 8) & _freeSquares;
         
         // dwa ruchy do przodu jesli to mozliwe
         if (moves != 0)
         {
             moves |= ((currentPosition & _blackPawnsStartPostions) >> 16) & _freeSquares;
         }
         
         // bicie w prawo
         moves |= ((currentPosition & ~_columns[7]) >> 9) & _whiteOccupiedSquares;
         if (((((currentPosition & ~_columns[7]) >> 9) & _whiteOccupiedSquares) & _piecesBitboards[WhiteKing]) != 0)
         {
             _attackingLine = ((currentPosition & ~_columns[7]) >> 9) & _whiteOccupiedSquares;
         }
         
         // bicie w lewo
         moves |= ((currentPosition & ~_columns[0]) >> 7) & _whiteOccupiedSquares;
         if (((((currentPosition & ~_columns[0]) >> 7) & _whiteOccupiedSquares) & _piecesBitboards[WhiteKing]) != 0)
         {
             _attackingLine = ((currentPosition & ~_columns[0]) >> 7) & _whiteOccupiedSquares;
         }
         
         return moves;
     }
 
     public UInt64 GenerateKnightMoves(UInt64 knightPosition, PieceColor knightColor)
     {
         UInt64 moves = 0UL;
         UInt64 ownOccupiedSquares = knightColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares;
         UInt64 currentPosition = knightPosition & (knightColor == PieceColor.White ? _piecesBitboards[WhiteKnights] : _piecesBitboards[BlackKnights]);
         UInt64 enemyKing = knightColor == PieceColor.White ? _piecesBitboards[BlackKing] : _piecesBitboards[WhiteKing];
         UInt64 freeSquares = ~ownOccupiedSquares;
         if (currentPosition == 0)
         {
             return moves;
         }

         UInt64 pos = (((currentPosition & _notAbFile) >> 6) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }

         pos = (((currentPosition & _notGhFile) << 6) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }

         pos = (((currentPosition & _notGhFile) >> 10) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }
         
         pos = (((currentPosition & _notAbFile) << 10) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }
         
         pos = (((currentPosition & (~_columns[7])) << 15) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }
         
         pos = (((currentPosition & (~_columns[0])) >> 15) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }

         pos = (((currentPosition & (~_columns[0])) << 17) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }
         
         pos = (((currentPosition & (~_columns[7])) >> 17) & freeSquares);
         moves |= pos;
         if ((pos & enemyKing) != 0)
         {
             _attackingLine = pos;
         }
         
         return moves;
     }
     public UInt64 GenerateRookMoves(UInt64 rookPosition, PieceColor rookColor)
     {
         if ((rookPosition & (rookColor == PieceColor.White ? _piecesBitboards[WhiteRooks] : _piecesBitboards[BlackRooks])) == 0)
         {
             return 0UL;
         }
         
         return GenerateVerticalAndHorizontalMoves(rookPosition, rookColor);
     }
 
     public UInt64 GenerateBishopMoves(UInt64 bishopPosition, PieceColor bishopColor)
     {
         if ((bishopPosition & (bishopColor == PieceColor.White ? _piecesBitboards[WhiteBishops] : _piecesBitboards[BlackBishops])) == 0)
         {
             return 0UL;
         }
         
         return GenerateDiagonalMoves(bishopPosition, bishopColor);
     }
 
     public UInt64 GenerateQueenMoves(UInt64 queenPosition, PieceColor queenColor)
     {
         if ((queenPosition & (queenColor == PieceColor.White ? _piecesBitboards[WhiteQueens] : _piecesBitboards[BlackQueens])) == 0)
         {
             return 0UL;
         }
         
         UInt64 moves = GenerateDiagonalMoves(queenPosition, queenColor);
         moves |= GenerateVerticalAndHorizontalMoves(queenPosition, queenColor);
         return moves;
     }
 
     public UInt64 GenerateKingMoves(UInt64 kingPosition, PieceColor kingColor)
     {
         UInt64 moves = 0UL;
         UInt64 currentPosition = kingPosition & (kingColor == PieceColor.White ? _piecesBitboards[WhiteKing] : _piecesBitboards[BlackKing]);
         UInt64 freeSquares = ~(kingColor == PieceColor.White ? _whiteOccupiedSquares : _blackOccupiedSquares);
         if (currentPosition == 0)
         {
             return moves;
         }
         
         // zwykle ruchy o jedno pole w kazda strone
         moves |= (((currentPosition & (~_columns[0])) << 1) & freeSquares);
         moves |= (((currentPosition & (~_columns[7])) >> 1) & freeSquares);
         moves |= (((currentPosition & (~_columns[7])) << 7) & freeSquares);
         moves |= (((currentPosition & (~_columns[0])) << 9) & freeSquares);
         moves |= ((currentPosition << 8) & freeSquares);
         moves |= (((currentPosition & (~_columns[0])) >> 7) & freeSquares);
         moves |= (((currentPosition & (~_columns[7])) >> 9) & freeSquares);
         moves |= ((currentPosition >> 8) & freeSquares);
         
         // roszada 
         moves |= CanShortCastle(kingColor);
         moves |= CanLongCastle(kingColor);
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

     public void ShortCastle(PieceColor side)
     {
         if (side == PieceColor.White)
         {
             _piecesBitboards[WhiteRooks] &= (~1UL);
             _piecesBitboards[WhiteRooks] |= 4UL;
             _whiteHRookMoved = true;
         }
         else
         {
             _piecesBitboards[BlackRooks] &= (~(1UL << 56));
             _piecesBitboards[BlackRooks] |= (4UL << 56);
             _blackHRookMoved = true;
         }
     }

     public void LongCastle(PieceColor side)
     {
         if (side == PieceColor.White)
         {
             _piecesBitboards[WhiteRooks] &= (~128UL);
             _piecesBitboards[WhiteRooks] |= 16UL;
             _whiteARookMoved = true;
         }
         else
         {
             _piecesBitboards[BlackRooks] &= (~(128UL << 56));
             _piecesBitboards[BlackRooks] |= (16UL << 56);
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
         UInt64 enemyKing = pieceColor == PieceColor.White ? _piecesBitboards[BlackKing] : _piecesBitboards[WhiteKing];
         
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
                 currentLine |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
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
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
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
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
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
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 currentLine |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
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
         UInt64 enemyKing = pieceColor == PieceColor.White ? _piecesBitboards[BlackKing] : _piecesBitboards[WhiteKing];

         
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
                 moves |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
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
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 moves |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
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
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 moves |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
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
                 break;
             }
 
             if ((currentPosition & enemyOccupiedSquares) != 0)
             {
                 moves |= currentPosition;
                 break;
             }
             
             currentLine |= currentPosition;
         }
         
         moves |= currentLine;
         if ((currentLine & enemyKing) != 0)
         {
             _attackingLine = currentLine;
         }
         
         return moves;
     }
 
     private UInt64 GenerateVerticalAndHorizontalVisibleSquares()
     {
         UInt64 visibleSquares = 0UL;
         
         return visibleSquares;
     }
     
     private void GenerateAttackedSquares()
     {
         _whiteAttackedSquares = 0Ul;
         _blackAttackedSquares = 0UL;
         
         UInt64 pieces = _piecesBitboards[WhitePawns];
         _attackingLine = 0UL;
         for (UInt64 i = 256; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateWhitePawnMoves(i);
             if (_attackingLine != 0)
             {
                 _whiteAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[WhiteRooks];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateRookMoves(i, PieceColor.White);
             if (_attackingLine != 0)
             {
                 _whiteAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[WhiteKnights];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateKnightMoves(i, PieceColor.White);
             if (_attackingLine != 0)
             {
                 _whiteAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[WhiteBishops];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateBishopMoves(i, PieceColor.White);
             if (_attackingLine != 0)
             {
                 _whiteAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[WhiteQueens];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateQueenMoves(i, PieceColor.White);
             if (_attackingLine != 0)
             {
                 _whiteAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[BlackPawns];
         _attackingLine = 0UL;
         for (UInt64 i = (128UL << 48); i != 0 ; i >>= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateBlackPawnMoves(i);
             if (_attackingLine != 0)
             {
                 _blackAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[BlackRooks];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateRookMoves(i, PieceColor.Black);
             if (_attackingLine != 0)
             {
                 _blackAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[BlackKnights];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateKnightMoves(i, PieceColor.Black);
             if (_attackingLine != 0)
             {
                 _blackAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[BlackBishops];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
                 
             GenerateBishopMoves(i, PieceColor.Black);
             if (_attackingLine != 0)
             {
                 _blackAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
         
         pieces = _piecesBitboards[BlackQueens];
         _attackingLine = 0UL;
         for (UInt64 i = 1; i != 0 ; i <<= 1)
         {
             if ((pieces & i) == 0)
             {
                 continue;
             }
             
             GenerateQueenMoves(i, PieceColor.Black);
             if (_attackingLine != 0)
             {
                 Utils.PrintBitboard(_attackingLine | i);
                 _blackAttackedSquares |= (_attackingLine | i);
                 _attackingLine = 0UL;
             }
         }
     }
}