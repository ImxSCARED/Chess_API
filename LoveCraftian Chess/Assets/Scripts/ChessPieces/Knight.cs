
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        if (isFrozen)
        {
            Debug.Log("knight frozen bool activated");
        }
        else
        {
            Vector2Int[] DMs = new Vector2Int[8]
        {
            new Vector2Int(currentX+1,currentY+2),
            new Vector2Int(currentX-1,currentY+2),
            new Vector2Int(currentX+1,currentY-2),
            new Vector2Int(currentX-1,currentY-2),
            new Vector2Int(currentX+2,currentY+1),
            new Vector2Int(currentX+2,currentY-1),
            new Vector2Int(currentX-2,currentY+1),
            new Vector2Int(currentX-2,currentY-1)
        };

            for (int i = 0; i < DMs.Length; i++)
            {
                if (DMs[i].x >= 0 & DMs[i].x <= tileCountX - 1 & DMs[i].y >= 0 & DMs[i].y <= tileCountY - 1)
                {
                    if (board[DMs[i].x, DMs[i].y] != null)
                    {
                        if (board[DMs[i].x, DMs[i].y].team != team) r.Add(DMs[i]);
                    }
                    else
                    {
                        r.Add(DMs[i]);
                    }
                }
            }
        }
        
        return r;
    }
}
       
