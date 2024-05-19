using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreyMind : ChessPiece
    
{
    public bool Grey1 = false;
    public bool Grey2 = false;
    public bool Grey3 = false;
    public bool Grey4 = false;

    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        return r;
    }
    private void Awake()
    {
        Debug.Log("i have arrived Mortal fleash");

        int randomNumber = Random.Range(1, 1);

        switch(randomNumber)
        {
            case 0:
                Debug.Log("Random number is 0. Running code A. (pawn switch)");
                // Run code A
                Grey1 = true;
                break;
            case 1:
                Debug.Log("Random number is 1. Running code B.(locking rook, bishops or knights switch)");
                // Run code B
                break;
            case 2:
                Debug.Log("Random number is 2. Running code C. (visual effect on board) ");
                // Run code C
                break;
            case 3:
                Debug.Log("Random number is 3. Running code D. (spawn en enemy unit ");
                // Run code C
                break;
            default:
                Debug.LogError("Unexpected random number!");
                break;
        }
    }
}
