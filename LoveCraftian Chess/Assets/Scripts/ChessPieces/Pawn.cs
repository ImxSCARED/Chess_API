using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    
    //public bool normalMove = true;
    
   
    public override List<Vector2Int> GetAvailableMoves( ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
            List<Vector2Int> r = new List<Vector2Int>();

        bool pawnBackwardMove = false;
        if (greyScript != null)
            pawnBackwardMove = greyScript.pawnInvert;

        if (pawnBackwardMove)
        {
            int direction = (team == 0) ? 1 : -1;
            // one in front kill
            if (board[currentX, currentY + direction] != null && board[currentX, currentY + direction].team != team)
                r.Add(new Vector2Int(currentX, currentY + direction));

            // two in front kill
            if (board[currentX, currentY + direction] != null && board[currentX, currentY + direction].team != team)
            {
                // White Team
                if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));

                //Black Team
                if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }

            // Inverted Move
            if (currentX != tileCountX - 1)
                if (board[currentX + 1, currentY + direction] == null)
                    r.Add(new Vector2Int(currentX + 1, currentY + direction));
            if (currentX != 0)
                if (board[currentX - 1, currentY + direction] == null)
                    r.Add(new Vector2Int(currentX - 1, currentY + direction));
        }
        else
        {
            int direction = (team == 0) ? 1 : -1;
            // one in front
            if (board[currentX, currentY + direction] == null)
                r.Add(new Vector2Int(currentX, currentY + direction));

            // two in front
            if (board[currentX, currentY + direction] == null)
            {
                // White Team
                if (team == 0 && currentY == 1 && board[currentX, currentY + (direction * 2)] == null)
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));

                //Black Team
                if (team == 1 && currentY == 6 && board[currentX, currentY + (direction * 2)] == null)
                    r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
            }

            // Kill Move
            if (currentX != tileCountX - 1)
                if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                    r.Add(new Vector2Int(currentX + 1, currentY + direction));
            if (currentX != 0)
                if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                    r.Add(new Vector2Int(currentX - 1, currentY + direction));
        }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaliableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        if((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
                return SpecialMove.Promotion;

        // En Passant
        if(movelist.Count > 0)
        {
            Vector2Int[] lastMove = movelist[movelist.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn) // if the last piece moved was a pawn
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) // if the last move was a +2 in either direction
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team) // if the move was afrom the other team
                    {
                        if (lastMove[1].y == currentY) // If both pawns are on the same Y
                        {
                            if (lastMove[1].x == currentX - 1) // Landed left
                            {
                                avaliableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == currentX + 1) // Landed Right
                            {
                                avaliableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                        
                }
            }
        }

        return SpecialMove.None;

    }
}
