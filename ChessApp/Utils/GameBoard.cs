using ChessApp.Pieces;
using ChessEngine;
using CommunityToolkit.Maui.Views;

namespace ChessApp.Utils;

public class GameBoard : VerticalStackLayout
{
    public static readonly UInt16 SquareSize = 100;
    private readonly List<BoardSquare> _board;
    private ChessBoard _logicalBoard;
    private Piece? _selectedPiece = null;
    private Menu _mainMenu;
    public GameBoard(ChessBoard.PieceColor color, Menu menu)
    {
        _board = new List<BoardSquare>(64);
        _logicalBoard = new ChessBoard();
        _mainMenu = menu;
        HorizontalStackLayout currentRow = new HorizontalStackLayout();
        char currentLetter = color == ChessBoard.PieceColor.White ? '8' : '1';
        currentRow.Add(new Label(){Text = currentLetter.ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
        if (color == ChessBoard.PieceColor.White)
            currentLetter--;
        else
            currentLetter++;
        
        // tworzenie pol szachownicy wraz z etykietami rzedow
        for (int i = 0; i < 64; i++)
        {
            BoardSquare frameToAdd = new BoardSquare(i, SquareTapped);
            _board.Add(frameToAdd);
            currentRow.Add(frameToAdd);
            if (i % 8 != 7) continue;
            Add(currentRow);
            currentRow = new HorizontalStackLayout();
            currentRow.Add(new Label(){Text = currentLetter.ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
            if (color == ChessBoard.PieceColor.White)
                currentLetter--;
            else
                currentLetter++;
        }
        
        // dodawanie etykiet kolumn
        currentRow = new HorizontalStackLayout();
        currentRow.Add(new Frame(){WidthRequest = SquareSize, HeightRequest = SquareSize});
        for (int i = 0; i < 8; i++)
        {
            if (color == ChessBoard.PieceColor.White)
                currentRow.Add(new Label(){Text = ((char)('A' + i)).ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
            else
                currentRow.Add(new Label(){Text = ((char)('H' - i)).ToString(), FontSize = 40, WidthRequest = SquareSize, HeightRequest = SquareSize, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});
        }
        Add(currentRow);
        
        // dodawanie pionow 
        for (int i = 0; i < 8; i++)
        {
            _board[i + 8].SetContent(new Pawn(i + 8, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares));
            _board[55 - i].SetContent(new Pawn(55 - i, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares));
        }
        
        // dodawanie reszty bierek
        _board[0].Content = new Rook(0, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares);
        _board[7].Content = new Rook(7, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares);
        _board[63].Content = new Rook(63, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares);
        _board[56].Content = new Rook(56, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares);
        
        _board[1].SetContent(new Knight(1, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares));
        _board[6].Content = new Knight(6, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares);
        _board[62].Content = new Knight(62, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares);
        _board[57].Content = new Knight(57, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares);
        
        _board[2].Content = new Bishop(2, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares);
        _board[5].Content = new Bishop(5, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.Black : ChessBoard.PieceColor.White, DrawMovableSquares);
        _board[61].Content = new Bishop(61, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares);
        _board[58].Content = new Bishop(58, color == ChessBoard.PieceColor.White ? ChessBoard.PieceColor.White : ChessBoard.PieceColor.Black, DrawMovableSquares);
        
        if (color == ChessBoard.PieceColor.White)
        {
            _board[3].Content = new Queen(3, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[4].Content = new King(4, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[60].Content = new King(60, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[59].Content = new Queen(59, ChessBoard.PieceColor.White, DrawMovableSquares);
        }
        else
        {
            _board[4].Content = new Queen(4, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[3].Content = new King(3, ChessBoard.PieceColor.White, DrawMovableSquares);
            _board[59].Content = new King(59, ChessBoard.PieceColor.Black, DrawMovableSquares);
            _board[60].Content = new Queen(60, ChessBoard.PieceColor.Black, DrawMovableSquares);
        }

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
                }
                else if ((tappedSquare.GridIndex == 58) && ((castlingRights & _logicalBoard.WhiteLongCastle) == _logicalBoard.WhiteLongCastle))
                {
                    longCastling = true;
                }
            }
            else
            {
                if (tappedSquare.GridIndex == 6 && (castlingRights & _logicalBoard.BlackShortCastle) == _logicalBoard.BlackShortCastle)
                {
                    shortCastling = true;
                }
                else if ((tappedSquare.GridIndex == 2) && ((castlingRights & _logicalBoard.BlackLongCastle) == _logicalBoard.BlackLongCastle))
                {
                    longCastling = true;
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
                }
            }
            else
            {
                if (tappedSquare.GridIndex / 8 == 7)
                {
                    promotion = true;
                }
            }
        }
        
        UInt64 enPassantSquare = _logicalBoard.GetEnPassantSquare();
        if (_selectedPiece is Pawn)
        {
            if (tappedSquare.Index == enPassantSquare)
            {
                enPassant = true;
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
            }
            
            _board[_selectedPiece.BoardPosition].Content = null;
            tappedSquare.SetContent(_selectedPiece);
            _selectedPiece.BoardPosition = tappedSquare.GridIndex;
            if (promotion)
            {
                var piece = await Promote(tappedSquare.GridIndex, _selectedPiece.GetPieceColor());
                _logicalBoard.Promote(piece);
            }
            
            if (_logicalBoard.GameEnded())
            {
                var popup = new EndGamePopup((ChessBoard.PieceColor)_logicalBoard.GetWinner()!);
                ((ContentPage)(Parent.Parent)).ShowPopup(popup);
            }
            
            ChangeSides();
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

    private void ChangeSides()
    {
        ChessBoard.PieceColor sideToMove = _logicalBoard.GetSideToMove();
        
        foreach (var square in _board)
        {
            if (square.Content == null)
            {
                continue;
            }
            
            Piece piece = (Piece)square.Content;
            piece.IsEnabled = piece.GetPieceColor() == sideToMove;
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
}