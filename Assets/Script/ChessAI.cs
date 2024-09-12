using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ChessAI : MonoBehaviour
{
    ChessPointCalculator calculator;
    private void Start()
    {
        calculator = gameObject.GetComponent<ChessPointCalculator>();
    }
    public ChessPiece[,] board;
    public Hareket hareket;
    public void SelectBestMove(ChessPiece[,] chessBoard)
    {
        board = chessBoard;
    }
    public double MinMax(int derinlik, double alpha, double beta, bool maksimizeEdiyor, bool first)
    {
        Debug.Log("ok");
        if (derinlik == 0)
        {
            // �terasyon sonunda tahtan�n de�erini d�nd�r�n
            return calculator.EvaluateBoard(board);
        }

        if (maksimizeEdiyor)
        {
            double enIyiDeger = double.MinValue;

            // Mevcut durumu de�erlendirin ve t�m olas� hamleleri inceleyin
            foreach (var hamle in OlasiHamleleriBul(board,PieceColor.White))
            {
                // Hamleyi yap�n
                ChessPiece temp = TahtayiGuncelle(hamle);

                // Min-Max algoritmas�n� rek�rsif olarak �a��r�n
                double deger = MinMax(derinlik - 1, alpha, beta, !maksimizeEdiyor, false);

                if (deger > enIyiDeger && first)
                {
                    hareket = hamle;
                }

                // En iyi de�eri g�ncelle
                enIyiDeger = Math.Max(enIyiDeger, deger);

                // Hamleyi geri al
                TahtayiGeriAl(hamle, temp);

                // Alpha de�erini g�ncelle
                alpha = Math.Max(alpha, enIyiDeger);

                // Beta kesmesi yap
                if (beta <= alpha)
                {
                    break;
                }


            }

            return enIyiDeger;
        }
        else
        {
            double enIyiDeger = double.MaxValue;

            // Mevcut durumu de�erlendirin ve t�m olas� hamleleri inceleyin
            foreach (var hamle in OlasiHamleleriBul(board,PieceColor.Black))
            {
                // Hamleyi yap�n
                ChessPiece temp=TahtayiGuncelle(hamle);

                // Min-Max algoritmas�n� rek�rsif olarak �a��r�n
                double deger = MinMax(derinlik - 1, alpha, beta, !maksimizeEdiyor, false);

                if (deger < enIyiDeger && first)
                {
                    hareket = hamle;
                }

                // En iyi de�eri g�ncelle
                enIyiDeger = Math.Min(enIyiDeger, deger);

                // Hamleyi geri al
                TahtayiGeriAl(hamle, temp);

                // Beta de�erini g�ncelle
                beta = Math.Min(beta, enIyiDeger);

                // Alpha kesmesi yap
                if (beta <= alpha)
                {
                    break;
                }


            }
            return enIyiDeger;
        }
    }
    private ChessPiece TahtayiGuncelle(Hareket hamle)
    {
        ChessPiece temp = null;
        if (board[hamle.TargetRow, hamle.TargetColumn] != null)
        {
            temp = (ChessPiece)board[hamle.TargetRow, hamle.TargetColumn].Clone();
            board[hamle.TargetRow, hamle.TargetColumn] = (ChessPiece)board[hamle.SourceRow, hamle.SourceColumn].Clone();
            board[hamle.SourceRow, hamle.SourceColumn] = null;
            board[hamle.TargetRow, hamle.TargetColumn].SetPosition(hamle.TargetRow, hamle.TargetColumn);
        }
        else
        {
            board[hamle.TargetRow, hamle.TargetColumn] = (ChessPiece)board[hamle.SourceRow, hamle.SourceColumn].Clone();
            board[hamle.SourceRow, hamle.SourceColumn] = null;
            board[hamle.TargetRow, hamle.TargetColumn].SetPosition(hamle.TargetRow, hamle.TargetColumn);
        }
        return temp;
    }

    private ChessPiece TahtayiGeriAl(Hareket hamle, ChessPiece temp)
    {
        if (temp != null)
        {
            board[hamle.SourceRow, hamle.SourceColumn] = (ChessPiece)board[hamle.TargetRow, hamle.TargetColumn].Clone();
            board[hamle.TargetRow, hamle.TargetColumn] = (ChessPiece)temp.Clone();
            temp = null;
            board[hamle.SourceRow, hamle.SourceColumn].SetPosition(hamle.SourceRow, hamle.SourceColumn);
        }
        else
        {
            board[hamle.SourceRow, hamle.SourceColumn] = (ChessPiece)board[hamle.TargetRow, hamle.TargetColumn].Clone();
            board[hamle.TargetRow, hamle.TargetColumn] = null;
            board[hamle.SourceRow, hamle.SourceColumn].SetPosition(hamle.SourceRow, hamle.SourceColumn);
        }
        return temp;
    }

    private List<Hareket> OlasiHamleleriBul(ChessPiece[,] chessBoard,PieceColor color)
    {
        List<Hareket> hareketler = new List<Hareket>();
        foreach (var ta� in chessBoard)
        {
            if (ta� != null && ta�.color == color)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (CanMoveToPosition(ta�, i, j) && !WouldCauseCheck(ta�, i, j))
                        {
                            Hareket hareket = new Hareket(ta�.row, ta�.col, i, j);
                            hareketler.Add(hareket);
                        }
                    }
                }
            }
        }
        return hareketler;
    }

    bool CanMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // Hedef pozisyonu ge�erli bir pozisyon mu diye kontrol et
        if (targetRow < 0 || targetRow >= 8 || targetCol < 0 || targetCol >= 8)
        {
            // Ge�ersiz pozisyon, ta� hareket edemez
            return false;
        }
        if (board[targetRow, targetCol] != null)
        {
            if (board[targetRow, targetCol].color == ta�.color)
                return false;
        }
        // Se�ilen ta��n t�r�ne g�re hareket kurallar�n� kontrol et
        switch (ta�.pieceType)
        {
            case PieceType.Pawn_Black:
            case PieceType.Pawn_White:
                return CanPawnMoveToPosition(ta�, targetRow, targetCol);
            case PieceType.Rook_Black:
            case PieceType.Rook_White:
                return CanRookMoveToPosition(ta�, targetRow, targetCol);
            case PieceType.Knight_Black:
            case PieceType.Knight_White:
                return CanKnightMoveToPosition(ta�, targetRow, targetCol);
            case PieceType.Bishop_Black:
            case PieceType.Bishop_White:
                return CanBishopMoveToPosition(ta�, targetRow, targetCol);
            case PieceType.Queen_Black:
            case PieceType.Queen_White:
                return CanQueenMoveToPosition(ta�, targetRow, targetCol);
            case PieceType.King_Black:
            case PieceType.King_White:
                return CanKingMoveToPosition(ta�, targetRow, targetCol);
            default:
                Debug.LogError("Invalid piece type: " + ta�.pieceType);
                return false;
        }
    }

    bool CanPawnMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // Piyonun hareket kurallar�n� kontrol et

        int forwardDirection = (ta�.color == PieceColor.White) ? 1 : -1; // Beyaz piyonlar ileri do�ru hareket eder, siyah piyonlar geriye do�ru hareket eder

        // �leri hareket (ayn� s�tunda, bir ad�m ileri)
        if (ta�.col == targetCol && ta�.row + forwardDirection == targetRow)
        {
            // Hedef pozisyon bo� ise hareket edebilir
            return board[targetRow, targetCol] == null;
        }

        // �lk hareket (ayn� s�tunda, iki ad�m ileri)
        if (ta�.col == targetCol && ta�.row + 2 * forwardDirection == targetRow && ta�.row == (ta�.color == PieceColor.White ? 1 : 6) && IsPathClear(ta�, targetRow, targetCol))
        {
            // �ki ad�m ileri hareket edebilmesi i�in hedef pozisyonun bo� olmas� ve ortada bir ta��n bulunmamas� gerekir
            if (board[targetRow, targetCol] == null && board[ta�.row + forwardDirection, targetCol] == null)
            {
                return true;
            }
        }

        // �apraz hareket (rakibin ta�� varsa)
        if (Mathf.Abs(ta�.col - targetCol) == 1 && ta�.row + forwardDirection == targetRow)
        {
            // Hedef pozisyonda rakip ta� varsa ta�� yiyebilir
            ChessPiece targetPiece = board[targetRow, targetCol];
            if (targetPiece != null)
            {
                if (targetPiece.color != ta�.color)
                {
                    return true;
                }
            }
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanRookMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // Kale ta��n�n hareket kurallar�n� kontrol et

        // Ayn� sat�rda veya s�tunda hareket
        if ((ta�.row == targetRow || ta�.col == targetCol) && IsPathClear(ta�, targetRow, targetCol))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanKnightMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // At ta��n�n hareket kurallar�n� kontrol et

        int rowDifference = Mathf.Abs(ta�.row - targetRow);
        int colDifference = Mathf.Abs(ta�.col - targetCol);

        // "L" �eklinde hareket (2 birim yatay ve 1 birim dikey ya da 1 birim yatay ve 2 birim dikey)
        if ((rowDifference == 2 && colDifference == 1) || (rowDifference == 1 && colDifference == 2))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanBishopMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // Fil ta��n�n hareket kurallar�n� kontrol et

        int rowDifference = Mathf.Abs(ta�.row - targetRow);
        int colDifference = Mathf.Abs(ta�.col - targetCol);

        // �apraz hareket (sat�r fark� ve s�tun fark� e�it)
        if ((rowDifference == colDifference) && IsPathClear(ta�, targetRow, targetCol))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanQueenMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // Vezir ta��n�n hareket kurallar�n� kontrol et

        // Kale ve filin hareket kurallar�n� i�erir
        return (CanRookMoveToPosition(ta�, targetRow, targetCol) || CanBishopMoveToPosition(ta�, targetRow, targetCol)) && IsPathClear(ta�, targetRow, targetCol);
    }

    bool CanKingMoveToPosition(ChessPiece ta�, int targetRow, int targetCol)
    {
        // �ah ta��n�n hareket kurallar�n� kontrol et

        int rowDifference = Mathf.Abs(ta�.row - targetRow);
        int colDifference = Mathf.Abs(ta�.col - targetCol);

        // Yatay, dikey veya �apraz birimlerle hareket (en fazla 1 birim farkla)
        if (((rowDifference == 1 && colDifference == 0) || (rowDifference == 0 && colDifference == 1) || (rowDifference == 1 && colDifference == 1)) && IsPathClear(ta�, targetRow, targetCol))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }
    bool IsPathClear(ChessPiece ta�, int targetRow, int targetCol)
    {
        //Hedef konum ile arada ba�ka ta� var m� kontrol�

        int rowDirection = Mathf.Clamp(targetRow - ta�.row, -1, 1);
        int colDirection = Mathf.Clamp(targetCol - ta�.col, -1, 1);
        int currentRow = ta�.row + rowDirection;
        int currentCol = ta�.col + colDirection;

        while (currentRow != targetRow || currentCol != targetCol)
        {
            ChessPiece middlePieceObject = board[currentRow, currentCol];

            if (middlePieceObject != null)
            {
                return false; // Aradaki karelerden herhangi birinde ta� var, ta�� hareket ettirme
            }

            currentRow += rowDirection;
            currentCol += colDirection;
        }

        return true; // Yol temiz, ta� hareket edebilir
    }

    public bool WouldCauseCheck(ChessPiece ta�, int targetRow, int targetCol)
    {
        int initialRow = ta�.row;
        int initialCol = ta�.col;

        ChessPiece targetPieceObject = board[targetRow, targetCol];

        if (targetPieceObject != null)
        {
            ChessPiece tempPiece = targetPieceObject;
            if (targetPieceObject.color != ta�.color)
            {
                board[targetRow, targetCol] = board[initialRow, initialCol];
                board[initialRow, initialCol] = null;

                // E�er bu hareket sonucunda kendi rengi i�in �ah durumu olu�ursa true d�nd�r
                if (IsInCheck(ta�.color))
                {
                    // Ta�� eski pozisyonuna geri yerle�tir
                    board[initialRow, initialCol] = board[targetRow, targetCol];
                    board[targetRow, targetCol] = tempPiece;
                    return true;
                }
            }

            board[initialRow, initialCol] = board[targetRow, targetCol];
            board[targetRow, targetCol] = tempPiece;
            return false;
        }
        else
        {
            //Ta�� hedef pozisyona ta��madan �nce ge�ici olarak hareket ettir
            board[targetRow, targetCol] = board[initialRow, initialCol];
            board[initialRow, initialCol] = null;

            // E�er bu hareket sonucunda kendi rengi i�in �ah durumu olu�ursa true d�nd�r
            if (IsInCheck(ta�.color))
            {
                // Ta�� eski pozisyonuna geri yerle�tir
                board[initialRow, initialCol] = board[targetRow, targetCol];
                board[targetRow, targetCol] = null;
                return true;
            }

            // Ta�� eski pozisyonuna geri yerle�tir
            board[initialRow, initialCol] = board[targetRow, targetCol];
            board[targetRow, targetCol] = null;
            return false;
        }
    }
    public bool IsInCheck(PieceColor playerColor)
    {
        // �ah ta��n�n konumunu bul
        ChessPiece kingPiece=null;

        foreach (ChessPiece piece in board)
        {
            if (piece != null)
            {
                if (piece.color == playerColor && (piece.pieceType == PieceType.King_Black || piece.pieceType == PieceType.King_White))
                {
                    kingPiece = piece; // �ah ta�� bulundu
                }
            }
        }
        ////////////////////////////D�ZENLE//////////////////
        // T�m rakip ta�lar� dola�arak �ah ta��n� tehdit ediyor mu kontrol et
        foreach (ChessPiece piece in board)
        {
            if (piece != null)
            {

                if (piece.color != playerColor)
                {
                    // Rakip ta��n hareket edebilece�i t�m pozisyonlar� kontrol et
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (CanMoveToPosition(piece, row, col))
                            {
                                // Rakip ta�, �ah ta��n�n bulundu�u pozisyonu tehdit ediyor mu?
                                if (kingPiece != null && row == kingPiece.row && col == kingPiece.col)
                                {
                                    return true; // �ah durumu, oyuncu �ah alt�nda
                                }
                            }
                        }
                    }
                }
            }
        }

        return false; // �ah durumu yok, oyuncu g�vende
    }
}

public class Hareket
{
    public int SourceRow { get; }
    public int SourceColumn { get; }
    public int TargetRow { get; }
    public int TargetColumn { get; }

    public Hareket(int sourceRow, int sourceColumn, int targetRow, int targetColumn)
    {
        SourceRow = sourceRow;
        SourceColumn = sourceColumn;
        TargetRow = targetRow;
        TargetColumn = targetColumn;
    }
}
