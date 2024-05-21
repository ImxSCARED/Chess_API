using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreyMind : ChessPiece
    
{
    public bool pawnInvert = false;
    public bool lockRookBishOrKnight = false;
    public bool lockRook = false;
    public bool lockBishop = false;
    public bool lockKight = false;
    public bool eldrichBoard = false;
    public bool spawnEnemy = false;

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
                pawnInvert = true;
                break;
            case 1:
                Debug.Log("Random number is 1. Running code B.(locking rook, bishops or knights switch)");
                // Run code B
                int randomFreeze = Random.Range(0, 0);
                switch(randomFreeze)
                {
                    case 0:
                        //run rook
                        lockRook = true;

                        break;
                    case 1:
                        //run bishop
                        lockBishop = true;
                        break;
                    case 2:
                        //run knight
                        lockKight = true;
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
                Debug.Log("Random number is 3. Running code D. (spawn en enemy unit ");
                // Run code C
                spawnEnemy = true;
                break;
            default:
                Debug.LogError("Unexpected random number!");
                break;
        }
    }
}
