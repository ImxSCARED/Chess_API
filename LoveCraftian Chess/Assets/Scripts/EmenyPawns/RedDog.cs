using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDog : ChessPiece
{

    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        for (int x = currentX - 1; x < currentX + 2; x++)
            for (int y = currentY - 1; y < currentY + 2; y++)
                if (y >= 0 && x >= 0 && x < tileCountX && y < tileCountY)
                    if (board[x, y] == null || board[x, y].team != team)
                        r.Add(new Vector2Int(x, y));


        return r;
    }
    public void RedMove(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = GetAvailableMoves(ref board, tileCountX, tileCountY);     
        
            Vector2Int move = moves[Random.Range(0, moves.Count)]; // Choose a random move
            board[currentX, currentY] = null; // Remove the pawn from the current position
            currentX = move.x;
            currentY = move.y;
            board[currentX, currentY] = this; // Place the pawn in the new position
   
    }
}
