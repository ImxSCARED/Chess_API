using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Collections;


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
    private ChessPiece currentDog;
    private ChessPiece dogTarget;
    private ChessPiece SecondDog;
    private ChessPiece dogTarget2;

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

    [Header("Counters")]
    // counters
    public int turnNumber = 1;
    public int greyturnCounter = 0;
    public int redturnCounter = 0;
    public int killCounter = 0;
    public int killCounterMax = 7;
    
    // designer edited
    public int greyEveryXTurns = 4;
    public int redEveryXTurns = 3;

    [Header("Coin Related")]
    //Coin stuff
    public GameObject Coin;
    public Vector3 coinSpawnLocation;
    public Vector3 range;
    public Quaternion spawnRotation;
    public Vector3 angularVelocity;


    [Header("Sound Effects")]
    // Array to hold sound effects
    public AudioClip[] greySpawningSounds;
    public AudioClip[] greyDeathSounds;
    public AudioClip[] greyFreezeSounds;
    public AudioClip[] fireDeathSounds;

    public AudioClip[] redSpawningSounds;
    public AudioClip[] redDeathSounds;


    public GameObject pointLightRef;
    private int effectcolor;
    ///
    public float deafenAudioAmount = -9f;
    [SerializeField] private float volumeDecreaseAmount;
    [SerializeField] private float volumeIncreaseAmount;

    // Reference to the AudioSource component
    private AudioSource audioSource;

    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSliderDeafault;

    


    /// //////////////
    /// ////////////////////////////////Seperation space ///////////////////////////////////////////////////////////////////
    /// //////////////
    private void Awake()
    {
        isWhiteTurn = true;
        //Finding camera and assigning script to camScript
        GameObject camera = GameObject.Find("Main Camera");
        camScript = camera.GetComponent<CameraMovements>();
        audioSource = GetComponent<AudioSource>();

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
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
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    //Is it our turn? 
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
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
                int oldTurnNumber = turnNumber;

                bool validMove = Moveto(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                    currentlyDragging.SetPosition(GetTileCenter(previousPositon.x, previousPositon.y));


                currentlyDragging = null;
                RemoveHighlightTiles();
                if (oldTurnNumber != turnNumber)
                {
                    if (currentDog != null)
                    {
                        if (dogTarget == null)
                        {
                            SetWeightedRandomDogTarget();
                            Debug.Log("the Dog's Null target Set Weight was activated");
                        }
                        Dog1();
                        /*
                        int horizontalMove = 0;  
                        int verticalMove = 0;

                        int targetPosX = dogTarget.currentX;  //These are where we want to end up. Set them to the x/y position of the target piece later.
                        int targetPosY = dogTarget.currentY;
                        Debug.Log("the Target's  X,Y Position are: " + "X" +  targetPosX + "Y" + targetPosY);

                        //These next four lines are your 'pathfinding'.
                        if (currentDog.currentX > targetPosX) horizontalMove = -1;
                        else if (currentDog.currentX < targetPosX) horizontalMove = 1;

                        if (currentDog.currentY > targetPosY) verticalMove = -1;
                        else if (currentDog.currentY < targetPosY) verticalMove = 1;

                        MoveDog(currentDog, currentDog.currentX + horizontalMove, currentDog.currentY + verticalMove);
                        */

                    }
                    if (SecondDog != null)
                    {
                        if (dogTarget2 == null)
                        {
                            SetWeightedRandomDogTarget();

                        }
                        Dog2();
                    }
                }
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
        // GreyMind is Spawned here
        if (greyturnCounter >= greyEveryXTurns && greyAlive == false)
        {
            CreateGreyMind();
            greyturnCounter = 0;
        }

        if (redturnCounter >= redEveryXTurns && currentDog == null)
        {
            CreateRedDog();
            redturnCounter = 0;
        }

        //this is just for testing
        if (Input.GetKeyDown(KeyCode.B))
        {
            CreateGreyMind();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            CreateRedDog();

        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnCoin();
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
    private ChessPiece SpawnSingleGreyMind(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        Transform childTransform = cp.transform.Find("Point Light");
        //= GetComponentInChildren<Light>();
        Light lightComponent = childTransform.GetComponent<Light>();
        if (lightComponent != null)
        {
            if (effectcolor == 0)
            {
                // Change the light for pawn invert
                lightComponent.color = new Color(0f,0.81f, 0.96f);
                lightComponent.intensity = 25f;
            }
            if (effectcolor == 1)
            {
                // Change the light for freeze
                lightComponent.color = Color.blue;
                lightComponent.intensity = 100f;
            }     
            if (effectcolor == 2)
            {
                // Change the light for Burn effect
                lightComponent.color = Color.red;
                lightComponent.intensity = 100f;

            }
            if (effectcolor == 3)
            {
                // Change the light for pawn invert
                lightComponent.color = new Color(0.96f, 0.5f, 0f);
                lightComponent.intensity = 25f;// Change the light for spawner
            }

        }
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
                if (ocp.transform.CompareTag("RedDog"))
                {
                    Destroy(ocp.gameObject);
                    currentDog = null;
                    redturnCounter = 0;
                    SpawnCoin();
                    killCounter++;
                }
                if (ocp.transform.CompareTag("YellowMind"))
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
        redturnCounter++;
        if (killCounter >= killCounterMax)
        {
            SceneManager.LoadScene("Victory Screen");
        }
        Debug.Log(greyturnCounter);
        Debug.Log(redturnCounter);
        //Debug.Log("Turn Number: " + turnNumber);


        // 
        if (isWhiteTurn==true)
        {
            Debug.Log("White Turn");
        }
        else
        {
            Debug.Log("Black Turn");
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
                Debug.Log("The Chackmate us called in check for Checkmate" + cp.team + "team won!"); 
                break;
            case 2:
                CheckMate(2);
                Debug.Log("The Chackmate us called in check for Checkmate - case 2 was activated");
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
    public bool elderBurn = false;
    public bool spawnEnemy = false;
    // 
    public bool greyAlive = false;
    private bool CreateGreyMind()
    {
        int attempts = 0;
        int maxAttempts = 100; // Set a limit to avoid potential infinite loops
        bool placedSuccessfully = false;
        ///
        //Debug.Log("i have arrived Mortal fleash");

        int randomNumber = Random.Range(0, 2);

        switch (randomNumber)
        {
            case 0:
                //Debug.Log("Random number is 0. Running code A. (pawn switch)");
                // Run code A
                StartCoroutine(PlayGreySpawnSounds());
                effectcolor = 0;
                pawnInvert = true;
                foreach (ChessPiece currentP in chessPieces)
                {
                    if (currentP != null)
                    {
                        if (currentP.type == ChessPieceType.Pawn)
                        {
                            currentP.GetComponent<Pawn>().pawnBackwardMove = true;
                            currentP.ColorPiece();
                        }
                    }
                }
                break;
            case 1:
                //Debug.Log("Random number is 1. Running code B.(locking rook, bishops or knights switch)");
                StartCoroutine(PlayGreyFreezeSounds());
                effectcolor = 1;
                int randomFreeze = Random.Range(0, 3);
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
                Debug.Log("Random number is 3. Running code D. (spawn en enemy unit");
                effectcolor = 3;
                //CreateElderRedDog();
                spawnEnemy = true;
                break;
            case 3:
                Debug.Log("Random number is 2. Running code C. (Burn Effect) ");
                effectcolor = 2;
                elderBurn = true;
                break;
            default:
                Debug.LogError("Unexpected random number!");
                break;
        }
        ///
        while (!placedSuccessfully && attempts < maxAttempts)
        {
            attempts++;
            int randomGreyX = Random.Range(0, 8);
            int randomGreyY = Random.Range(2, 6);
            int eteam = 2;
            ChessPiece greyMind = SpawnSingleGreyMind(ChessPieceType.GreyMind, eteam);
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
        StartCoroutine(PlayGreyDeathSounds());
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
                        currentP.UnColorPiece();
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
            int randomRedX = Random.Range(0, 8);
            int randomRedY = Random.Range(2, 6);
            int eteam = 2;
            ChessPiece redDog = SpawnSingleElder(ChessPieceType.RedDog, eteam);
            currentDog = redDog;
            if (chessPieces[randomRedX, randomRedY] == null)
            {
                chessPieces[randomRedX, randomRedY] = redDog;
                PositionSinglePiece(randomRedX, randomRedY, true);
                placedSuccessfully = true;
                //Debug.Log("the Dog's spawn X,Y Position are: " + "X" + randomRedX + "Y" + randomRedY);
                SetWeightedRandomDogTarget();
                Debug.Log("the Dog's spawn Set Weight was activated");
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
                //Debug.Log("the Dog's spawn X,Y Position are: " + "X" + randomRedX + "Y" + randomRedY);
                SetWeightedRandomDogTarget();
                Debug.Log("the Dog's spawn Set Weight was activated");

            }
        }
        
        return placedSuccessfully;
    }
    private bool CreateElderRedDog()
    {
        int attempts = 0;
        int maxAttempts = 100; // Set a limit to avoid potential infinite loops
        bool placedSuccessfully = false;

        while (!placedSuccessfully && attempts < maxAttempts)
        {
            attempts++;
            int randomRedX = Random.Range(0, 8);
            int randomRedY = Random.Range(2, 6);
            int eteam = 2;
            ChessPiece redDog = SpawnSingleElder(ChessPieceType.RedDog, eteam);
            SecondDog = redDog;
            if (chessPieces[randomRedX, randomRedY] == null)
            {
                chessPieces[randomRedX, randomRedY] = redDog;
                PositionSinglePiece(randomRedX, randomRedY, true);
                placedSuccessfully = true;
                //Debug.Log("the Dog's spawn X,Y Position are: " + "X" + randomRedX + "Y" + randomRedY);
                SetWeightedRandomDogTarget();
                Debug.Log("the Dog's spawn Set Weight was activated");
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
                //Debug.Log("the Dog's spawn X,Y Position are: " + "X" + randomRedX + "Y" + randomRedY);
                SetWeightedRandomDogTarget();
                Debug.Log("the Dog's spawn Set Weight was activated");

            }
        }

        return placedSuccessfully;
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
    IEnumerator PlayGreySpawnSounds()
    {
        if (greySpawningSounds.Length > 0)
        {
            // Select a random sound effect from the array
            int randomIndex = Random.Range(0, greySpawningSounds.Length);
            AudioClip randomClip = greySpawningSounds[randomIndex];

            // Set the selected sound effect to the AudioSource and play it
            audioSource.clip = randomClip;
            myMixer.SetFloat("Music", deafenAudioAmount / -9f * volumeDecreaseAmount);

            audioSource.Play();


        }
        else
        {
            Debug.LogWarning("No sound effects assigned to the soundEffects array.");
        }
        
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        float volume1 = musicSliderDeafault.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume1) * 20);
    }
    IEnumerator PlayGreyDeathSounds()
    {
        if (greyDeathSounds.Length > 0)
        {
            // Select a random sound effect from the array
            int randomIndex = Random.Range(0, greyDeathSounds.Length);
            AudioClip randomClip = greyDeathSounds[randomIndex];

            // Set the selected sound effect to the AudioSource and play it
            audioSource.clip = randomClip;
            myMixer.SetFloat("Music", deafenAudioAmount / -9f * volumeDecreaseAmount);

            audioSource.Play();


        }
        else
        {
            Debug.LogWarning("No sound effects assigned to the soundEffects array.");
        }

        while (audioSource.isPlaying)
        {
            yield return null;
        }

        float volume1 = musicSliderDeafault.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume1) * 20);
    }
    IEnumerator PlayGreyFreezeSounds()
    {
        if (greyFreezeSounds.Length > 0)
        {
            Debug.Log("The Free Sound effect is activated");
            // Select a random sound effect from the array
            int randomIndex = Random.Range(0, greyFreezeSounds.Length);
            AudioClip randomClip = greyFreezeSounds[randomIndex];

            // Set the selected sound effect to the AudioSource and play it
            audioSource.clip = randomClip;
            myMixer.SetFloat("Music", deafenAudioAmount / -9f * volumeDecreaseAmount);

            audioSource.Play();


        }
        else
        {
            Debug.LogWarning("No sound effects assigned to the soundEffects array.");
        }

        while (audioSource.isPlaying)
        {
            yield return null;
        }

        float volume1 = musicSliderDeafault.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume1) * 20);
    }

    private bool MoveDog(ChessPiece cp, int x, int y)
    {
        //if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
        //return false;

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
                if (ocp == dogTarget)
                {
                    dogTarget = null;
                }
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
                if (ocp == dogTarget)
                {
                    dogTarget = null;
                }
            }
         
        }
        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y);
        //Debug.Log("the Dog's new  X,Y Position are: " + "X" + x + "Y" + y);

        return true;


    }

    private bool MoveDog2(ChessPiece cp, int x, int y)
    {
        //if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
        //return false;

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
                if (ocp == dogTarget2)
                {
                    dogTarget2 = null;
                }
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
                if (ocp == dogTarget2)
                {
                    dogTarget2 = null;
                }
            }

        }
        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y);
        //Debug.Log("the Dog's new  X,Y Position are: " + "X" + x + "Y" + y);

        return true;


    }
    public void SetWeightedRandomDogTarget()
    {
        List<ChessPiece> grabBag = new List<ChessPiece>();

        //I don't know what the actual variable is called that is the list of all the pieces so I'm guessing 'pieces' here...
        foreach (ChessPiece thisPiece in chessPieces)
        {
            if (thisPiece != null)
            {
                switch (thisPiece.type)
                {
                    case ChessPieceType.Pawn:
                        grabBag.Add(thisPiece);
                        break;
                    case ChessPieceType.Rook: //Add rooks three times to make them three times more likely than pawns.
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        break;
                    case ChessPieceType.Bishop: //Add rooks three times to make them three times more likely than pawns.
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        break;
                    case ChessPieceType.Knight: //Add rooks three times to make them three times more likely than pawns.
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        break;
                    case ChessPieceType.Queen: //Add rooks three times to make them three times more likely than pawns.
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        grabBag.Add(thisPiece);
                        break;
                    case ChessPieceType.King:
                        break;    //Don't add the king at all - we don't want it to be a target.

                        //add your own case statements here to make the array of piece references. Add pieces more times if you want them to be more likely.
                }

            }
        }
        if(dogTarget == null)
        {
            dogTarget = grabBag[Random.Range(0, grabBag.Count)];
            Debug.Log("the target for dog 1 is " + dogTarget);
        }
        if(dogTarget2 == null)
        {
            dogTarget2 = grabBag[Random.Range(0, grabBag.Count)];
            Debug.Log("the target for dog 2 is " + dogTarget2);
        }
    }

    private void Dog1()
    {
        int horizontalMove = 0;
        int verticalMove = 0;

        int targetPosX = dogTarget.currentX;  //These are where we want to end up. Set them to the x/y position of the target piece later.
        int targetPosY = dogTarget.currentY;
        Debug.Log("the Target's  X,Y Position are: " + "X" + targetPosX + "Y" + targetPosY);

        //These next four lines are your 'pathfinding'.
        if (currentDog.currentX > targetPosX) horizontalMove = -1;
        else if (currentDog.currentX < targetPosX) horizontalMove = 1;

        if (currentDog.currentY > targetPosY) verticalMove = -1;
        else if (currentDog.currentY < targetPosY) verticalMove = 1;

        MoveDog(currentDog, currentDog.currentX + horizontalMove, currentDog.currentY + verticalMove);
    } // pathfinding and move script for dog that spawns every 2 turns
    private void Dog2()
    {
        int horizontalMove2 = 0;
        int verticalMove2 = 0;

        int targetPosX2 = dogTarget2.currentX;  //These are where we want to end up. Set them to the x/y position of the target piece later.
        int targetPosY2 = dogTarget2.currentY;
        Debug.Log("the Target's  X,Y Position are: " + "X" + targetPosX2 + "Y" + targetPosY2);

        //These next four lines are your 'pathfinding'.
        if (SecondDog.currentX > targetPosX2) horizontalMove2 = -1;
        else if (SecondDog.currentX < targetPosX2) horizontalMove2 = 1;

        if (SecondDog.currentY > targetPosY2) verticalMove2 = -1;
        else if (SecondDog.currentY < targetPosY2) verticalMove2 = 1;

        MoveDog2(SecondDog, SecondDog.currentX + horizontalMove2, SecondDog.currentY + verticalMove2);
    }  // pathfinding and move script for dog that spawns with elder
} 