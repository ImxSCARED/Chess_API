using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
   public override List<Vector2Int> GetAvailableMoves( ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        // one in front
        if (board[currentX, currentY + direction] == null)
            r.Add(new Vector2Int(currentX, currentY + direction));


        return r;
    }
}
