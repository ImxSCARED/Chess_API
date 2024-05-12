
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
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

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> movelist, ref List<Vector2Int> avaliableMoves)
    {
        SpecialMove r = SpecialMove.None;

        var kingMove = movelist.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRook = movelist.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRook = movelist.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));

        if(kingMove == null && currentX == 4)
        {
            // White Team
            if(team == 0)
            {
                //Left Rook
                if(leftRook == null)
                    if (board[0, 0].type == ChessPieceType.Rook)
                        if (board[0,0].team == 0)
                            if (board[3,0] == null)
                                if (board[2,0] == null)
                                    if (board[1,0] == null)
                                    {
                                        avaliableMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }
                //Right Rook
                if (rightRook == null)
                    if (board[7, 0].type == ChessPieceType.Rook)
                        if (board[7, 0].team == 0)
                            if (board[5, 0] == null)
                                if (board[6, 0] == null)
                                    {
                                        avaliableMoves.Add(new Vector2Int(6, 0));
                                        r = SpecialMove.Castling;
                                    }
            }
            else
            {
                //Left Rook
                if (leftRook == null)
                    if (board[0, 7].type == ChessPieceType.Rook)
                        if (board[0, 7].team == 1)
                            if (board[3, 7] == null)
                                if (board[2, 7] == null)
                                    if (board[1, 7] == null)
                                    {
                                        avaliableMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }

                //Right Rook
                if (rightRook == null)
                    if (board[7, 7].type == ChessPieceType.Rook)
                        if (board[7, 7].team == 1)
                            if (board[5, 7] == null)
                                if (board[6, 7] == null)
                                {
                                    avaliableMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }
            }
        }

        return r;
    }
}
