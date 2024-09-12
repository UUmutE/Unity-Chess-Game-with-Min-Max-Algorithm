using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    public GameObject tilePrefab; // Kullan�lacak kare objesi prefab�
    public GameObject[] piecePrefabs; // Kullan�lacak ta� objesi prefablar�
    public int boardSize = 8; // Tahta boyutu (8x8)
    
    private float tileSize = 10f; // Kare boyutu
    private PieceColor currentPlayerColor = PieceColor.White; // Ba�lang��ta beyaz oyuncuyla ba�layal�m

    private ChessPiece selectedPiece;// Se�ilen ta�

    void Start()
    {
        GenerateChessboard();
        PlaceChessPieces();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Fare t�klamas�n� kontrol et

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // T�klanan objeyi kontrol et

                ChessPiece clickedPiece = hit.transform.GetComponent<ChessPiece>();
                ChessboardTile clickedTile = hit.transform.GetComponent<ChessboardTile>();

                if (clickedPiece != null && clickedPiece.color == currentPlayerColor && currentPlayerColor == PieceColor.White)
                {
                    // T�klanan obje bir ta� ve s�ras� oyuncunun rengine ait

                    if (selectedPiece != null)
                    {
                        // �nceden se�ilmi� bir ta� varsa, se�imini iptal et
                        DeselectPiece();
                    }

                    // Ta�� se�
                    SelectPiece(clickedPiece);
                }
                else if (clickedTile != null && selectedPiece != null)
                {
                    // T�klanan obje bir kare ve bir ta� se�ilmi�

                    int targetRow = clickedTile.row;
                    int targetCol = clickedTile.col;

                    if (selectedPiece.CanMoveToPosition(targetRow, targetCol) && !selectedPiece.WouldCauseCheck(targetRow, targetCol))
                    {
                        ChessboardTile tile = FindTileAtPosition(selectedPiece.row, selectedPiece.col);

                        if (tile != null)
                        {
                            // Kare rengini as�l rengine d�nd�r
                            Renderer tileRenderer = tile.GetComponent<Renderer>();
                            if ((selectedPiece.row + selectedPiece.col) % 2 == 0)
                            {
                                // Siyah renk
                                tileRenderer.material.color = Color.black;
                            }
                            else
                            {
                                // Beyaz renk
                                tileRenderer.material.color = Color.white;
                            }
                        }
                        // Ta�� hedef pozisyona hareket ettir
                        MovePiece(selectedPiece, targetRow, targetCol);
                    }

                    // Ta��n se�imini iptal et
                    DeselectPiece();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ChessPiece[,] board = new ChessPiece[boardSize, boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    GameObject piece = FindPieceAtPosition(i, j);
                    if (piece != null)
                    {
                        board[i, j] = (ChessPiece)piece.GetComponent<ChessPiece>().Clone();
                    }
                }
            }
            gameObject.GetComponent<ChessAI>().SelectBestMove(board);

            chessAI = gameObject.GetComponent<ChessAI>();
            thread�al���yor = true;
            thread = new Thread(AIMove);
            thread.Start();
        }
        if (thread�al���yor && thread.ThreadState == ThreadState.Stopped)
        {
            Debug.Log("asd");
            Hareket hamle = chessAI.hareket;
            ChessPiece Ai = FindPieceAtPosition(hamle.SourceRow, hamle.SourceColumn).GetComponent<ChessPiece>();

            MovePiece(Ai, hamle.TargetRow, hamle.TargetColumn);
            thread�al���yor = false;
        }
    }
    Thread thread;
    ChessAI chessAI;
    bool thread�al���yor;
    private void AIMove()
    {
        chessAI.MinMax(4, double.MinValue, double.MaxValue, false, true);
    }
    void SwitchPlayerColor()
    {
        // Oyuncu rengini de�i�tir
        currentPlayerColor = (currentPlayerColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
    }

    void SelectPiece(ChessPiece piece)
    {
        selectedPiece = piece;

        // Se�ili ta��n gidebilece�i kareleri a��k mavi renge �evir
        HighlightValidMoves(piece);

        // Ta��n bulundu�u kareyi bul
        ChessboardTile tile = FindTileAtPosition(piece.row, piece.col);

        if (tile != null)
        {
            // Kare rengini a��k sar� yap
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            tileRenderer.material.color = Color.yellow;
        }

    }

    void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            // Ta��n bulundu�u kareyi bul
            ChessboardTile tile = FindTileAtPosition(selectedPiece.row, selectedPiece.col);

            if (tile != null)
            {
                // Kare rengini as�l rengine d�nd�r
                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if ((selectedPiece.row + selectedPiece.col) % 2 == 0)
                {
                    // Siyah renk
                    tileRenderer.material.color = Color.black;
                }
                else
                {
                    // Beyaz renk
                    tileRenderer.material.color = Color.white;
                }
            }

            // Se�ili ta��n gidebilece�i kareleri as�l renklerine �evir
            ClearHighlightedMoves();

            selectedPiece = null;
        }
    }

    void HighlightValidMoves(ChessPiece piece)
    {
        // Se�ili ta��n se�ildi�i varsay�larak, ta��n gidebilece�i t�m kareleri a��k mavi renge �evir

    // Se�ili ta��n bulundu�u sat�r ve s�tunu al
    int currentRow = piece.row;
    int currentCol = piece.col;

    for (int row = 0; row < boardSize; row++)
    {
        for (int col = 0; col < boardSize; col++)
        {
            // Hedef pozisyonu ge�erli bir pozisyon mu diye kontrol et
            if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
            {
                // Ge�ersiz pozisyon, devam et
                continue;
            }

            // Se�ili ta��n hedef pozisyona hareket edebilip edemeyece�ini kontrol et
            if (piece.CanMoveToPosition(row, col) && !piece.WouldCauseCheck(row,col))
            {
                // Hedef pozisyonu dolu mu diye kontrol et
                GameObject targetPieceObject = FindPieceAtPosition(row, col);
                if (targetPieceObject != null)
                {
                    ChessPiece targetPiece = targetPieceObject.GetComponent<ChessPiece>();
                    if (targetPiece.color == piece.color)
                    {
                        // Hedef pozisyon kendi rengindeki bir ta� taraf�ndan i�gal ediliyor, renklendirme yapma
                        continue;
                    }
                }

                // Hedef pozisyon renklendirme i�lemleri
                ChessboardTile currentTile = FindTileAtPosition(currentRow, currentCol);
                ChessboardTile targetTile = FindTileAtPosition(row, col);

                if (currentTile != null && targetTile != null)
                {
                    Renderer targetTileRenderer = targetTile.GetComponent<Renderer>();

                    // Hedef pozisyonu farkl� renkte ta� taraf�ndan i�gal ediliyorsa k�rm�z� renk yap
                    if (targetPieceObject != null && targetPieceObject.GetComponent<ChessPiece>().color != piece.color)
                    {
                        targetTileRenderer.material.color = Color.red;
                    }
                    else
                    {
                        targetTileRenderer.material.color = Color.cyan; // Aksi takdirde a��k mavi renk yap
                    }
                }
            }
        }
    }
    }

    void ClearHighlightedMoves()
    {
        // Ta��n gidebilece�i kareleri as�l renklerine �evir

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                // Hedef pozisyonu ge�erli bir pozisyon mu diye kontrol et
                if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
                {
                    // Ge�ersiz pozisyon, devam et
                    continue;
                }

                // Tahtadaki her kareyi bul
                ChessboardTile tile = FindTileAtPosition(row, col);

                if (tile != null)
                {
                    // Kare rengini as�l rengine d�nd�r
                    Renderer tileRenderer = tile.GetComponent<Renderer>();
                    if ((row + col) % 2 == 0)
                    {
                        // Siyah renk
                        tileRenderer.material.color = Color.black;
                    }
                    else
                    {
                        // Beyaz renk
                        tileRenderer.material.color = Color.white;
                    }
                }
            }
        }
    }

    void MovePiece(ChessPiece piece, int targetRow, int targetCol)
    {
        // Hedef pozisyonu ge�erli bir pozisyon mu diye kontrol et
        if (targetRow < 0 || targetRow >= boardSize || targetCol < 0 || targetCol >= boardSize)
        {
            return; // Ge�ersiz pozisyon, ta� hareket edemez
        }

        // Se�ili ta��n bulundu�u kareyi ve hedef kareyi al
        ChessboardTile currentTile = FindTileAtPosition(piece.row, piece.col);
        ChessboardTile targetTile = FindTileAtPosition(targetRow, targetCol);

        if (currentTile == null || targetTile == null)
        {
            return; // Ge�ersiz ta� veya hedef kare, ta� hareket edemez
        }

        // Hedef pozisyondaki ta�� bul
        GameObject targetPieceObject = FindPieceAtPosition(targetRow, targetCol);

        if (targetPieceObject != null)
        {
            // Hedef pozisyonu dolumu?
            ChessPiece targetPiece = targetPieceObject.GetComponent<ChessPiece>();
            if (targetPiece.color == piece.color)
            {
                return; // Hedefteki ta� se�ili ta�la ayn� renkte, ta�� hareket ettirme
            }
            else
            {
                // Hedefteki ta� se�ili ta�la farkl� renkte, ta�� yemek (yok etmek)
                Destroy(targetPieceObject);
            }
        }

        // Ta�� hedef pozisyona hareket ettirme i�lemlerini ger�ekle�tirin
        piece.SetPosition(targetRow, targetCol);

        // Ta�� hedef pozisyona hareket ettir
        piece.transform.position = targetTile.transform.position;

        // Oyuncu rengini de�i�tir
        SwitchPlayerColor();

        // Ta��n bulundu�u karenin rengini as�l rengine d�nd�r
        Renderer currentTileRenderer = currentTile.GetComponent<Renderer>();
        if ((currentTile.row + currentTile.col) % 2 == 0)
        {
            // Siyah renk
            currentTileRenderer.material.color = Color.black;
        }
        else
        {
            // Beyaz renk
            currentTileRenderer.material.color = Color.white;
        }
        if (IsInCheck(currentPlayerColor))
        {
            if (CanEscapeCheck(currentPlayerColor))
                Debug.Log("Ka��� var");
            else
                Debug.Log("oyun bitti. Kazanan" + ((currentPlayerColor == PieceColor.White) ? " siyah" : " beyaz"));
        }
        if (!CanEscapeCheck(currentPlayerColor))
        {
            Debug.Log("oyun bitti. Kazanan" + ((currentPlayerColor == PieceColor.White) ? " siyah" : " beyaz"));
        }
        if (!CanEscapeCheck((currentPlayerColor == PieceColor.White) ? PieceColor.Black : PieceColor.White))
        {
            Debug.Log(((currentPlayerColor == PieceColor.White) ? PieceColor.Black : PieceColor.White) + " is can't move!");
        }


        ChessPiece[,] board = new ChessPiece[boardSize, boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                GameObject AIPiece = FindPieceAtPosition(i, j);
                if (AIPiece != null)
                {
                    board[i, j] = (ChessPiece)AIPiece.GetComponent<ChessPiece>().Clone();
                }
            }
        }
        gameObject.GetComponent<ChessAI>().SelectBestMove(board);

        chessAI = gameObject.GetComponent<ChessAI>();
        thread�al���yor = true;
        thread = new Thread(AIMove);
        thread.Start();
    }


    ////////////////////////////////////////////D�zenle///////////////////////////////////
    public bool IsInCheck(PieceColor playerColor)
    {
        // �ah ta��n�n konumunu bul
        ChessPiece kingPiece = FindKingPiece(playerColor);

        if (kingPiece == null)
        {
            return false; // �ah ta�� bulunamad�, �ah durumu yok
        }

        // T�m rakip ta�lar� dola�arak �ah ta��n� tehdit ediyor mu kontrol et
        ChessPiece[] opponentPieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in opponentPieces)
        {
            if (piece.color != playerColor)
            {
                // Rakip ta��n hareket edebilece�i t�m pozisyonlar� kontrol et
                for (int row = 0; row < boardSize; row++)
                {
                    for (int col = 0; col < boardSize; col++)
                    {
                        if (piece.CanMoveToPosition(row, col))
                        {
                            // Rakip ta�, �ah ta��n�n bulundu�u pozisyonu tehdit ediyor mu?
                            if (row == kingPiece.row && col == kingPiece.col)
                            {
                                return true; // �ah durumu, oyuncu �ah alt�nda
                            }
                        }
                    }
                }
            }
        }

        return false; // �ah durumu yok, oyuncu g�vende
    }
    public bool CanEscapeCheck(PieceColor playerColor)
    {
        ChessPiece[] playerPieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in playerPieces)
        {
            if (piece.color == playerColor)
            {
                for (int row = 0; row < boardSize; row++)
                {
                    for (int col = 0; col < boardSize; col++)
                    {
                        if (piece.CanMoveToPosition(row, col) && !piece.WouldCauseCheck(row, col))
                        {
                            return true; // En az bir hamle ile �ah durumundan ��kabilir
                        }
                    }
                }
            }
        }

        return false; // Hi�bir hamle ile �ah durumundan ��kamaz
    }

    ChessPiece FindKingPiece(PieceColor playerColor)
    {
        ChessPiece[] pieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in pieces)
        {
            if (piece.color == playerColor && (piece.pieceType == PieceType.King_Black || piece.pieceType == PieceType.King_White))
            {
                return piece; // �ah ta�� bulundu
            }
        }

        return null; // �ah ta�� bulunamad�
    }

    void GenerateChessboard()
    {
        Vector3 startPos = new Vector3(0f, 0f, 0f);

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                // Her bir kare i�in pozisyon hesaplama
                float x = startPos.x + col * tileSize;
                float z = startPos.z + row * tileSize;
                Vector3 tilePosition = new Vector3(x, 0f, z);

                // Kare objesini olu�turma
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);

                // Karelerin renklerini ayarlama
                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if ((row + col) % 2 == 0)
                {
                    // Siyah renk
                    tileRenderer.material.color = Color.black;
                }
                else
                {
                    // Beyaz renk
                    tileRenderer.material.color = Color.white;
                }

                // Kare konum �zelli�i ekleme
                ChessboardTile chessboardTile = tile.AddComponent<ChessboardTile>();
                chessboardTile.SetPosition(row, col);

                // Kare alt�nda yer alan TextMeshPro bile�enini d�nd�rme
                TextMeshPro textMesh = tile.GetComponentInChildren<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }
        }
    }

    void PlaceChessPieces()
    {
        // Siyah ta�lar
        PlacePiece(PieceType.Rook_Black, 7, 0, PieceColor.Black); // Kale
        PlacePiece(PieceType.Knight_Black, 7, 1, PieceColor.Black); // At
        PlacePiece(PieceType.Bishop_Black, 7, 2, PieceColor.Black); // Fil
        PlacePiece(PieceType.Queen_Black, 7, 3, PieceColor.Black); // Vezir
        PlacePiece(PieceType.King_Black, 7, 4, PieceColor.Black); // �ah
        PlacePiece(PieceType.Bishop_Black, 7, 5, PieceColor.Black); // Fil
        PlacePiece(PieceType.Knight_Black, 7, 6, PieceColor.Black); // At
        PlacePiece(PieceType.Rook_Black, 7, 7, PieceColor.Black); // Kale

        // Beyaz ta�lar
        PlacePiece(PieceType.Rook_White, 0, 0, PieceColor.White); // Kale
        PlacePiece(PieceType.Knight_White, 0, 1, PieceColor.White); // At
        PlacePiece(PieceType.Bishop_White, 0, 2, PieceColor.White); // Fil
        PlacePiece(PieceType.Queen_White, 0, 3, PieceColor.White); // Vezir
        PlacePiece(PieceType.King_White, 0, 4, PieceColor.White); // �ah
        PlacePiece(PieceType.Bishop_White, 0, 5, PieceColor.White); // Fil
        PlacePiece(PieceType.Knight_White, 0, 6, PieceColor.White); // At
        PlacePiece(PieceType.Rook_White, 0, 7, PieceColor.White); // Kale

        // Beyaz piyonlar
        for (int col = 0; col < boardSize; col++)
        {
            PlacePiece(PieceType.Pawn_White, 1, col, PieceColor.White); // Piyon
        }

        // Siyah piyonlar
        for (int col = 0; col < boardSize; col++)
        {
            PlacePiece(PieceType.Pawn_Black, 6, col, PieceColor.Black); // Piyon
        }
    }

    void PlacePiece(PieceType pieceType, int row, int col, PieceColor pieceColor)
    {
        GameObject piecePrefab = GetPiecePrefab(pieceType);
        if (piecePrefab == null)
        {
            Debug.LogError("Invalid piece type: " + pieceType);
            return;
        }

        Vector3 piecePosition = CalculatePiecePosition(row, col);
        GameObject chessPiece = Instantiate(piecePrefab, piecePosition, Quaternion.identity);
        chessPiece.transform.SetParent(transform);

        ChessPiece piece = chessPiece.GetComponent<ChessPiece>();
        piece.SetPosition(row, col);
        piece.SetColor(pieceColor);

        Renderer pieceRenderer = chessPiece.GetComponent<Renderer>();

        if (pieceColor == PieceColor.White)
        {
            pieceRenderer.material.color = Color.white;
        }

        if (pieceColor == PieceColor.Black)
        {
            pieceRenderer.material.color = Color.black;
        }
    }

    GameObject GetPiecePrefab(PieceType pieceType)
    {
        foreach (GameObject piecePrefab in piecePrefabs)
        {
            if (piecePrefab.GetComponent<ChessPiece>().pieceType == pieceType)
            {
                return piecePrefab;
            }
        }
        return null;
    }

    Vector3 CalculatePiecePosition(int row, int col)
    {
        float x = col * tileSize;
        float z = row * tileSize;
        return new Vector3(x, 0f, z);
    }

    ChessboardTile FindTileAtPosition(int row, int col)
    {
        ChessboardTile[] chessboardTiles = FindObjectsOfType<ChessboardTile>();

        foreach (ChessboardTile tile in chessboardTiles)
        {
            if (tile.row == row && tile.col == col)
            {
                // �stenen sat�r ve s�tundaki kare bulundu
                return tile;
            }
        }

        // �stenen sat�r ve s�tunda kare bulunamad�
        return null;
    }

    public GameObject FindPieceAtPosition(int row, int col)
    {
        if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
        {
            // Ge�ersiz pozisyon, ta� yok
            return null;
        }

        ChessPiece[] chessPieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in chessPieces)
        {
            if (piece.row == row && piece.col == col)
            {
                // �stenen sat�r ve s�tundaki ta� bulundu
                return piece.gameObject;
            }
        }

        // �stenen sat�r ve s�tunda ta� bulunamad�
        return null;
    }
}