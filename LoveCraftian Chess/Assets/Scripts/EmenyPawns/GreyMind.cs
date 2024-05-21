using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreyMind : ChessPiece
    
{
    

    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        return r;
    }
    private void Awake()
    {
        
    }
}
