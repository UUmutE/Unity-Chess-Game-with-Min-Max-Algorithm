using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPointCalculator : MonoBehaviour
{
    ChessPiece[,] board;
    // Satranç tahtasýnýn durumunu puanlaþtýran fonksiyon
    public double EvaluateBoard(ChessPiece[,] chessBoard)
    {
        board = chessBoard;
        int boardSize = 8;

        double totalEvaluation = 0;

        // Satranç tahtasýný dolaþarak taþ deðerlerini hesapla
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i,j] != null)
                    totalEvaluation += GetPieceValue(board[i, j], i, j);
            }
        }

        return totalEvaluation;
    }

    // Beyaz piyonlarýn deðerlerini tutan dizi
    private static readonly double[,] PawnEvalWhite =
    {
        {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
        {5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0, 5.0},
        {1.0, 1.0, 2.0, 3.0, 3.0, 2.0, 1.0, 1.0},
        {0.5, 0.5, 1.0, 2.5, 2.5, 1.0, 0.5, 0.5},
        {0.0, 0.0, 0.0, 2.0, 2.0, 0.0, 0.0, 0.0},
        {0.5, -0.5, -1.0, 0.0, 0.0, -1.0, -0.5, 0.5},
        {0.5, 1.0, 1.0, -2.0, -2.0, 1.0, 1.0, 0.5},
        {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0}
    };

    // Siyah piyonlarýn deðerlerini hesaplamak için beyaz piyon deðerlerini tersine çevirir
    private static readonly double[,] PawnEvalBlack = ReverseArray(PawnEvalWhite);

    // Diðer taþ türlerinin deðerlerini tanýmlayan diziler (at, fil, kale, vezir, þah için ayrý diziler)
    private static readonly double[,] KnightEval =
    {
        {-5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0},
        {-4.0, -2.0, 0.0, 0.0, 0.0, 0.0, -2.0, -4.0},
        {-3.0, 0.0, 1.0, 1.5, 1.5, 1.0, 0.0, -3.0},
        {-3.0, 0.5, 1.5, 2.0, 2.0, 1.5, 0.5, -3.0},
        {-3.0, 0.0, 1.5, 2.0, 2.0, 1.5, 0.0, -3.0},
        {-3.0, 0.5, 1.0, 1.5, 1.5, 1.0, 0.5, -3.0},
        {-4.0, -2.0, 0.0, 0.5, 0.5, 0.0, -2.0, -4.0},
        {-5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0}
    };

    private static readonly double[,] BishopEvalWhite =
    {
        {-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0},
        {-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1.0},
        {-1.0, 0.0, 0.5, 1.0, 1.0, 0.5, 0.0, -1.0},
        {-1.0, 0.5, 0.5, 1.0, 1.0, 0.5, 0.5, -1.0},
        {-1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 0.0, -1.0},
        {-1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, -1.0},
        {-1.0, 0.5, 0.0, 0.0, 0.0, 0.0, 0.5, -1.0},
        {-2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0}
    };

    private static readonly double[,] BishopEvalBlack = ReverseArray(BishopEvalWhite);

    private static readonly double[,] RookEvalWhite =
    {
        {0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0},
        {0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.5},
        {-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5},
        {-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5},
        {-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5},
        {-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5},
        {-0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.5},
        {0.0, 0.0, 0.0, 0.5, 0.5, 0.0, 0.0, 0.0}
    };

    private static readonly double[,] RookEvalBlack = ReverseArray(RookEvalWhite);

    private static readonly double[,] EvalQueen =
    {
        {-2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0},
        {-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -1.0},
        {-1.0, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -1.0},
        {-0.5, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -0.5},
        {0.0, 0.0, 0.5, 0.5, 0.5, 0.5, 0.0, -0.5},
        {-1.0, 0.5, 0.5, 0.5, 0.5, 0.5, 0.0, -1.0},
        {-1.0, 0.0, 0.5, 0.0, 0.0, 0.0, 0.0, -1.0},
        {-2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0}
    };

    private static readonly double[,] KingEvalWhite =
    {
        {-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
        {-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
        {-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
        {-3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0},
        {-2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0},
        {-1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0},
        {2.0, 2.0, 0.0, 0.0, 0.0, 0.0, 2.0, 2.0},
        {2.0, 3.0, 1.0, 0.0, 0.0, 1.0, 3.0, 2.0}
    };

    private static readonly double[,] KingEvalBlack = ReverseArray(KingEvalWhite);

    // Taþ deðerlerini hesaplayan fonksiyon
    public static double GetPieceValue(ChessPiece piece, int x, int y)
    {
        if (piece == null)
        {
            return 0;
        }

        // Taþýn türüne ve rengine göre deðeri hesapla
        double GetAbsoluteValue(ChessPiece piece, bool isWhite, int x, int y)
        {
            if (piece.pieceType == PieceType.Pawn_White || piece.pieceType == PieceType.Pawn_Black)
            {
                return 10 + (isWhite ? PawnEvalWhite[y, x] : PawnEvalBlack[y, x]);
            }
            else if (piece.pieceType == PieceType.Rook_White || piece.pieceType == PieceType.Rook_Black)
            {
                return 50 + (isWhite ? RookEvalWhite[y, x] : RookEvalBlack[y, x]);
            }
            else if (piece.pieceType == PieceType.Knight_White || piece.pieceType == PieceType.Knight_Black)
            {
                return 30 + KnightEval[y, x];
            }
            else if (piece.pieceType == PieceType.Bishop_White || piece.pieceType == PieceType.Bishop_Black)
            {
                return 30 + (isWhite ? BishopEvalWhite[y, x] : BishopEvalBlack[y, x]);
            }
            else if (piece.pieceType == PieceType.Queen_White || piece.pieceType == PieceType.Queen_Black)
            {
                return 90 + EvalQueen[y, x];
            }
            else if (piece.pieceType == PieceType.King_White || piece.pieceType == PieceType.King_Black)
            {
                return 900 + (isWhite ? KingEvalWhite[y, x] : KingEvalBlack[y, x]);
            }
            throw new Exception("Unknown piece type: " + piece.pieceType);
        }

        double absoluteValue = GetAbsoluteValue(piece, piece.color == PieceColor.White, x, y);
        return piece.color == PieceColor.White ? absoluteValue : -absoluteValue;
    }

    // Ýki boyutlu bir diziyi tersine çeviren yardýmcý fonksiyon
    private static double[,] ReverseArray(double[,] arr)
    {
        int rows = arr.GetLength(0);
        int cols = arr.GetLength(1);
        double[,] reversed = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                reversed[i, j] = arr[rows - 1 - i, j];
            }
        }

        return reversed;
    }
}
