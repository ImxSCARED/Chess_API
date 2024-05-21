using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
    GreyMind = 7,
    RedDog = 8,
    YellowMind = 9
}

public class ChessPiece : MonoBehaviour
{

    [Header("variables")]
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    public string materialPath = "FrozenMat/Cosmos";
    public Material originalMaterial;

    public bool isFrozen = false; //Only use on bishop, Rook, Knight to stop their movement
    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    public GreyMind greyScript;
    private Chessboard chessScript;

    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == 0) ? Vector3.zero : new Vector3(0,180,0));
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }


    //check how many moves are available

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int TileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

     
        return r;
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaliableMoves)
    {
        return SpecialMove.None;
    }

    //set the positions of the pieces
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }
    //set the size of the pieces
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.localScale = desiredScale;
    }

    //swaps the material/look to frozen one
    public void FreezePiece()
    {
        if(type == ChessPieceType.Rook || type == ChessPieceType.Knight || type == ChessPieceType.Bishop)
        {
            isFrozen = true;
            Material[] mats = gameObject.GetComponent<Renderer>().materials;
            mats[0] = Resources.Load<Material>(materialPath);
            gameObject.GetComponent<Renderer>().materials = mats;
        }
        else
        {
            Debug.Log("Trying to freeze a piece that cant freeze fucking idiot");
        }
    }

    public void UnFreezePiece()
    {
        if(type == ChessPieceType.Rook || type == ChessPieceType.Knight || type == ChessPieceType.Bishop)
        {
            isFrozen = false;
            gameObject.GetComponent<Renderer>().material = originalMaterial;
        }
        else
        {
            Debug.Log("Trying to unfreeze a piece that cant unfreeze fucking idiot");
        }
    }
}