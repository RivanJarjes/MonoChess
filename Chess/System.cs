using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Chess
{
    public static class System
    {
        public static List<Vector2> FindMoves(Cell[,] Grid, Vector2 Position, bool Recursion = true, int Move = -1)
        {
            List<Vector2> Moves = new List<Vector2>();
            int X = (int)Position.X;
            int Y = (int)Position.Y;

            switch (Grid[X, Y].Piece)
            {
                case Piece.Pawn:
                    int DirectionMultiplier = 1;
                    if (Grid[X, Y].Ownership == Player.Black)
                        DirectionMultiplier = -1;
                    if (PossibleTargetMove(Grid, new Vector2(
                        X - 1, Y - DirectionMultiplier), Position))
                        Moves.Add(new Vector2(X - 1, Y - DirectionMultiplier));
                    if (PossibleTargetMove(Grid, new Vector2(
                        X + 1, Y - DirectionMultiplier), Position))
                        Moves.Add(new Vector2(X + 1, Y - DirectionMultiplier));
                    if (PossibleEmptyMove(Grid, new Vector2(
                        X, Y - DirectionMultiplier)))
                        Moves.Add(new Vector2(X, Y - DirectionMultiplier));
                    else
                        break;
                    if (Grid[X, Y].Original &&
                        PossibleEmptyMove(Grid, new Vector2(
                        X, Y - DirectionMultiplier * 2)))
                        Moves.Add(new Vector2(X, Y - DirectionMultiplier * 2));
                    
                    if (X < 7)
                    {
                        if (Grid[X + 1, Y].Piece == Piece.Pawn && 
                            Grid[X + 1, Y].EnPassant &&
                            Grid[X + 1, Y].LastMove == Move - 1 &&
                            Grid[X + 1, Y].Ownership != Grid[X,Y].Ownership &&
                            PossibleEmptyMove(Grid, new Vector2(
                            X + 1, Y - DirectionMultiplier)))
                        {
                            Moves.Add(new Vector2(X + 1, Y - DirectionMultiplier));
                        }
                    }
                    if (X > 0)
                    {
                        if (Grid[X - 1, Y].Piece == Piece.Pawn &&
                            Grid[X - 1, Y].EnPassant &&
                            Grid[X - 1, Y].LastMove == Move - 1 &&
                            Grid[X - 1, Y].Ownership != Grid[X, Y].Ownership &&
                            PossibleEmptyMove(Grid, new Vector2(
                            X - 1, Y - DirectionMultiplier)))
                        {
                            Moves.Add(new Vector2(X - 1, Y - DirectionMultiplier));
                        }
                    }

                    break;

                case Piece.Knight:
                    for (int x = -2; x <= 2; x++)
                        for (int y = -2; y <= 2; y++)
                        {
                            if (x == 0 || y == 0 ||
                                Math.Abs(x) == Math.Abs(y))
                                continue;
                            if (PossibleMove(Grid, new Vector2(
                                X + x, Y + y), Position))
                                Moves.Add(new Vector2(X + x, Y + y));
                        }
                    break;

                case Piece.Rook:
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            for (int a = 1; a < 8; a++)
                            {
                                if (x != 0 && y != 0)
                                    continue;
                                if (PossibleEmptyMove(Grid, new Vector2
                                (X + (x * a), Y + (y * a))))
                                    Moves.Add(new Vector2(X + (x * a), Y + (y * a)));
                                else
                                {
                                    if (PossibleTargetMove(Grid, new Vector2(
                                        X + (x * a), Y + (y * a)), Position))
                                        Moves.Add(new Vector2(X + (x * a), Y + (y * a)));
                                    break;
                                }
                            }
                    break;

                case Piece.Bishop:
                    for (int x = -1; x <= 1; x += 2)
                        for (int y = -1; y <= 1; y += 2)
                            for (int a = 1; a < 8; a++)
                            {
                                if (PossibleEmptyMove(Grid, new Vector2
                                (X + (x * a), Y + (y * a))))
                                    Moves.Add(new Vector2(X + (x * a), Y + (y * a)));
                                else
                                {
                                    if (PossibleTargetMove(Grid, new Vector2(
                                        X + (x * a), Y + (y * a)), Position))
                                        Moves.Add(new Vector2(X + (x * a), Y + (y * a)));
                                    break;
                                }
                            }
                    break;

                case Piece.Queen:
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            for (int a = 1; a < 8; a++)
                            {
                                if (PossibleEmptyMove(Grid, new Vector2
                                (X + (x * a), Y + (y * a))))
                                    Moves.Add(new Vector2(X + (x * a), Y + (y * a)));
                                else
                                {
                                    if (PossibleTargetMove(Grid, new Vector2(
                                        X + (x * a), Y + (y * a)), Position))
                                        Moves.Add(new Vector2(X + (x * a), Y + (y * a)));
                                    break;
                                }
                            }
                    break;

                case Piece.King:
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0)
                                continue;
                            if (PossibleMove(Grid, new Vector2
                            (X + x, Y + y), Position))
                                Moves.Add(new Vector2(X + x, Y + y));
                        }

                    if (Grid[X, Y].Original)
                    {
                        if (Grid[5, Y].Piece == Piece.None &&
                        Grid[6, Y].Piece == Piece.None &&
                        Grid[7, Y].Original && Grid[7, Y].Piece == Piece.Rook)
                            Moves.Add(new Vector2(7, Y));
                        if (Grid[3, Y].Piece == Piece.None &&
                        Grid[2, Y].Piece == Piece.None &&
                        Grid[1, Y].Piece == Piece.None &&
                        Grid[0, Y].Original && Grid[0, Y].Piece == Piece.Rook)
                            Moves.Add(new Vector2(0, Y));
                    }
                    break;
            }

            if (Recursion)
                for (int i = Moves.Count-1; i > -1; i--)
                {
                    if (IsMoveBad(Grid, Position, Moves[i]))
                    {
                        Moves.RemoveAt(i);
                    }
                }

            return Moves;
        }

        public static bool IsCheckMate(Cell[,] mGrid, Player Player)
        {
            if (!IsKingCheck(mGrid, Player))
                return false;

            for (int x = 0; x < mGrid.GetLength(0); x++)
                for (int y = 0; y < mGrid.GetLength(1); y++)
                    if (mGrid[x, y].Ownership == Player)
                    {
                        List<Vector2> MoveList = FindMoves(mGrid, new Vector2(x, y));
                        if (MoveList.Count > 0)
                            return false;
                    }

            return true;
        }

        public static bool IsStaleMate(Cell[,] mGrid, Player Player)
        {
            if (IsKingCheck(mGrid, Player))
                return false;

            for (int x = 0; x < mGrid.GetLength(0); x++)
                for (int y = 0; y < mGrid.GetLength(1); y++)
                    if (mGrid[x, y].Ownership == Player)
                    {
                        List<Vector2> MoveList = FindMoves(mGrid, new Vector2(x, y));
                        if (MoveList.Count > 0)
                            return false;
                    }

            return true;
        }

        public static bool IsKingCheck(Cell[,] mGrid, Player Player)
        {
            Vector2 KingPosition = Vector2.Zero;
            for (int x = 0; x < mGrid.GetLength(0); x++)
                for (int y = 0; y < mGrid.GetLength(1); y++)
                    if (mGrid[x, y].Piece == Piece.King &&
                        mGrid[x, y].Ownership == Player)
                        KingPosition = new Vector2(x, y);

            for (int x = 0; x < mGrid.GetLength(0); x++)
                for (int y = 0; y < mGrid.GetLength(1); y++)
                    if (mGrid[x, y].Piece != Piece.None &&
                        mGrid[x, y].Ownership != Player)
                    {
                        List<Vector2> MoveList = FindMoves(mGrid, new Vector2(x, y), false);
                        foreach (Vector2 pMove in MoveList)
                            if (KingPosition == pMove)
                                return true;
                    }

            return false;
        }

        private static bool IsMoveBad(Cell[,] mGrid, Vector2 oldPosition, Vector2 newPosition)
        {
            int oX = (int)oldPosition.X;
            int oY = (int)oldPosition.Y;
            int nX = (int)newPosition.X;
            int nY = (int)newPosition.Y;
            Cell[,] nGrid = new Cell[mGrid.GetLength(0), mGrid.GetLength(1)];
            for (int x = 0; x < mGrid.GetLength(0); x++)
                for (int y = 0; y < mGrid.GetLength(1); y++)
                {
                    nGrid[x, y] = new Cell(mGrid[x, y].Content)
                    {
                        Ownership = mGrid[x, y].Ownership,
                        Piece = mGrid[x, y].Piece,
                        Original = mGrid[x, y].Original
                    };
                }
            nGrid[nX, nY].Piece = nGrid[oX, oY].Piece;
            nGrid[nX, nY].Ownership = nGrid[oX, oY].Ownership;
            nGrid[nX, nY].Original = false;
            nGrid[oX, oY].Piece = Piece.None;
            nGrid[oX, oY].Ownership = Player.None;
            nGrid[oX, oY].Original = false;

            return IsKingCheck(nGrid, nGrid[nX, nY].Ownership);
        }

        private static bool PossibleMove(Cell[,] Grid, Vector2 NewPosition, Vector2 OldPosition)
        {
            int oldX = (int)OldPosition.X;
            int oldY = (int)OldPosition.Y;
            int X = (int)NewPosition.X;
            int Y = (int)NewPosition.Y;
            if (X > -1 && X < 8 &&
                Y > -1 && Y < 8 &&
                Grid[X, Y].Ownership != Grid[oldX, oldY].Ownership)
            {
                return true;
            }
            return false;
        }

        private static bool PossibleEmptyMove(Cell[,] Grid, Vector2 Position)
        {
            int X = (int)Position.X;
            int Y = (int)Position.Y;
            if (X > -1 && X < 8 &&
                Y > -1 && Y < 8 &&
                Grid[X,Y].Piece == Piece.None)
            {
                return true;
            }
            return false;
        }

        private static bool PossibleTargetMove(Cell[,] Grid, Vector2 NewPosition, Vector2 OldPosition)
        {
            int oldX = (int)OldPosition.X;
            int oldY = (int)OldPosition.Y;
            int X = (int)NewPosition.X;
            int Y = (int)NewPosition.Y;
            if (X > -1 && X < 8 &&
                Y > -1 && Y < 8 &&
                Grid[X, Y].Ownership != Grid[oldX, oldY].Ownership &&
                Grid[X, Y].Piece != Piece.None)
            {
                return true;
            }
            return false;
        }
    }
}
