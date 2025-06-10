using System.Net.Sockets;
using ChessApp.Pieces;
using ChessEngine;
using CommunityToolkit.Maui.Views;

namespace ChessApp.Utils;

public class MultiplayerGameBoard : VerticalStackLayout
{
    public static readonly UInt16 SquareSize = 100;
    private readonly List<BoardSquare> _board;
    private ChessBoard _logicalBoard;
    private Piece? _selectedPiece = null;
    private ChessBoard.PieceColor _color;
    private TcpClient _client;
    private NetworkStream _stream;
    private Byte[] _message = new Byte[1];
    private Thread _mmoThread;
    private Menu _mainMenu;
    private readonly byte _shortCastle = 1;
    private readonly byte _longCastle = 2;
    private readonly byte _promotion = 3;
    private readonly byte _enPassant = 4;
    public MultiplayerGameBoard(Menu menu)
    {
        _client = new TcpClient("localhost", 10105);
        _stream = _client.GetStream();
        _mmoThread = new Thread(Listener);
        _mainMenu = menu;
        int size = _stream.Read(_message, 0, 1);
        _board = new List<BoardSquare>(64);
        _logicalBoard = new ChessBoard();
        _color = _message[0] == 1 ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black;
        HorizontalStackLayout currentRow = new HorizontalStackLayout();
        char currentLetter = '8';
        currentRow.Add(new Label(){Text = currentLetter.ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
        currentLetter--;

        
        // tworzenie pol szachownicy wraz z etykietami rzedow
        for (int i = 0; i < 64; i++)
        {
            BoardSquare frameToAdd = new BoardSquare(i, SquareTapped);
            Console.WriteLine(frameToAdd.GridIndex);
            ChessEngine.Utils.PrintBitboard(frameToAdd.Index);
            _board.Add(frameToAdd);
            currentRow.Add(frameToAdd);
            if (i % 8 != 7) continue;
            Add(currentRow);
            currentRow = new HorizontalStackLayout();
            currentRow.Add(new Label(){Text = currentLetter.ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
            currentLetter--;
        }
        
        // dodawanie etykiet kolumn
        currentRow = new HorizontalStackLayout();
        currentRow.Add(new Border(){WidthRequest = SquareSize, HeightRequest = SquareSize});
        for (int i = 0; i < 8; i++)
        {
            currentRow.Add(new Label(){Text = ((char)('A' + i)).ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
        }
        Add(currentRow);
        
        // dodawanie pionow
        if (_color == ChessBoard.PieceColor.White)
        {
            for (int i = 0; i < 8; i++)
            {
                _board[i + 8].Content = new Image(){Source = "bpawn.png"};
                _board[55 - i].SetContent(new Pawn(55 - i, ChessBoard.PieceColor.White, DrawMovableSquares));
            }
        
            // dodawanie reszty bierek
            _board[0].Content = new Image(){Source = "brook.png"};
            _board[7].Content = new Image(){Source = "brook.png"};
            _board[63].Content = new Rook(63, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[56].Content = new Rook(56, ChessBoard.PieceColor.White, DrawMovableSquares);
            
            _board[1].Content = new Image(){Source = "bknight.png"};
            _board[6].Content = new Image(){Source = "bknight.png"};
            _board[62].Content = new Knight(62, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[57].Content = new Knight(57, ChessBoard.PieceColor.White, DrawMovableSquares);
            
            _board[2].Content = new Image(){Source = "bbishop.png"};
            _board[5].Content = new Image(){Source = "bbishop.png"};
            _board[61].Content = new Bishop(61, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[58].Content = new Bishop(58, ChessBoard.PieceColor.White, DrawMovableSquares);
            
            _board[3].Content = new Image(){Source = "bqueen.png"};
            _board[4].Content = new Image(){Source = "bking.png"};
            _board[60].Content = new King(60, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[59].Content = new Queen(59, ChessBoard.PieceColor.White, DrawMovableSquares);
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                _board[i + 8].Content = new Pawn(i + 8, ChessBoard.PieceColor.Black, DrawMovableSquares);
                _board[55 - i].Content = new Image(){Source = "wpawn.png"};
            }
        
            // dodawanie reszty bierek
            _board[0].Content = new Rook(0, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[7].Content =new Rook(7, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[63].Content = new Image(){Source = "wrook.png"};
            _board[56].Content = new Image(){Source = "wrook.png"};
            
            _board[1].Content = new Knight(1, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[6].Content = new Knight(6, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[62].Content = new Image(){Source = "wknight.png"};
            _board[57].Content = new Image(){Source = "wknight.png"};
            
            _board[2].Content = new Bishop(2, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[5].Content = new Bishop(5, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[61].Content = new Image(){Source = "wbishop.png"};
            _board[58].Content = new Image(){Source = "wbishop.png"};
            
            _board[3].Content = new Queen(3, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[4].Content = new King(4, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[60].Content = new Image() { Source = "wking" };
            _board[59].Content = new Image() { Source = "wqueen" };
        }
        
        _mmoThread.Start();
    }  
    
    private void DrawMovableSquares(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }
        
        ClearBoard();
        Piece clickedPiece = (Piece)sender;
        _selectedPiece = clickedPiece;
        UInt64 moves = clickedPiece.GetLegalMoves(ref _logicalBoard);
        for (int i = 0; i < 64; i++)
        {
            if (moves == 0)
            {
                break;
            }
            
            if ((moves & 1Ul) != 0)
            {
                _board[63 - i].SetColor(BoardSquare.SquareColor.Movable);
            }
            
            moves >>= 1;
        }
    }

    private async void SquareTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        BoardSquare? tappedSquare = (BoardSquare?)sender;
        if (tappedSquare is null || _selectedPiece is null)
        {
            return;
        }

        UInt64 from = _selectedPiece.Position;
        UInt64 to = tappedSquare.Index;
        UInt64 additinalInfo = 0Ul;
        byte pieceType = _selectedPiece.PieceType;
        byte type = 0;
        bool shortCastling = false;
        bool longCastling = false;
        bool enPassant = false;
        bool promotion = false;
        if (_selectedPiece is King)
        {
            int castlingRights = _logicalBoard.GetCastlingRights();
            if (_selectedPiece.GetPieceColor() == ChessBoard.PieceColor.White)
            {
                if ((tappedSquare.GridIndex == 62) && ((castlingRights & _logicalBoard.WhiteShortCastle) == _logicalBoard.WhiteShortCastle))
                {
                    shortCastling = true;
                    type = _shortCastle;
                }
                else if ((tappedSquare.GridIndex == 58) && ((castlingRights & _logicalBoard.WhiteLongCastle) == _logicalBoard.WhiteLongCastle))
                {
                    longCastling = true;
                    type = _longCastle;
                }
            }
            else
            {
                if (tappedSquare.GridIndex == 6 && (castlingRights & _logicalBoard.BlackShortCastle) == _logicalBoard.BlackShortCastle)
                {
                    shortCastling = true;
                    type = _shortCastle;
                }
                else if ((tappedSquare.GridIndex == 2) && ((castlingRights & _logicalBoard.BlackLongCastle) == _logicalBoard.BlackLongCastle))
                {
                    longCastling = true;
                    type = _longCastle;
                }
            }
        }
        else if (_selectedPiece is Pawn)
        {
            if (_selectedPiece.GetPieceColor() == ChessBoard.PieceColor.White)
            {
                if (tappedSquare.GridIndex / 8 == 0)
                {
                    promotion = true;
                    type = _promotion;
                }
            }
            else
            {
                if (tappedSquare.GridIndex / 8 == 7)
                {
                    promotion = true;
                    type = _promotion;
                }
            }
        }

        UInt64 enPassantSquare = _logicalBoard.GetEnPassantSquare();
        if (_selectedPiece is Pawn)
        {
            if (tappedSquare.Index == enPassantSquare)
            {
                enPassant = true;
                type = _enPassant;
                additinalInfo = enPassantSquare;
            }
        }
        
        if (tappedSquare.BackgroundColor.Equals(Colors.Bisque) && _selectedPiece.Move(tappedSquare.Index, ref _logicalBoard))
        {
            _mainMenu.UpdateStatus(_logicalBoard.EvaluatePosition(), from, to, _selectedPiece.PieceTypeBoard, _selectedPiece.Color);
            if (shortCastling)
            {
                ShortCastle(_selectedPiece.GetPieceColor());
                _logicalBoard.ShortCastle(_selectedPiece.GetPieceColor());
            }
            else if (longCastling)
            {
                LongCastle(_selectedPiece.GetPieceColor());
                _logicalBoard.LongCastle(_selectedPiece.GetPieceColor());
            }

            if (enPassant)
            {
                EnPassant(enPassantSquare, _selectedPiece.GetPieceColor());
                _logicalBoard.EnPassant(enPassantSquare, _selectedPiece.GetPieceColor());
                additinalInfo = enPassantSquare;
            }
            
            _board[_selectedPiece.BoardPosition].Content = null;
            tappedSquare.SetContent(_selectedPiece);
            _selectedPiece.BoardPosition = tappedSquare.GridIndex;
            if (promotion)
            {
                var piece = await Promote(tappedSquare.GridIndex, _selectedPiece.GetPieceColor());
                additinalInfo = (UInt64)piece;
                _logicalBoard.Promote(piece);
            }
            
            _stream.Write(BitConverter.GetBytes(from), 0, sizeof(UInt64));
            _stream.Write(BitConverter.GetBytes(to), 0, sizeof(UInt64));
            _stream.Write([pieceType], 0, 1);
            _stream.Write([type], 0, 1);
            _stream.Write(BitConverter.GetBytes(additinalInfo), 0, sizeof(UInt64));
        }
        
        ClearBoard();
        _selectedPiece = null;
    }
    
    private void EnPassant(UInt64 targetSquare, ChessBoard.PieceColor pawnColor)
    {
        if (pawnColor == ChessBoard.PieceColor.White)
        {
            targetSquare >>= 8;
        }
        else
        {
            targetSquare <<= 8;
        }
        
        for (int i = 0; i < 64; i++)
        {
            if (_board[i].Index == targetSquare)
            {
                _board[i].Content = null;
            }
        }
    }

    private async Task<ChessBoard.PieceType> Promote(int promotionTargetSquare, ChessBoard.PieceColor color)
    {
        ChoosePiecePopup popup = new ChoosePiecePopup();
        var chosenPiece = (ChessBoard.PieceType)(await ((ContentPage)(Parent.Parent)).ShowPopupAsync(popup));
        _board[promotionTargetSquare].Content = null;

        if (chosenPiece is ChessBoard.PieceType.Queen)
        {
            _board[promotionTargetSquare].Content = new Queen(promotionTargetSquare, color, DrawMovableSquares);
        }
        else if (chosenPiece is ChessBoard.PieceType.Rook)
        {
            _board[promotionTargetSquare].Content = new Rook(promotionTargetSquare, color, DrawMovableSquares);
        }
        else if (chosenPiece is ChessBoard.PieceType.Knight)
        {
            _board[promotionTargetSquare].Content = new Knight(promotionTargetSquare, color, DrawMovableSquares);
        }
        else
        {
            _board[promotionTargetSquare].Content = new Bishop(promotionTargetSquare, color, DrawMovableSquares);
        }
        
        return chosenPiece;
    }
    private void ClearBoard()
    {
        foreach (var square in _board)
        {
            square.SetColor(BoardSquare.SquareColor.Default);
        }
    }

    private void ShortCastle(ChessBoard.PieceColor side)
    {
        if (side == ChessBoard.PieceColor.White)
        {
            Piece? rook = (Piece?)_board[63].Content;
            if (rook is null)
            {
                return;
            }

            _board[63].Content = null;
            _board[61].Content = rook;
            rook.BoardPosition = 61;
            rook.Position = 4Ul;
        }
        else
        {
            Piece? rook = (Piece?)_board[7].Content;
            if (rook is null)
            {
                return;
            }
            
            _board[7].Content = null;
            _board[5].Content = rook;
            rook.BoardPosition = 5;
            rook.Position = (4Ul << 56);
        }
    }

    private void LongCastle(ChessBoard.PieceColor side)
    {
        if (side == ChessBoard.PieceColor.White)
        {
            Piece? rook = (Piece?)_board[56].Content;
            if (rook is null)
            {
                return;
            }

            _board[56].Content = null;
            _board[59].Content = rook;
            rook.BoardPosition = 61;
            rook.Position = 16Ul;
        }
        else
        {
            Piece? rook = (Piece?)_board[0].Content;
            if (rook is null)
            {
                return;
            }
            
            _board[0].Content = null;
            _board[3].Content = rook;
            rook.BoardPosition = 3;
            rook.Position = (16Ul << 56);
        }
    }

    private void Listener()
    {
        byte[] fromBytes = new byte[sizeof(UInt64)];
        byte[] toBytes = new byte[sizeof(UInt64)];
        byte[] pieceBytes = new byte[1];
        byte[] typeBytes = new byte[1];
        byte[] additionalInfoBytes = new byte[sizeof(UInt64)];
        UInt64 from;
        UInt64 to;
        UInt64 additionalInfo;
        BoardSquare fromSquare = null!;
        BoardSquare toSquare = null!;
        while (_logicalBoard.GetWinner() is null)
        {
            var size = _stream.Read(fromBytes, 0, sizeof(UInt64));
            size = _stream.Read(toBytes, 0, sizeof(UInt64));
            size = _stream.Read(pieceBytes, 0, 1);
            size = _stream.Read(typeBytes, 0, 1);
            size = _stream.Read(additionalInfoBytes, 0, sizeof(UInt64));
            additionalInfo = BitConverter.ToUInt64(additionalInfoBytes, 0); 
            ChessBoard.PieceType pieceToMove;
            switch (pieceBytes[0])
            {
                case 1:
                    pieceToMove = ChessBoard.PieceType.Rook;
                    break;
                
                case 2:
                    pieceToMove = ChessBoard.PieceType.Knight;
                    break;
                
                case 3:
                    pieceToMove = ChessBoard.PieceType.Bishop;
                    break;
                
                case 4:
                    pieceToMove = ChessBoard.PieceType.Queen;
                    break;
                
                case 5:
                    pieceToMove = ChessBoard.PieceType.King;
                    break;
                
                default:
                    pieceToMove = ChessBoard.PieceType.Pawn;
                    break;
            }
            
            from = BitConverter.ToUInt64(fromBytes, 0);
            to = BitConverter.ToUInt64(toBytes, 0);
            Console.WriteLine(_logicalBoard.MakeMove(from, to, pieceToMove, _color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White));
            foreach (var square in _board)
            {
                if (square.Index == from)
                {
                    fromSquare = square;
                }

                if (square.Index == to)
                {
                    toSquare = square;
                }
            }
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                toSquare.Content = fromSquare.Content;
                fromSquare.Content = null;
                _mainMenu.UpdateStatus(_logicalBoard.EvaluatePosition(), from, to, pieceToMove, _color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White);
                if (typeBytes[0] == _longCastle)
                {
                    if (_color == ChessBoard.PieceColor.Black)
                    {
                        Image? rook = (Image?)_board[56].Content;
                        if (rook is null)
                        {
                            return;
                        }

                        _board[56].Content = null;
                        _board[59].Content = rook;
                    }
                    else
                    {
                        Image? rook = (Image?)_board[0].Content;
                        if (rook is null)
                        {
                            return;
                        }
            
                        _board[0].Content = null;
                        _board[3].Content = rook;
                    }
                    
                    _logicalBoard.ShortCastle(_color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White);
                }
                else if (typeBytes[0] == _shortCastle)
                {
                    if (_color == ChessBoard.PieceColor.Black)
                    {
                        Image? rook = (Image?)_board[63].Content;
                        if (rook is null)
                        {
                            return;
                        }

                        _board[63].Content = null;
                        _board[61].Content = rook;
                    }
                    else
                    {
                        Image? rook = (Image?)_board[7].Content;
                        if (rook is null)
                        {
                            return;
                        }
            
                        _board[7].Content = null;
                        _board[5].Content = rook;
                    }
                    
                    _logicalBoard.ShortCastle(_color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White);
                }
                else if (typeBytes[0] == _promotion)
                {
                    ChessBoard.PieceType piece = (ChessBoard.PieceType)additionalInfo; 
                    _logicalBoard.Promote(piece);
                    int promotionTargetSquare = (int)UInt64.Log2(to);
                    toSquare.Content = null;
                    if (piece is ChessBoard.PieceType.Queen)
                    {
                        toSquare.Content = new Image(){Source = _color == ChessBoard.PieceColor.White ? "bqueen.png" : "wqueen.png"};
                    }
                    else if (piece is ChessBoard.PieceType.Rook)
                    {
                        toSquare.Content = new Image(){Source = _color == ChessBoard.PieceColor.White ? "brook.png" : "wrook.png"};
                    }
                    else if (piece is ChessBoard.PieceType.Knight)
                    {
                        toSquare.Content = new Image(){Source = _color == ChessBoard.PieceColor.White ? "bknight.png" : "wknight.png"};
                    }
                    else
                    {
                        toSquare.Content = new Image(){Source = _color == ChessBoard.PieceColor.White ? "bbishop.png" : "wbishop.png"};
                    }
                }
                else if (typeBytes[0] == _enPassant)
                {
                    EnPassant(additionalInfo, _color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White);
                }
            });
        }
    }
}