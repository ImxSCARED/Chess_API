using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}
public class Chessboard : MonoBehaviour
{


    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1.5f;
    [SerializeField] private GameObject victoryScreen;
    [Header("Prefabs && materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();


    // STAN LAND

    //Stan's Camera stuff
    public CameraMovements camScript;

    //Stan's Enemy stuff
    

    // counters
    public int turnNumber = 1;
    public int greyturnCounter = 0;
    public int enemyturnCounter = 0;
    public int killCounter = 0;
    public int killCounterMax = 7;
    
    // designer edited
    public int greyEveryXTurns = 4;
    public int EnemyEveryXTurns = 2;

    //Coin stuff
    public GameObject Coin;
    public Vector3 coinSpawnLocation;
    public Vector3 range;
    public Quaternion spawnRotation;
    public Vector3 angularVelocity;



    /// //////////////
    /// ////////////////////////////////Seperation space ///////////////////////////////////////////////////////////////////
    /// //////////////
    private void Awake()
    {
        isWhiteTurn = true;
        //Finding camera and assigning script to camScript
        GameObject camera = GameObject.Find("Main Camera");
        camScript = camera.GetComponent<CameraMovements>();

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        //SpawnElderTeam();
        PositionAllpieces();
    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
             //Get the indexes of tile we hit
             Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

             //If we are hovering any tile after not hovering any tile
             if (currentHover == -Vector2Int.one)
                {
                    currentHover = hitPosition;
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }

             //if we were already hovernig a tile, change previous
             if (currentHover != hitPosition)
                {
                    tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                    currentHover = hitPosition;
                    tiles[currentHover.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }

             //if we press down left click on mouse
             if(Input.GetMouseButtonDown(0))
             {
                 if (chessPieces[hitPosition.x,hitPosition.y] != null)
                 {
                     //Is it our turn? 
                     if((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                     {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        // Get a list of where i can go, Highlight titles as well
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                        // Get List of special moves as well
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);


                        PreventCheck();
                        HighlightTiles();

                     }
                 }
             }

             //if we are releasing left click on mouse
             if (currentlyDragging != null && Input.GetMouseButtonUp(0))
             {
                 Vector2Int previousPositon = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                 
                 bool validMove = Moveto(currentlyDragging, hitPosition.x, hitPosition.y);
                 if (!validMove)
                     currentlyDragging.SetPosition(GetTileCenter(previousPositon.x, previousPositon.y));     
                 
                 currentlyDragging = null;
                 RemoveHighlightTiles();

             }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        // If we're dragging a piece
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            CreateGreyMind();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            //CreateRedDog();
            SpawnCoin();
        }

        if(greyturnCounter >= greyEveryXTurns && greyAlive == false)
        {
            CreateGreyMind();
            greyturnCounter = 0;
        }
    }


    // Generate the Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {

        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;
        


       tiles =new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;


        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset,  (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };


        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();


        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        

        return tileObject;


    }

    //Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteteam = 0, blackteam = 1;

        //White Team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteteam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteteam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteteam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteteam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteteam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteteam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteteam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteteam);


        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteteam);
        }

        //Black Team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackteam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackteam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackteam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackteam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackteam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackteam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackteam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackteam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackteam);
        }

        
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        Material tempMat = teamMaterials[((team == 0) ? 0 : 6) + ((int)type - 1)];
        cp.GetComponent<MeshRenderer>().material = tempMat;
        cp.originalMaterial = tempMat;
        return cp; 

    }
    private ChessPiece SpawnSingleElder(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
       
        return cp;

    }

    // Positioning
    private void PositionAllpieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);

    }
    private void PositionSinglePiece(int x, int y,bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x,y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }


    // Checkmate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    public void OnResetButton()
    {
        //UI
        victoryScreen.transform.GetChild(2).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        // Fields reset
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();

        // Clean up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    Destroy(chessPieces[x, y].gameObject);

                chessPieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllpieces();
        isWhiteTurn = true;

    }
    public void OnExit()
    {
        Application.Quit();
    }

    //Special Moves
    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if(myPawn.currentX == enemyPawn.currentX)
            {
                if(myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if(enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(
                            new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(
                            new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.back * deathSpacing) * deadBlacks.Count);
                    }
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if(targetPawn.type == ChessPieceType.Pawn)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            //Left Rook
            if (lastMove[1].x == 2)
            {
                if(lastMove[1].y == 0) // White Side
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) // Black Side
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            // Right Rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) // White Side
                    {
                        ChessPiece rook = chessPieces[7, 0];
                        chessPieces[5, 0] = rook;
                        PositionSinglePiece(5, 0);
                        chessPieces[7, 0] = null;
                    }
                    else if (lastMove[1].y == 7) // Black Side
                    {
                        ChessPiece rook = chessPieces[7, 7];
                        chessPieces[5, 7] = rook;
                        PositionSinglePiece(5, 7);
                        chessPieces[7, 7] = null;
                    }
            }
        }    
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    if (chessPieces[x, y].type == ChessPieceType.King)
                        if (chessPieces[x, y].team == currentlyDragging.team)
                            targetKing = chessPieces[x, y];


        //Since we're sending ref availableMoves, we will be deleting moves that are putting us in check
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }
    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves,ChessPiece targetKing)
    {
        // Save the current values, to reset after the function call
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and check if we're in check
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            //Did we Simulate the King's Move
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);

            // copy the [,] and not a reference
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (chessPieces[x,y] !=null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                            simAttackingPieces.Add(simulation[x, y]);

                    }
                }
            }

            //Simulate that move
            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            //Did one of the pieces get taken down during our simulation
            var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
                simAttackingPieces.Remove(deadPiece);

            // Get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }

            //Is the King in trouble? if so, remove the move
            if(ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual CP DATA
            cp.currentX = actualX;
            cp.currentY = actualY;
        }

        //Remove from current available move List
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }
    private int CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                            targetKing = chessPieces[x, y];
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }

        //Is the King under attacked
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int b = 0; b < pieceMoves.Count; b++)
                currentAvailableMoves.Add(pieceMoves[b]);
        }

        //Are we in check right now?
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            //King is under attack,can we move something to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (defendingMoves.Count != 0)
                    return 0;
            }
            return 1;//CheckMate Exit
        }
        else
        {
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (defendingMoves.Count != 0)
                    return 0;
            }
            return 2; //staleMate Exit
        }
    }


    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
        
    }
    private bool Moveto(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //is there another piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
                return false;

            //if it's enemy team
            if (ocp.team == 0)
            {

                if (ocp.type == ChessPieceType.King)
                    CheckMate(1);

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else if(ocp.team == 1)
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(0);

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count);

            }
            else
            {
                if (ocp.transform.CompareTag("GreyMind"))
                {
                    DestroyGreyMind();
                    Destroy(ocp.gameObject);
                }
                if (ocp.transform.CompareTag("RedDog") || ocp.transform.CompareTag("YellowMind"))
                {
                    Destroy(ocp.gameObject);
                }
            } // this is the grey mind kill scirpt
        }
        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;

        // STAN:  Turn Counter

        turnNumber++;
        greyturnCounter++;
        if(killCounter >= killCounterMax)
        {
            SceneManager.LoadScene("Victory Screen");
        }
        Debug.Log(greyturnCounter);

        //Debug.Log("Turn Number: " + turnNumber);
        

        // 
        if(isWhiteTurn==true)
        {
            Debug.Log("White Turn");
        }
        else
        {
            Debug.Log("Black Turn");
            RedMoves();
        }
        
        
        // // // 
        

        moveList.Add(new Vector2Int[] {previousPosition, new Vector2Int(x,y)});

        ProcessSpecialMove();
        switch(CheckForCheckmate())
        {
            default:
                break;
            case 1:
                CheckMate(cp.team);
                break;
            case 2:
                CheckMate(2);
                break;
            }

        camScript.SwitchSides();
        return true;
        

    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one;
                    
    }


    // Stan's Enemy spawning script - vars for the Spawn GreyMind function.
    public bool pawnInvert = false;
    public bool lockRook = false;
    public bool lockBishop = false;
    public bool lockKnight = false;
    public bool eldrichBoard = false;
    public bool spawnEnemy = false;
    // 
    public bool greyAlive = false;
    private bool CreateGreyMind()
    {
        int attempts = 0;
        int maxAttempts = 100; // Set a limit to avoid potential infinite loops
        bool placedSuccessfully = false;
        ///
        Debug.Log("i have arrived Mortal fleash");

        int randomNumber = UnityEngine.Random.Range(0, 2);

        switch (randomNumber)
        {
            case 0:
                Debug.Log("Random number is 0. Running code A. (pawn switch)");
                // Run code A
                pawnInvert = true;
                foreach (ChessPiece currentP in chessPieces)
                {
                    if (currentP != null)
                    {
                        if (currentP.type == ChessPieceType.Pawn)
                        {
                            currentP.GetComponent<Pawn>().pawnBackwardMove = true;
                        }
                    }
                }
                break;
            case 1:
                Debug.Log("Random number is 1. Running code B.(locking rook, bishops or knights switch)");
                // Run code B
                int randomFreeze = UnityEngine.Random.Range(0, 3);
                switch (randomFreeze)
                {
                    case 0:
                        {
                            //run rook
                            lockRook = true;
                            foreach (ChessPiece currentP in chessPieces)
                            {
                                if (currentP != null)
                                {
                                    if (currentP.type == ChessPieceType.Rook)
                                    {
                                        currentP.FreezePiece();
                                    }                                    
                                }
                            }
                        }
                        break;
                    case 1:
                        {
                            //run bishop
                            lockBishop = true;
                            foreach (ChessPiece currentP in chessPieces)
                            {
                                if (currentP != null)
                                {
                                    if (currentP.type == ChessPieceType.Bishop)
                                    {
                                        currentP.FreezePiece();
                                    }
                                }
                            }
                        }
                        break;
                    case 2:                        
                        {
                            //run knight
                            lockKnight = true;
                            foreach (ChessPiece currentP in chessPieces)
                            {
                                if (currentP != null)
                                {
                                    if (currentP.type == ChessPieceType.Knight)
                                    {
                                        currentP.FreezePiece();
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        Debug.LogError("Unexpected random freeze number!");
                        break;
                }
                break;
            case 2:
                Debug.Log("Random number is 2. Running code C. (visual effect on board) ");
                // Run code C
                eldrichBoard = true;

                break;
            case 3:
                Debug.Log("Random number is 3. Running code D. (spawn en enemy unit");
                // Run code C
                spawnEnemy = true;
                break;
            default:
                Debug.LogError("Unexpected random number!");
                break;
        }
        ///
        while (!placedSuccessfully && attempts < maxAttempts)
        {
            attempts++;
            int randomGreyX = UnityEngine.Random.Range(0, 8);
            int randomGreyY = UnityEngine.Random.Range(2, 6);
            int eteam = 2;
            ChessPiece greyMind = SpawnSingleElder(ChessPieceType.GreyMind, eteam);
            ////

            ////
            if (chessPieces[randomGreyX, randomGreyY] == null)
            {
                chessPieces[randomGreyX, randomGreyY] = greyMind;
                PositionSinglePiece(randomGreyX, randomGreyY, true);
                foreach (ChessPiece currentP in chessPieces)
                {
                    if (currentP != null)
                    {
                        if (currentP.type == ChessPieceType.Pawn || currentP.type == ChessPieceType.Bishop || currentP.type == ChessPieceType.Knight || currentP.type == ChessPieceType.Rook)
                        {
                            currentP.greyScript = greyMind.GetComponent<GreyMind>();
                            
                        }
                    }
                }
                placedSuccessfully = true;
                greyAlive = true;
                Debug.Log("grey is Alive bool is set to" + greyAlive);

            }
            else
            {
                ChessPiece ocp = chessPieces[randomGreyX, randomGreyY];

                if (greyMind.team == ocp.team)
                {
                    // If the same team, retry
                    continue;
                }

                // If it's the enemy team
                if (ocp.team == 0)
                {
                    if (ocp.type == ChessPieceType.King)
                        CheckMate(1);

                    deadWhites.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    ocp.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                        - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.forward * deathSpacing) * deadWhites.Count);
                }
                else if (ocp.team == 1)
                {
                    if (ocp.type == ChessPieceType.King)
                        CheckMate(0);

                    deadBlacks.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    ocp.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                        - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.back * deathSpacing) * deadBlacks.Count);
                }

                chessPieces[randomGreyX, randomGreyY] = greyMind;
                PositionSinglePiece(randomGreyX, randomGreyY, true);
                foreach (ChessPiece currentP in chessPieces)
                {
                    if (currentP != null)
                    {
                        if (currentP.type == ChessPieceType.Pawn || currentP.type == ChessPieceType.Bishop || currentP.type == ChessPieceType.Knight || currentP.type == ChessPieceType.Rook)
                        {
                            currentP.greyScript = greyMind.GetComponent<GreyMind>();
                            
                        }
                    }
                }
                placedSuccessfully = true;
                greyAlive = true;
                Debug.Log("grey is Alive bool is set to" + greyAlive);
            }
        }

        return placedSuccessfully;
        

        

    }

    private void DestroyGreyMind()
    {
        greyturnCounter = 0;
        killCounter++;
        greyAlive = false;
        SpawnCoin();
        Debug.Log("grey is Alive bool is set to" + greyAlive);
        //If you make a tag for bishop, and a tag for rook and whatever, do search by tag and call its unfreeze function, instead of this stupid search
        if (pawnInvert)
        {
            pawnInvert = false;
            foreach (ChessPiece currentP in chessPieces)
            {
                if (currentP != null)
                {
                    if (currentP.type == ChessPieceType.Pawn)
                    {
                        currentP.GetComponent<Pawn>().pawnBackwardMove = false;
                    }
                }
            }
        }
        if(lockBishop)
        {
            lockBishop = false;
            foreach (ChessPiece currentP in chessPieces)
            {
                if (currentP != null)
                {
                    if (currentP.type == ChessPieceType.Bishop)
                    {
                        currentP.UnFreezePiece();
                    }
                }
            }
        }
        if(lockKnight)
        {
            lockKnight = false;
            foreach (ChessPiece currentP in chessPieces)
            {
                if (currentP != null)
                {
                    if (currentP.type == ChessPieceType.Knight)
                    {
                        currentP.UnFreezePiece();
                    }
                }
            }
        }
        if(lockRook)
        {
            lockRook = false;
            foreach (ChessPiece currentP in chessPieces)
            {
                if (currentP != null)
                {
                    if (currentP.type == ChessPieceType.Rook)
                    {
                        currentP.UnFreezePiece();
                    }
                }
            }
        }
    }
    private bool CreateRedDog()
    {
        int attempts = 0;
        int maxAttempts = 100; // Set a limit to avoid potential infinite loops
        bool placedSuccessfully = false;

        while (!placedSuccessfully && attempts < maxAttempts)
        {
            attempts++;
            int randomRedX = UnityEngine.Random.Range(0, 8);
            int randomRedY = UnityEngine.Random.Range(2, 6);
            int eteam = 2;
            ChessPiece redDog = SpawnSingleElder(ChessPieceType.RedDog, eteam);

            if (chessPieces[randomRedX, randomRedY] == null)
            {
                chessPieces[randomRedX, randomRedY] = redDog;
                PositionSinglePiece(randomRedX, randomRedY, true);
                placedSuccessfully = true;
            }
            else
            {
                ChessPiece ocp = chessPieces[randomRedX, randomRedY];

                if (redDog.team == ocp.team)
                {
                    // If the same team, retry
                    continue;
                }

                // If it's the enemy team
                if (ocp.team == 0)
                {
                    if (ocp.type == ChessPieceType.King)
                        CheckMate(1);

                    deadWhites.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    ocp.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                        - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.forward * deathSpacing) * deadWhites.Count);
                }
                else if (ocp.team == 1)
                {
                    if (ocp.type == ChessPieceType.King)
                        CheckMate(0);

                    deadBlacks.Add(ocp);
                    ocp.SetScale(Vector3.one * deathSize);
                    ocp.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                        - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.back * deathSpacing) * deadBlacks.Count);
                }

                chessPieces[randomRedX, randomRedY] = redDog;
                PositionSinglePiece(randomRedX, randomRedY, true);
                placedSuccessfully = true;
            }
        }

        return placedSuccessfully;
    }
 
    private void RedMoves()
    {
       //Debug.Log("first part of REDMoves");
       foreach (ChessPiece thePiece in chessPieces)
        {
           // Debug.Log("second part of REDMoves");
            if (thePiece is RedDog red)
            {
                // add move code here
               // Debug.Log("Red Moved");
            }
        }
    }

    private void SpawnCoin()
    {
        Vector3 randomPosition = new Vector3(
                Random.Range(coinSpawnLocation.x - range.x, coinSpawnLocation.x + range.x),
                Random.Range(coinSpawnLocation.y - range.y, coinSpawnLocation.y + range.y),
                Random.Range(coinSpawnLocation.z - range.z, coinSpawnLocation.z + range.z)
            );
        GameObject InstantCoin = Instantiate(Coin, randomPosition, spawnRotation);
        Rigidbody rb = InstantCoin.GetComponent<Rigidbody>();
        rb.angularVelocity = angularVelocity;
    }
} 