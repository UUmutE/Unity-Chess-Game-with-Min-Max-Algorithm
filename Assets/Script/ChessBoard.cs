using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    public GameObject tilePrefab; // Kullanýlacak kare objesi prefabý
    public GameObject[] piecePrefabs; // Kullanýlacak taþ objesi prefablarý
    public int boardSize = 8; // Tahta boyutu (8x8)
    
    private float tileSize = 10f; // Kare boyutu
    private PieceColor currentPlayerColor = PieceColor.White; // Baþlangýçta beyaz oyuncuyla baþlayalým

    private ChessPiece selectedPiece;// Seçilen taþ

    void Start()
    {
        GenerateChessboard();
        PlaceChessPieces();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Fare týklamasýný kontrol et

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Týklanan objeyi kontrol et

                ChessPiece clickedPiece = hit.transform.GetComponent<ChessPiece>();
                ChessboardTile clickedTile = hit.transform.GetComponent<ChessboardTile>();

                if (clickedPiece != null && clickedPiece.color == currentPlayerColor && currentPlayerColor == PieceColor.White)
                {
                    // Týklanan obje bir taþ ve sýrasý oyuncunun rengine ait

                    if (selectedPiece != null)
                    {
                        // Önceden seçilmiþ bir taþ varsa, seçimini iptal et
                        DeselectPiece();
                    }

                    // Taþý seç
                    SelectPiece(clickedPiece);
                }
                else if (clickedTile != null && selectedPiece != null)
                {
                    // Týklanan obje bir kare ve bir taþ seçilmiþ

                    int targetRow = clickedTile.row;
                    int targetCol = clickedTile.col;

                    if (selectedPiece.CanMoveToPosition(targetRow, targetCol) && !selectedPiece.WouldCauseCheck(targetRow, targetCol))
                    {
                        ChessboardTile tile = FindTileAtPosition(selectedPiece.row, selectedPiece.col);

                        if (tile != null)
                        {
                            // Kare rengini asýl rengine döndür
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
                        // Taþý hedef pozisyona hareket ettir
                        MovePiece(selectedPiece, targetRow, targetCol);
                    }

                    // Taþýn seçimini iptal et
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
            threadÇalýþýyor = true;
            thread = new Thread(AIMove);
            thread.Start();
        }
        if (threadÇalýþýyor && thread.ThreadState == ThreadState.Stopped)
        {
            Debug.Log("asd");
            Hareket hamle = chessAI.hareket;
            ChessPiece Ai = FindPieceAtPosition(hamle.SourceRow, hamle.SourceColumn).GetComponent<ChessPiece>();

            MovePiece(Ai, hamle.TargetRow, hamle.TargetColumn);
            threadÇalýþýyor = false;
        }
    }
    Thread thread;
    ChessAI chessAI;
    bool threadÇalýþýyor;
    private void AIMove()
    {
        chessAI.MinMax(4, double.MinValue, double.MaxValue, false, true);
    }
    void SwitchPlayerColor()
    {
        // Oyuncu rengini deðiþtir
        currentPlayerColor = (currentPlayerColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
    }

    void SelectPiece(ChessPiece piece)
    {
        selectedPiece = piece;

        // Seçili taþýn gidebileceði kareleri açýk mavi renge çevir
        HighlightValidMoves(piece);

        // Taþýn bulunduðu kareyi bul
        ChessboardTile tile = FindTileAtPosition(piece.row, piece.col);

        if (tile != null)
        {
            // Kare rengini açýk sarý yap
            Renderer tileRenderer = tile.GetComponent<Renderer>();
            tileRenderer.material.color = Color.yellow;
        }

    }

    void DeselectPiece()
    {
        if (selectedPiece != null)
        {
            // Taþýn bulunduðu kareyi bul
            ChessboardTile tile = FindTileAtPosition(selectedPiece.row, selectedPiece.col);

            if (tile != null)
            {
                // Kare rengini asýl rengine döndür
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

            // Seçili taþýn gidebileceði kareleri asýl renklerine çevir
            ClearHighlightedMoves();

            selectedPiece = null;
        }
    }

    void HighlightValidMoves(ChessPiece piece)
    {
        // Seçili taþýn seçildiði varsayýlarak, taþýn gidebileceði tüm kareleri açýk mavi renge çevir

    // Seçili taþýn bulunduðu satýr ve sütunu al
    int currentRow = piece.row;
    int currentCol = piece.col;

    for (int row = 0; row < boardSize; row++)
    {
        for (int col = 0; col < boardSize; col++)
        {
            // Hedef pozisyonu geçerli bir pozisyon mu diye kontrol et
            if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
            {
                // Geçersiz pozisyon, devam et
                continue;
            }

            // Seçili taþýn hedef pozisyona hareket edebilip edemeyeceðini kontrol et
            if (piece.CanMoveToPosition(row, col) && !piece.WouldCauseCheck(row,col))
            {
                // Hedef pozisyonu dolu mu diye kontrol et
                GameObject targetPieceObject = FindPieceAtPosition(row, col);
                if (targetPieceObject != null)
                {
                    ChessPiece targetPiece = targetPieceObject.GetComponent<ChessPiece>();
                    if (targetPiece.color == piece.color)
                    {
                        // Hedef pozisyon kendi rengindeki bir taþ tarafýndan iþgal ediliyor, renklendirme yapma
                        continue;
                    }
                }

                // Hedef pozisyon renklendirme iþlemleri
                ChessboardTile currentTile = FindTileAtPosition(currentRow, currentCol);
                ChessboardTile targetTile = FindTileAtPosition(row, col);

                if (currentTile != null && targetTile != null)
                {
                    Renderer targetTileRenderer = targetTile.GetComponent<Renderer>();

                    // Hedef pozisyonu farklý renkte taþ tarafýndan iþgal ediliyorsa kýrmýzý renk yap
                    if (targetPieceObject != null && targetPieceObject.GetComponent<ChessPiece>().color != piece.color)
                    {
                        targetTileRenderer.material.color = Color.red;
                    }
                    else
                    {
                        targetTileRenderer.material.color = Color.cyan; // Aksi takdirde açýk mavi renk yap
                    }
                }
            }
        }
    }
    }

    void ClearHighlightedMoves()
    {
        // Taþýn gidebileceði kareleri asýl renklerine çevir

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                // Hedef pozisyonu geçerli bir pozisyon mu diye kontrol et
                if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
                {
                    // Geçersiz pozisyon, devam et
                    continue;
                }

                // Tahtadaki her kareyi bul
                ChessboardTile tile = FindTileAtPosition(row, col);

                if (tile != null)
                {
                    // Kare rengini asýl rengine döndür
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
        // Hedef pozisyonu geçerli bir pozisyon mu diye kontrol et
        if (targetRow < 0 || targetRow >= boardSize || targetCol < 0 || targetCol >= boardSize)
        {
            return; // Geçersiz pozisyon, taþ hareket edemez
        }

        // Seçili taþýn bulunduðu kareyi ve hedef kareyi al
        ChessboardTile currentTile = FindTileAtPosition(piece.row, piece.col);
        ChessboardTile targetTile = FindTileAtPosition(targetRow, targetCol);

        if (currentTile == null || targetTile == null)
        {
            return; // Geçersiz taþ veya hedef kare, taþ hareket edemez
        }

        // Hedef pozisyondaki taþý bul
        GameObject targetPieceObject = FindPieceAtPosition(targetRow, targetCol);

        if (targetPieceObject != null)
        {
            // Hedef pozisyonu dolumu?
            ChessPiece targetPiece = targetPieceObject.GetComponent<ChessPiece>();
            if (targetPiece.color == piece.color)
            {
                return; // Hedefteki taþ seçili taþla ayný renkte, taþý hareket ettirme
            }
            else
            {
                // Hedefteki taþ seçili taþla farklý renkte, taþý yemek (yok etmek)
                Destroy(targetPieceObject);
            }
        }

        // Taþý hedef pozisyona hareket ettirme iþlemlerini gerçekleþtirin
        piece.SetPosition(targetRow, targetCol);

        // Taþý hedef pozisyona hareket ettir
        piece.transform.position = targetTile.transform.position;

        // Oyuncu rengini deðiþtir
        SwitchPlayerColor();

        // Taþýn bulunduðu karenin rengini asýl rengine döndür
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
                Debug.Log("Kaçýþ var");
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
        threadÇalýþýyor = true;
        thread = new Thread(AIMove);
        thread.Start();
    }


    ////////////////////////////////////////////Düzenle///////////////////////////////////
    public bool IsInCheck(PieceColor playerColor)
    {
        // Þah taþýnýn konumunu bul
        ChessPiece kingPiece = FindKingPiece(playerColor);

        if (kingPiece == null)
        {
            return false; // Þah taþý bulunamadý, þah durumu yok
        }

        // Tüm rakip taþlarý dolaþarak þah taþýný tehdit ediyor mu kontrol et
        ChessPiece[] opponentPieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in opponentPieces)
        {
            if (piece.color != playerColor)
            {
                // Rakip taþýn hareket edebileceði tüm pozisyonlarý kontrol et
                for (int row = 0; row < boardSize; row++)
                {
                    for (int col = 0; col < boardSize; col++)
                    {
                        if (piece.CanMoveToPosition(row, col))
                        {
                            // Rakip taþ, þah taþýnýn bulunduðu pozisyonu tehdit ediyor mu?
                            if (row == kingPiece.row && col == kingPiece.col)
                            {
                                return true; // Þah durumu, oyuncu þah altýnda
                            }
                        }
                    }
                }
            }
        }

        return false; // Þah durumu yok, oyuncu güvende
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
                            return true; // En az bir hamle ile þah durumundan çýkabilir
                        }
                    }
                }
            }
        }

        return false; // Hiçbir hamle ile þah durumundan çýkamaz
    }

    ChessPiece FindKingPiece(PieceColor playerColor)
    {
        ChessPiece[] pieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in pieces)
        {
            if (piece.color == playerColor && (piece.pieceType == PieceType.King_Black || piece.pieceType == PieceType.King_White))
            {
                return piece; // Þah taþý bulundu
            }
        }

        return null; // Þah taþý bulunamadý
    }

    void GenerateChessboard()
    {
        Vector3 startPos = new Vector3(0f, 0f, 0f);

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                // Her bir kare için pozisyon hesaplama
                float x = startPos.x + col * tileSize;
                float z = startPos.z + row * tileSize;
                Vector3 tilePosition = new Vector3(x, 0f, z);

                // Kare objesini oluþturma
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

                // Kare konum özelliði ekleme
                ChessboardTile chessboardTile = tile.AddComponent<ChessboardTile>();
                chessboardTile.SetPosition(row, col);

                // Kare altýnda yer alan TextMeshPro bileþenini döndürme
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
        // Siyah taþlar
        PlacePiece(PieceType.Rook_Black, 7, 0, PieceColor.Black); // Kale
        PlacePiece(PieceType.Knight_Black, 7, 1, PieceColor.Black); // At
        PlacePiece(PieceType.Bishop_Black, 7, 2, PieceColor.Black); // Fil
        PlacePiece(PieceType.Queen_Black, 7, 3, PieceColor.Black); // Vezir
        PlacePiece(PieceType.King_Black, 7, 4, PieceColor.Black); // Þah
        PlacePiece(PieceType.Bishop_Black, 7, 5, PieceColor.Black); // Fil
        PlacePiece(PieceType.Knight_Black, 7, 6, PieceColor.Black); // At
        PlacePiece(PieceType.Rook_Black, 7, 7, PieceColor.Black); // Kale

        // Beyaz taþlar
        PlacePiece(PieceType.Rook_White, 0, 0, PieceColor.White); // Kale
        PlacePiece(PieceType.Knight_White, 0, 1, PieceColor.White); // At
        PlacePiece(PieceType.Bishop_White, 0, 2, PieceColor.White); // Fil
        PlacePiece(PieceType.Queen_White, 0, 3, PieceColor.White); // Vezir
        PlacePiece(PieceType.King_White, 0, 4, PieceColor.White); // Þah
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
                // Ýstenen satýr ve sütundaki kare bulundu
                return tile;
            }
        }

        // Ýstenen satýr ve sütunda kare bulunamadý
        return null;
    }

    public GameObject FindPieceAtPosition(int row, int col)
    {
        if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
        {
            // Geçersiz pozisyon, taþ yok
            return null;
        }

        ChessPiece[] chessPieces = FindObjectsOfType<ChessPiece>();

        foreach (ChessPiece piece in chessPieces)
        {
            if (piece.row == row && piece.col == col)
            {
                // Ýstenen satýr ve sütundaki taþ bulundu
                return piece.gameObject;
            }
        }

        // Ýstenen satýr ve sütunda taþ bulunamadý
        return null;
    }
}