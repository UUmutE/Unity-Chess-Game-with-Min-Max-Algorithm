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
            // Ýterasyon sonunda tahtanýn deðerini döndürün
            return calculator.EvaluateBoard(board);
        }

        if (maksimizeEdiyor)
        {
            double enIyiDeger = double.MinValue;

            // Mevcut durumu deðerlendirin ve tüm olasý hamleleri inceleyin
            foreach (var hamle in OlasiHamleleriBul(board,PieceColor.White))
            {
                // Hamleyi yapýn
                ChessPiece temp = TahtayiGuncelle(hamle);

                // Min-Max algoritmasýný rekürsif olarak çaðýrýn
                double deger = MinMax(derinlik - 1, alpha, beta, !maksimizeEdiyor, false);

                if (deger > enIyiDeger && first)
                {
                    hareket = hamle;
                }

                // En iyi deðeri güncelle
                enIyiDeger = Math.Max(enIyiDeger, deger);

                // Hamleyi geri al
                TahtayiGeriAl(hamle, temp);

                // Alpha deðerini güncelle
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

            // Mevcut durumu deðerlendirin ve tüm olasý hamleleri inceleyin
            foreach (var hamle in OlasiHamleleriBul(board,PieceColor.Black))
            {
                // Hamleyi yapýn
                ChessPiece temp=TahtayiGuncelle(hamle);

                // Min-Max algoritmasýný rekürsif olarak çaðýrýn
                double deger = MinMax(derinlik - 1, alpha, beta, !maksimizeEdiyor, false);

                if (deger < enIyiDeger && first)
                {
                    hareket = hamle;
                }

                // En iyi deðeri güncelle
                enIyiDeger = Math.Min(enIyiDeger, deger);

                // Hamleyi geri al
                TahtayiGeriAl(hamle, temp);

                // Beta deðerini güncelle
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
        foreach (var taþ in chessBoard)
        {
            if (taþ != null && taþ.color == color)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (CanMoveToPosition(taþ, i, j) && !WouldCauseCheck(taþ, i, j))
                        {
                            Hareket hareket = new Hareket(taþ.row, taþ.col, i, j);
                            hareketler.Add(hareket);
                        }
                    }
                }
            }
        }
        return hareketler;
    }

    bool CanMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // Hedef pozisyonu geçerli bir pozisyon mu diye kontrol et
        if (targetRow < 0 || targetRow >= 8 || targetCol < 0 || targetCol >= 8)
        {
            // Geçersiz pozisyon, taþ hareket edemez
            return false;
        }
        if (board[targetRow, targetCol] != null)
        {
            if (board[targetRow, targetCol].color == taþ.color)
                return false;
        }
        // Seçilen taþýn türüne göre hareket kurallarýný kontrol et
        switch (taþ.pieceType)
        {
            case PieceType.Pawn_Black:
            case PieceType.Pawn_White:
                return CanPawnMoveToPosition(taþ, targetRow, targetCol);
            case PieceType.Rook_Black:
            case PieceType.Rook_White:
                return CanRookMoveToPosition(taþ, targetRow, targetCol);
            case PieceType.Knight_Black:
            case PieceType.Knight_White:
                return CanKnightMoveToPosition(taþ, targetRow, targetCol);
            case PieceType.Bishop_Black:
            case PieceType.Bishop_White:
                return CanBishopMoveToPosition(taþ, targetRow, targetCol);
            case PieceType.Queen_Black:
            case PieceType.Queen_White:
                return CanQueenMoveToPosition(taþ, targetRow, targetCol);
            case PieceType.King_Black:
            case PieceType.King_White:
                return CanKingMoveToPosition(taþ, targetRow, targetCol);
            default:
                Debug.LogError("Invalid piece type: " + taþ.pieceType);
                return false;
        }
    }

    bool CanPawnMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // Piyonun hareket kurallarýný kontrol et

        int forwardDirection = (taþ.color == PieceColor.White) ? 1 : -1; // Beyaz piyonlar ileri doðru hareket eder, siyah piyonlar geriye doðru hareket eder

        // Ýleri hareket (ayný sütunda, bir adým ileri)
        if (taþ.col == targetCol && taþ.row + forwardDirection == targetRow)
        {
            // Hedef pozisyon boþ ise hareket edebilir
            return board[targetRow, targetCol] == null;
        }

        // Ýlk hareket (ayný sütunda, iki adým ileri)
        if (taþ.col == targetCol && taþ.row + 2 * forwardDirection == targetRow && taþ.row == (taþ.color == PieceColor.White ? 1 : 6) && IsPathClear(taþ, targetRow, targetCol))
        {
            // Ýki adým ileri hareket edebilmesi için hedef pozisyonun boþ olmasý ve ortada bir taþýn bulunmamasý gerekir
            if (board[targetRow, targetCol] == null && board[taþ.row + forwardDirection, targetCol] == null)
            {
                return true;
            }
        }

        // Çapraz hareket (rakibin taþý varsa)
        if (Mathf.Abs(taþ.col - targetCol) == 1 && taþ.row + forwardDirection == targetRow)
        {
            // Hedef pozisyonda rakip taþ varsa taþý yiyebilir
            ChessPiece targetPiece = board[targetRow, targetCol];
            if (targetPiece != null)
            {
                if (targetPiece.color != taþ.color)
                {
                    return true;
                }
            }
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanRookMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // Kale taþýnýn hareket kurallarýný kontrol et

        // Ayný satýrda veya sütunda hareket
        if ((taþ.row == targetRow || taþ.col == targetCol) && IsPathClear(taþ, targetRow, targetCol))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanKnightMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // At taþýnýn hareket kurallarýný kontrol et

        int rowDifference = Mathf.Abs(taþ.row - targetRow);
        int colDifference = Mathf.Abs(taþ.col - targetCol);

        // "L" þeklinde hareket (2 birim yatay ve 1 birim dikey ya da 1 birim yatay ve 2 birim dikey)
        if ((rowDifference == 2 && colDifference == 1) || (rowDifference == 1 && colDifference == 2))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanBishopMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // Fil taþýnýn hareket kurallarýný kontrol et

        int rowDifference = Mathf.Abs(taþ.row - targetRow);
        int colDifference = Mathf.Abs(taþ.col - targetCol);

        // Çapraz hareket (satýr farký ve sütun farký eþit)
        if ((rowDifference == colDifference) && IsPathClear(taþ, targetRow, targetCol))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanQueenMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // Vezir taþýnýn hareket kurallarýný kontrol et

        // Kale ve filin hareket kurallarýný içerir
        return (CanRookMoveToPosition(taþ, targetRow, targetCol) || CanBishopMoveToPosition(taþ, targetRow, targetCol)) && IsPathClear(taþ, targetRow, targetCol);
    }

    bool CanKingMoveToPosition(ChessPiece taþ, int targetRow, int targetCol)
    {
        // Þah taþýnýn hareket kurallarýný kontrol et

        int rowDifference = Mathf.Abs(taþ.row - targetRow);
        int colDifference = Mathf.Abs(taþ.col - targetCol);

        // Yatay, dikey veya çapraz birimlerle hareket (en fazla 1 birim farkla)
        if (((rowDifference == 1 && colDifference == 0) || (rowDifference == 0 && colDifference == 1) || (rowDifference == 1 && colDifference == 1)) && IsPathClear(taþ, targetRow, targetCol))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }
    bool IsPathClear(ChessPiece taþ, int targetRow, int targetCol)
    {
        //Hedef konum ile arada baþka taþ var mý kontrolü

        int rowDirection = Mathf.Clamp(targetRow - taþ.row, -1, 1);
        int colDirection = Mathf.Clamp(targetCol - taþ.col, -1, 1);
        int currentRow = taþ.row + rowDirection;
        int currentCol = taþ.col + colDirection;

        while (currentRow != targetRow || currentCol != targetCol)
        {
            ChessPiece middlePieceObject = board[currentRow, currentCol];

            if (middlePieceObject != null)
            {
                return false; // Aradaki karelerden herhangi birinde taþ var, taþý hareket ettirme
            }

            currentRow += rowDirection;
            currentCol += colDirection;
        }

        return true; // Yol temiz, taþ hareket edebilir
    }

    public bool WouldCauseCheck(ChessPiece taþ, int targetRow, int targetCol)
    {
        int initialRow = taþ.row;
        int initialCol = taþ.col;

        ChessPiece targetPieceObject = board[targetRow, targetCol];

        if (targetPieceObject != null)
        {
            ChessPiece tempPiece = targetPieceObject;
            if (targetPieceObject.color != taþ.color)
            {
                board[targetRow, targetCol] = board[initialRow, initialCol];
                board[initialRow, initialCol] = null;

                // Eðer bu hareket sonucunda kendi rengi için þah durumu oluþursa true döndür
                if (IsInCheck(taþ.color))
                {
                    // Taþý eski pozisyonuna geri yerleþtir
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
            //Taþý hedef pozisyona taþýmadan önce geçici olarak hareket ettir
            board[targetRow, targetCol] = board[initialRow, initialCol];
            board[initialRow, initialCol] = null;

            // Eðer bu hareket sonucunda kendi rengi için þah durumu oluþursa true döndür
            if (IsInCheck(taþ.color))
            {
                // Taþý eski pozisyonuna geri yerleþtir
                board[initialRow, initialCol] = board[targetRow, targetCol];
                board[targetRow, targetCol] = null;
                return true;
            }

            // Taþý eski pozisyonuna geri yerleþtir
            board[initialRow, initialCol] = board[targetRow, targetCol];
            board[targetRow, targetCol] = null;
            return false;
        }
    }
    public bool IsInCheck(PieceColor playerColor)
    {
        // Þah taþýnýn konumunu bul
        ChessPiece kingPiece=null;

        foreach (ChessPiece piece in board)
        {
            if (piece != null)
            {
                if (piece.color == playerColor && (piece.pieceType == PieceType.King_Black || piece.pieceType == PieceType.King_White))
                {
                    kingPiece = piece; // Þah taþý bulundu
                }
            }
        }
        ////////////////////////////DÜZENLE//////////////////
        // Tüm rakip taþlarý dolaþarak þah taþýný tehdit ediyor mu kontrol et
        foreach (ChessPiece piece in board)
        {
            if (piece != null)
            {

                if (piece.color != playerColor)
                {
                    // Rakip taþýn hareket edebileceði tüm pozisyonlarý kontrol et
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (CanMoveToPosition(piece, row, col))
                            {
                                // Rakip taþ, þah taþýnýn bulunduðu pozisyonu tehdit ediyor mu?
                                if (kingPiece != null && row == kingPiece.row && col == kingPiece.col)
                                {
                                    return true; // Þah durumu, oyuncu þah altýnda
                                }
                            }
                        }
                    }
                }
            }
        }

        return false; // Þah durumu yok, oyuncu güvende
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
