using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceColor
{
    White,
    Black
}

public enum PieceType
{
    Pawn_Black,
    Pawn_White,
    Rook_Black,
    Rook_White,
    Knight_Black,
    Knight_White,
    Bishop_Black,
    Bishop_White,
    Queen_Black,
    Queen_White,
    King_Black,
    King_White
}

public class ChessPiece : MonoBehaviour, ICloneable
{
    public PieceType pieceType; // Taþ türü
    public int row; // Satýr numarasý
    public int col; // Sütun numarasý
    public PieceColor color;

    private int boardSize = 8;

    public void SetPosition(int rowIndex, int colIndex)
    {
        row = rowIndex;
        col = colIndex;
    }
    
    public void SetColor(PieceColor pieceColor)
    {
        color = pieceColor;
    }

    public bool CanMoveToPosition(int targetRow, int targetCol)
    {
        // Hedef pozisyonu geçerli bir pozisyon mu diye kontrol et
        if (targetRow < 0 || targetRow >= boardSize || targetCol < 0 || targetCol >= boardSize)
        {
            // Geçersiz pozisyon, taþ hareket edemez
            return false;
        }

        // Seçilen taþýn türüne göre hareket kurallarýný kontrol et
        switch (pieceType)
        {
            case PieceType.Pawn_Black:
            case PieceType.Pawn_White:
                return CanPawnMoveToPosition(targetRow, targetCol);
            case PieceType.Rook_Black:
            case PieceType.Rook_White:
                return CanRookMoveToPosition(targetRow, targetCol);
            case PieceType.Knight_Black:
            case PieceType.Knight_White:
                return CanKnightMoveToPosition(targetRow, targetCol);
            case PieceType.Bishop_Black:
            case PieceType.Bishop_White:
                return CanBishopMoveToPosition(targetRow, targetCol);
            case PieceType.Queen_Black:
            case PieceType.Queen_White:
                return CanQueenMoveToPosition(targetRow, targetCol);
            case PieceType.King_Black:
            case PieceType.King_White:
                return CanKingMoveToPosition(targetRow, targetCol);
            default:
                Debug.LogError("Invalid piece type: " + pieceType);
                return false;
        }
    }

    bool CanPawnMoveToPosition(int targetRow, int targetCol)
    {
        // Piyonun hareket kurallarýný kontrol et

        int forwardDirection = (color == PieceColor.White) ? 1 : -1; // Beyaz piyonlar ileri doðru hareket eder, siyah piyonlar geriye doðru hareket eder

        // Ýleri hareket (ayný sütunda, bir adým ileri)
        if (col == targetCol && row + forwardDirection == targetRow)
        {
            // Hedef pozisyon boþ ise hareket edebilir
            return transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(targetRow, targetCol) == null;
        }

        // Ýlk hareket (ayný sütunda, iki adým ileri)
        if (col == targetCol && row + 2 * forwardDirection == targetRow && row == (color == PieceColor.White ? 1 : 6) && IsPathClear(targetRow, targetCol))
        {
            // Ýki adým ileri hareket edebilmesi için hedef pozisyonun boþ olmasý ve ortada bir taþýn bulunmamasý gerekir
            if (transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(targetRow, targetCol) == null && transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(row + forwardDirection, targetCol) == null)
            {
                return true;
            }
        }

        // Çapraz hareket (rakibin taþý varsa)
        if (Mathf.Abs(col - targetCol) == 1 && row + forwardDirection == targetRow)
        {
            // Hedef pozisyonda rakip taþ varsa ve farklý renkteyse, taþý yiyebilir
            GameObject targetPieceObject = transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(targetRow, targetCol);
            if (targetPieceObject != null)
            {
                ChessPiece targetPiece = targetPieceObject.GetComponent<ChessPiece>();
                if (targetPiece.color != color)
                {
                    return true;
                }
            }
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanRookMoveToPosition(int targetRow, int targetCol)
    {
        // Kale taþýnýn hareket kurallarýný kontrol et

        // Ayný satýrda veya sütunda hareket
        if ((row == targetRow || col == targetCol) && IsPathClear(targetRow, targetCol))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanKnightMoveToPosition(int targetRow, int targetCol)
    {
        // At taþýnýn hareket kurallarýný kontrol et

        int rowDifference = Mathf.Abs(row - targetRow);
        int colDifference = Mathf.Abs(col - targetCol);

        // "L" þeklinde hareket (2 birim yatay ve 1 birim dikey ya da 1 birim yatay ve 2 birim dikey)
        if ((rowDifference == 2 && colDifference == 1) || (rowDifference == 1 && colDifference == 2))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanBishopMoveToPosition(int targetRow, int targetCol)
    {
        // Fil taþýnýn hareket kurallarýný kontrol et

        int rowDifference = Mathf.Abs(row - targetRow);
        int colDifference = Mathf.Abs(col - targetCol);

        // Çapraz hareket (satýr farký ve sütun farký eþit)
        if ((rowDifference == colDifference) && IsPathClear(targetRow, targetCol))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }

    bool CanQueenMoveToPosition(int targetRow, int targetCol)
    {
        // Vezir taþýnýn hareket kurallarýný kontrol et

        // Kale ve filin hareket kurallarýný içerir
        return (CanRookMoveToPosition(targetRow, targetCol) || CanBishopMoveToPosition(targetRow, targetCol)) && IsPathClear(targetRow, targetCol);
    }

    bool CanKingMoveToPosition(int targetRow, int targetCol)
    {
        // Þah taþýnýn hareket kurallarýný kontrol et

        int rowDifference = Mathf.Abs(row - targetRow);
        int colDifference = Mathf.Abs(col - targetCol);

        // Yatay, dikey veya çapraz birimlerle hareket (en fazla 1 birim farkla)
        if (((rowDifference == 1 && colDifference == 0) || (rowDifference == 0 && colDifference == 1) || (rowDifference == 1 && colDifference == 1)) && IsPathClear(targetRow, targetCol))
        {
            return true;
        }

        // Diðer durumlarda hedef pozisyon geçerli deðil
        return false;
    }
    bool IsPathClear(int targetRow, int targetCol)
    {
        //Hedef konum ile arada baþka taþ var mý kontrolü

        int rowDirection = Mathf.Clamp(targetRow - row, -1, 1);
        int colDirection = Mathf.Clamp(targetCol - col, -1, 1);
        int currentRow = row + rowDirection;
        int currentCol = col + colDirection;

        while (currentRow != targetRow || currentCol != targetCol)
        {
            GameObject middlePieceObject = transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(currentRow, currentCol);

            if (middlePieceObject != null)
            {
                return false; // Aradaki karelerden herhangi birinde taþ var, taþý hareket ettirme
            }

            currentRow += rowDirection;
            currentCol += colDirection;
        }

        return true; // Yol temiz, taþ hareket edebilir
    }

    public bool WouldCauseCheck(int targetRow, int targetCol)
    {
        int initialRow = row;
        int initialCol = col;

        GameObject targetPieceObject = transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(targetRow, targetCol);

        if (targetPieceObject != null)
        {
            if (targetPieceObject.GetComponent<ChessPiece>().color != color)
            {
                targetPieceObject.SetActive(false);
                SetPosition(targetRow, targetCol);

                // Eðer bu hareket sonucunda kendi rengi için þah durumu oluþursa true döndür
                if (transform.parent.GetComponent<ChessBoard>().IsInCheck(color))
                {
                    // Taþý eski pozisyonuna geri yerleþtir
                    SetPosition(initialRow, initialCol);
                    targetPieceObject.SetActive(true);
                    return true;
                }
            }

            SetPosition(initialRow, initialCol);
            targetPieceObject.SetActive(true);
            return false;
        }
        else
        {
            //Taþý hedef pozisyona taþýmadan önce geçici olarak hareket ettir
            SetPosition(targetRow, targetCol);

            // Eðer bu hareket sonucunda kendi rengi için þah durumu oluþursa true döndür
            if (transform.parent.GetComponent<ChessBoard>().IsInCheck(color))
            {
                // Taþý eski pozisyonuna geri yerleþtir
                SetPosition(initialRow, initialCol);
                return true;
            }

            // Taþý eski pozisyonuna geri yerleþtir
            SetPosition(initialRow, initialCol);
            return false;
        }
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}