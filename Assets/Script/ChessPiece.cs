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
    public PieceType pieceType; // Ta� t�r�
    public int row; // Sat�r numaras�
    public int col; // S�tun numaras�
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
        // Hedef pozisyonu ge�erli bir pozisyon mu diye kontrol et
        if (targetRow < 0 || targetRow >= boardSize || targetCol < 0 || targetCol >= boardSize)
        {
            // Ge�ersiz pozisyon, ta� hareket edemez
            return false;
        }

        // Se�ilen ta��n t�r�ne g�re hareket kurallar�n� kontrol et
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
        // Piyonun hareket kurallar�n� kontrol et

        int forwardDirection = (color == PieceColor.White) ? 1 : -1; // Beyaz piyonlar ileri do�ru hareket eder, siyah piyonlar geriye do�ru hareket eder

        // �leri hareket (ayn� s�tunda, bir ad�m ileri)
        if (col == targetCol && row + forwardDirection == targetRow)
        {
            // Hedef pozisyon bo� ise hareket edebilir
            return transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(targetRow, targetCol) == null;
        }

        // �lk hareket (ayn� s�tunda, iki ad�m ileri)
        if (col == targetCol && row + 2 * forwardDirection == targetRow && row == (color == PieceColor.White ? 1 : 6) && IsPathClear(targetRow, targetCol))
        {
            // �ki ad�m ileri hareket edebilmesi i�in hedef pozisyonun bo� olmas� ve ortada bir ta��n bulunmamas� gerekir
            if (transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(targetRow, targetCol) == null && transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(row + forwardDirection, targetCol) == null)
            {
                return true;
            }
        }

        // �apraz hareket (rakibin ta�� varsa)
        if (Mathf.Abs(col - targetCol) == 1 && row + forwardDirection == targetRow)
        {
            // Hedef pozisyonda rakip ta� varsa ve farkl� renkteyse, ta�� yiyebilir
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

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanRookMoveToPosition(int targetRow, int targetCol)
    {
        // Kale ta��n�n hareket kurallar�n� kontrol et

        // Ayn� sat�rda veya s�tunda hareket
        if ((row == targetRow || col == targetCol) && IsPathClear(targetRow, targetCol))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanKnightMoveToPosition(int targetRow, int targetCol)
    {
        // At ta��n�n hareket kurallar�n� kontrol et

        int rowDifference = Mathf.Abs(row - targetRow);
        int colDifference = Mathf.Abs(col - targetCol);

        // "L" �eklinde hareket (2 birim yatay ve 1 birim dikey ya da 1 birim yatay ve 2 birim dikey)
        if ((rowDifference == 2 && colDifference == 1) || (rowDifference == 1 && colDifference == 2))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanBishopMoveToPosition(int targetRow, int targetCol)
    {
        // Fil ta��n�n hareket kurallar�n� kontrol et

        int rowDifference = Mathf.Abs(row - targetRow);
        int colDifference = Mathf.Abs(col - targetCol);

        // �apraz hareket (sat�r fark� ve s�tun fark� e�it)
        if ((rowDifference == colDifference) && IsPathClear(targetRow, targetCol))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }

    bool CanQueenMoveToPosition(int targetRow, int targetCol)
    {
        // Vezir ta��n�n hareket kurallar�n� kontrol et

        // Kale ve filin hareket kurallar�n� i�erir
        return (CanRookMoveToPosition(targetRow, targetCol) || CanBishopMoveToPosition(targetRow, targetCol)) && IsPathClear(targetRow, targetCol);
    }

    bool CanKingMoveToPosition(int targetRow, int targetCol)
    {
        // �ah ta��n�n hareket kurallar�n� kontrol et

        int rowDifference = Mathf.Abs(row - targetRow);
        int colDifference = Mathf.Abs(col - targetCol);

        // Yatay, dikey veya �apraz birimlerle hareket (en fazla 1 birim farkla)
        if (((rowDifference == 1 && colDifference == 0) || (rowDifference == 0 && colDifference == 1) || (rowDifference == 1 && colDifference == 1)) && IsPathClear(targetRow, targetCol))
        {
            return true;
        }

        // Di�er durumlarda hedef pozisyon ge�erli de�il
        return false;
    }
    bool IsPathClear(int targetRow, int targetCol)
    {
        //Hedef konum ile arada ba�ka ta� var m� kontrol�

        int rowDirection = Mathf.Clamp(targetRow - row, -1, 1);
        int colDirection = Mathf.Clamp(targetCol - col, -1, 1);
        int currentRow = row + rowDirection;
        int currentCol = col + colDirection;

        while (currentRow != targetRow || currentCol != targetCol)
        {
            GameObject middlePieceObject = transform.parent.GetComponent<ChessBoard>().FindPieceAtPosition(currentRow, currentCol);

            if (middlePieceObject != null)
            {
                return false; // Aradaki karelerden herhangi birinde ta� var, ta�� hareket ettirme
            }

            currentRow += rowDirection;
            currentCol += colDirection;
        }

        return true; // Yol temiz, ta� hareket edebilir
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

                // E�er bu hareket sonucunda kendi rengi i�in �ah durumu olu�ursa true d�nd�r
                if (transform.parent.GetComponent<ChessBoard>().IsInCheck(color))
                {
                    // Ta�� eski pozisyonuna geri yerle�tir
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
            //Ta�� hedef pozisyona ta��madan �nce ge�ici olarak hareket ettir
            SetPosition(targetRow, targetCol);

            // E�er bu hareket sonucunda kendi rengi i�in �ah durumu olu�ursa true d�nd�r
            if (transform.parent.GetComponent<ChessBoard>().IsInCheck(color))
            {
                // Ta�� eski pozisyonuna geri yerle�tir
                SetPosition(initialRow, initialCol);
                return true;
            }

            // Ta�� eski pozisyonuna geri yerle�tir
            SetPosition(initialRow, initialCol);
            return false;
        }
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}