using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess
{
    class Grid
    {
        private Cell[,] mGrid;
        private ContentManager mContent;
        private GraphicsDevice mGraphics;
        private int mMove;
        private Color mPrimaryColor;
        private Color mSecondaryColor;
        private Color mBlackOverlayColor;
        private Texture2D mPrimarySquare;
        private Texture2D mSecondarySquare;
        private Texture2D mSelectionOverlay;
        private Texture2D mPieceSelectionOverlay;
        private Texture2D mWhiteIcon;
        private Texture2D mBlackIcon;
        private Texture2D mIconOutline;
        private Texture2D mBlackOverlay;
        private Vector2 mSelection;
        private Vector2 mLastSelection;
        private List<Vector2> mPossibleMoves;
        private Player mTurn;
        private Player mWinner;
        private bool mGameOver;
        private bool mAwaitingSelection;
        private Cell mLastPiece;
        private bool mGrabbingPiece;
        private LostPieces mWhiteLostPieces;
        private LostPieces mBlackLostPieces;
        private PieceTexture[] mPieceTextures;
        private bool mCheck;

        private ChoiceMenu oChoiceMenu;
        private Button oRestartButton;

        private struct LostPieces
        {
            public int Pawns;
            public int Rooks;
            public int Knights;
            public int Bishops;
            public int Queen;
            public LostPieces()
            {
                Pawns = 0;
                Rooks = 0;
                Knights = 0;
                Bishops = 0;
                Queen = 0;
            }
        }
        private struct PieceTexture
        {
            public Piece Piece;
            public Player Ownership;
            public Texture2D Texture;

            public PieceTexture(Piece Piece, Player Ownership, Texture2D Texture)
            {
                this.Piece = Piece;
                this.Ownership = Ownership;
                this.Texture = Texture;
            }
        }

        public Vector2 Selection
        {
            get { return mSelection; }
            set { mSelection = value; }
        }
        public bool GameOver
        {
            get { return mGameOver; }
        }
        public Player Winner
        {
            get { return mWinner; }
        }

        public Grid(ContentManager Content, GraphicsDevice Graphics)
        {
            mMove = 0;
            mContent = Content;
            mGraphics = Graphics;
            mPrimaryColor = Color.FromNonPremultiplied(238, 238, 210, 255);
            mSecondaryColor = Color.FromNonPremultiplied(118, 150, 88, 255);
            mBlackOverlayColor = Color.FromNonPremultiplied(0, 0, 0, 124);
            mPrimarySquare = new Texture2D(Graphics, 1, 1);
            mPrimarySquare.SetData(new[] { mPrimaryColor });
            mSecondarySquare = new Texture2D(Graphics, 1, 1);
            mSecondarySquare.SetData(new[] { mSecondaryColor });
            mBlackOverlay = new Texture2D(Graphics, 1, 1);
            mBlackOverlay.SetData(new[] { mBlackOverlayColor });
            mSelectionOverlay = mContent.Load<Texture2D>("Selection");
            mPieceSelectionOverlay = mContent.Load<Texture2D>("SelectionPiece");
            mPossibleMoves = new List<Vector2>();
            mSelection = new Vector2(-1, -1);
            mLastSelection = mSelection;
            mTurn = Player.White;
            mGameOver = false;
            mAwaitingSelection = false;
            mGrabbingPiece = false;
            mWhiteLostPieces = new LostPieces();
            mBlackLostPieces = new LostPieces();
            mWhiteIcon = Content.Load<Texture2D>("WhiteChessIcon");
            mBlackIcon = Content.Load<Texture2D>("BlackChessIcon");
            mIconOutline = Content.Load<Texture2D>("IconOutline");
            mCheck = false;

            oRestartButton = new Button(Content.Load<Texture2D>("RestartButton"),
                Content.Load<Texture2D>("RestartButtonHovered"));

            mPieceTextures = new PieceTexture[12];
            int x = 0;
            string sOwnership = "w";
            Player pOwnership = Player.White;
            for (int i = 0; i < 2; i++)
            {
                if (i == 1)
                {
                    sOwnership = "b";
                    pOwnership = Player.Black;
                }
                foreach (Piece oPiece in Enum.GetValues(typeof(Piece)))
                {
                    if (oPiece == Piece.None)
                        continue;
                    string TextureName = sOwnership + Convert.ToString((char)oPiece);
                    mPieceTextures[x] = new PieceTexture(oPiece, pOwnership, Content.Load<Texture2D>(TextureName));
                    x++;
                }
            }

            mGrid = new Cell[8, 8];
            for (int i = 0; i < mGrid.GetLength(0); i++)
                for (int j = 0; j < mGrid.GetLength(1); j++)
                    mGrid[i, j] = new Cell(mContent);

            SetupBoard();
        }

        public void Update()
        {
            Input.NormalCursor();

            if (mGameOver)
                return;

            if (mAwaitingSelection)
            {
                NewPieceSelection();
                return;
            }

            oRestartButton.Update();

            if (oRestartButton.Pressed)
            {
                RestartBoard();
                oRestartButton.Pressed = false;
            }

            if ((mLastSelection != mSelection ||
                (Input.MouseClick() || Input.MouseRelease())) &&
                !(mSelection.X < 0 ||
                    mSelection.Y < 0 ||
                    mSelection.X > 7 ||
                    mSelection.Y > 7))
            {
                foreach (Vector2 pMove in mPossibleMoves)
                {
                    if (mSelection.X == pMove.X
                        && mSelection.Y == pMove.Y)
                    {
                        MovePiece(mLastSelection, pMove);
                        mPossibleMoves.Clear();
                        mLastSelection = mSelection;
                        return;
                    }
                }

                Cell SelectedCell = mGrid[(int)mSelection.X, (int)mSelection.Y];
                if (SelectedCell.Piece != Piece.None)
                {
                    SelectedCell.Grabbed = true;
                    mGrabbingPiece = true;
                }

                if (mTurn == SelectedCell.Ownership &&
                    !(Input.MouseRelease() && mSelection != mLastSelection))
                {
                    mPossibleMoves = System.FindMoves(mGrid, mSelection, true, mMove);
                    mLastSelection = mSelection;
                }
                else
                {
                    mSelection = mLastSelection;
                    mPossibleMoves.Clear();
                }
            }

            if (Input.MouseRelease() || !Input.MouseHeld())
            {
                foreach (Cell oCell in mGrid)
                    oCell.Grabbed = false;
                mGrabbingPiece = false;
            }

            if (mGrabbingPiece)
                Input.GrabCursor();
            else
            {
                if (Game1.MouseGridPosition.X > -1 &&
                    Game1.MouseGridPosition.X < 8 &&
                    Game1.MouseGridPosition.Y > -1 &&
                    Game1.MouseGridPosition.Y < 8 &&
                    mGrid[(int)Game1.MouseGridPosition.X, (int)Game1.MouseGridPosition.Y].Piece != Piece.None)
                        Input.HoverCursor();
            }
        }
        public void RestartBoard()
        {
            mMove = 0;
            mTurn = Player.White;
            mGrid = new Cell[8, 8];
            for (int i = 0; i < mGrid.GetLength(0); i++)
                for (int j = 0; j < mGrid.GetLength(1); j++)
                    mGrid[i, j] = new Cell(mContent);
            SetupBoard();
            mBlackLostPieces = new LostPieces();
            mWhiteLostPieces = new LostPieces();
            mGameOver = false;
            mSelection = new Vector2(-1, -1);
            mLastSelection = mSelection;
            mPossibleMoves.Clear();
            mGameOver = false;
            mAwaitingSelection = false;
            mGrabbingPiece = false;
            mCheck = false;
        }

        private void MovePiece(Vector2 OldPosition, Vector2 NewPosition)
        {
            Cell OldPiece = mGrid[(int)OldPosition.X, (int)OldPosition.Y];
            Cell NewPiece = mGrid[(int)NewPosition.X, (int)NewPosition.Y];
            if (NewPiece.Ownership == OldPiece.Ownership &&
                OldPiece.Piece == Piece.King && NewPiece.Piece == Piece.Rook)
            {
                if (NewPosition.X > OldPosition.X)
                {
                    mGrid[6, (int)OldPosition.Y].Piece = Piece.King;
                    mGrid[6, (int)OldPosition.Y].Ownership = OldPiece.Ownership;
                    mGrid[6, (int)OldPosition.Y].Original = false;
                    mGrid[5, (int)OldPosition.Y].Piece = Piece.Rook;
                    mGrid[5, (int)OldPosition.Y].Ownership = OldPiece.Ownership;
                    mGrid[5, (int)OldPosition.Y].Original = false;

                }
                else
                {
                    mGrid[2, (int)OldPosition.Y].Piece = Piece.King;
                    mGrid[2, (int)OldPosition.Y].Ownership = OldPiece.Ownership;
                    mGrid[2, (int)OldPosition.Y].Original = false;
                    mGrid[3, (int)OldPosition.Y].Piece = Piece.Rook;
                    mGrid[3, (int)OldPosition.Y].Ownership = OldPiece.Ownership;
                    mGrid[3, (int)OldPosition.Y].Original = false;
                }
                OldPiece.Original = false;
                OldPiece.Ownership = Player.None;
                OldPiece.Piece = Piece.None;
                NewPiece.Original = false;
                NewPiece.Ownership = Player.None;
                NewPiece.Piece = Piece.None;
                ChangeTurn();
                return;
            }

            if (NewPiece.Piece != Piece.None)
            {
                LostPieces oLostPieces = new LostPieces();
                if (NewPiece.Ownership == Player.White)
                    oLostPieces = mWhiteLostPieces;
                if (NewPiece.Ownership == Player.Black)
                    oLostPieces = mBlackLostPieces;
                switch (NewPiece.Piece)
                {
                    case Piece.Pawn:
                        oLostPieces.Pawns++;
                        break;
                    case Piece.Rook:
                        oLostPieces.Rooks++;
                        break;
                    case Piece.Bishop:
                        oLostPieces.Bishops++;
                        break;
                    case Piece.Knight:
                        oLostPieces.Knights++;
                        break;
                    case Piece.Queen:
                        oLostPieces.Queen++;
                        break;
                }

                if (NewPiece.Ownership == Player.White)
                    mWhiteLostPieces = oLostPieces;
                if (NewPiece.Ownership == Player.Black)
                    mBlackLostPieces = oLostPieces;
            }

            if (OldPiece.Piece == Piece.Pawn &&
                NewPiece.Piece == Piece.None &&
                (NewPosition.X == OldPosition.X - 1 ||
                NewPosition.X == OldPosition.X + 1))
            {
                Cell PassantPiece;
                if (NewPosition.X == OldPosition.X - 1)
                {
                    PassantPiece = mGrid[(int)OldPosition.X - 1, (int)OldPosition.Y];
                    PassantPiece.Piece = Piece.None;
                    PassantPiece.Ownership = Player.None;
                    PassantPiece.Original = false;
                }
                if (NewPosition.X == OldPosition.X + 1)
                {
                    PassantPiece = mGrid[(int)OldPosition.X + 1, (int)OldPosition.Y];
                    PassantPiece.Piece = Piece.None;
                    PassantPiece.Ownership = Player.None;
                    PassantPiece.Original = false;
                }
                if (OldPiece.Ownership == Player.White)
                    mBlackLostPieces.Pawns++;
                else if (OldPiece.Ownership == Player.Black)
                    mWhiteLostPieces.Pawns++;
            }

            NewPiece.Original = OldPiece.Original;
            NewPiece.Ownership = OldPiece.Ownership;
            NewPiece.Piece = OldPiece.Piece;
            if (NewPiece.Original && 
                ((NewPosition.Y == 3 && NewPiece.Ownership == Player.Black) || 
                (NewPosition.Y == 4 && NewPiece.Ownership == Player.White)))
            {
                NewPiece.EnPassant = true;
            }
            NewPiece.Original = false;
            NewPiece.LastMove = mMove;
            OldPiece.Original = false;
            OldPiece.Ownership = Player.None;
            OldPiece.Piece = Piece.None;
            if (NewPiece.Piece == Piece.Pawn &&
                ((NewPosition.Y == 0 && NewPiece.Ownership == Player.White) ||
                (NewPosition.Y == 7 && NewPiece.Ownership == Player.Black)))
            {
                string PlayerColor = "w";
                if (NewPiece.Ownership == Player.Black)
                    PlayerColor = "b";
                mAwaitingSelection = true;
                oChoiceMenu = new ChoiceMenu(mContent, mGraphics, Game1.WindowSize,
                    new ChoiceMenu.Choice[]
                    {
                        new ChoiceMenu.Choice(mContent.Load<Texture2D>(PlayerColor + "q")),
                        new ChoiceMenu.Choice(mContent.Load<Texture2D>(PlayerColor + "r")),
                        new ChoiceMenu.Choice(mContent.Load<Texture2D>(PlayerColor + "b")),
                        new ChoiceMenu.Choice(mContent.Load<Texture2D>(PlayerColor + "n"))
                    });
                mLastPiece = NewPiece;
                return;
            }
            ChangeTurn();
        }

        private void NewPieceSelection()
        {
            oChoiceMenu.Update();
            if (oChoiceMenu.Selection != 0)
            {
                switch(oChoiceMenu.Selection)
                {
                    case 1:
                        mLastPiece.Piece = Piece.Queen;
                        break;
                    case 2:
                        mLastPiece.Piece = Piece.Rook;
                        break;
                    case 3:
                        mLastPiece.Piece = Piece.Bishop;
                        break;
                    case 4:
                        mLastPiece.Piece = Piece.Knight;
                        break;
                }
                oChoiceMenu.IsActive = false;
                mAwaitingSelection = false;
                ChangeTurn();
                mPossibleMoves.Clear();
            }
        }

        private void ChangeTurn()
        {
            if (mTurn == Player.White)
                mTurn = Player.Black;
            else
                mTurn = Player.White;

            mCheck = System.IsKingCheck(mGrid, mTurn);

            if (System.IsCheckMate(mGrid, mTurn))
            {
                mGameOver = true;
                if (mTurn == Player.White)
                    mWinner = Player.Black;
                else
                    mWinner = Player.White;
            }

            if (System.IsStaleMate(mGrid, mTurn))
            {
                mGameOver = true;
                mWinner = Player.None;
            }

            mMove++;
        }

        private void SetupBoard()
        {
            for (int i = 0; i < mGrid.GetLength(0); i++)
                for (int j = 0; j < mGrid.GetLength(1); j++)
                {
                    if (j <= 1)
                        mGrid[i, j].Ownership = Player.Black;
                    else if (j >= 6)
                        mGrid[i, j].Ownership = Player.White;
                    else
                        continue;

                    if (j == 1 || j == 6)
                        mGrid[i, j].Piece = Piece.Pawn;
                    else
                    {
                        if (i == 0 || i == 7)
                            mGrid[i, j].Piece = Piece.Rook;
                        else if (i == 1 || i == 6)
                            mGrid[i, j].Piece = Piece.Knight;
                        else if (i == 2 || i == 5)
                            mGrid[i, j].Piece = Piece.Bishop;
                        else if (i == 3)
                            mGrid[i, j].Piece = Piece.Queen;
                        else if (i == 4)
                            mGrid[i, j].Piece = Piece.King;
                    }
                }
        }

        public void Draw(SpriteBatch mSpriteBatch, Vector2 Position, int Size)
        {

            Vector2 OutlinePosition = new Vector2((int)Position.X - Size, (int)Position.Y);
            if (mTurn == Player.Black)
                OutlinePosition = new Vector2((int)(Position.X + Size * 8 + Size * 0.25), (int)Position.Y);

            mSpriteBatch.Draw(mIconOutline, new Rectangle(
                (int)OutlinePosition.X, (int)OutlinePosition.Y,
                (int)(Size * 0.75), (int)(Size * 0.75)), Color.White);

            mSpriteBatch.Draw(mWhiteIcon, new Rectangle
                ((int)Position.X - Size, (int)Position.Y,
                (int)(Size * 0.75), (int)(Size * 0.75)),
                Color.White);
            mSpriteBatch.Draw(mBlackIcon, new Rectangle
                ((int)(Position.X + Size * 8 + Size * 0.25), (int)Position.Y,
                (int)(Size * 0.75), (int)(Size * 0.75)),
                Color.White);

            DrawLostPieces(mSpriteBatch, Position, Size);

            oRestartButton.Draw(mSpriteBatch,
                new Vector2(Position.X, Position.Y + Size * 8 + (Size / 30)), Size / 4);

            for (int i = 0; i < mGrid.GetLength(0); i++)
                for (int j = 0; j < mGrid.GetLength(1); j++)
                {
                    Texture2D Box;
                    if ((i + j) % 2 == 0)
                        Box = mPrimarySquare;
                    else
                        Box = mSecondarySquare;

                    mSpriteBatch.Draw(Box, new Rectangle
                        (i * Size + (int)Position.X,
                        j * Size + (int)Position.Y, Size, Size),
                        Color.White);
                }

            for (int i = 0; i < mGrid.GetLength(0); i++)
                for (int j = 0; j < mGrid.GetLength(1); j++)
                {
                    mGrid[i, j].Check = (mCheck && mGrid[i, j].Ownership == mTurn);

                    if (mGrid[i, j].Grabbed)
                        continue;

                    mGrid[i, j].Draw(mSpriteBatch,
                        new Vector2(i * Size + (int)Position.X,
                        j * Size + (int)Position.Y), Size);
                }

            foreach (Vector2 pMove in mPossibleMoves)
            {
                Texture2D Select;
                if (mGrid[(int)pMove.X, (int)pMove.Y].Piece == Piece.None)
                    Select = mSelectionOverlay;
                else
                    Select = mPieceSelectionOverlay;

                mSpriteBatch.Draw(Select, new Rectangle
                        ((int)pMove.X * Size + (int)Position.X,
                        (int)pMove.Y * Size + (int)Position.Y, Size, Size),
                        Color.White);
            }

            for (int i = 0; i < mGrid.GetLength(0); i++)
                for (int j = 0; j < mGrid.GetLength(1); j++)
                {
                    if (mGrid[i, j].Grabbed)
                    {
                        mGrid[i, j].Draw(mSpriteBatch, new Vector2(
                        Input.MouseLocation().X - Size / 2,
                        Input.MouseLocation().Y - Size / 2), Size);
                    }
                }

            if (mAwaitingSelection || mGameOver)
                mSpriteBatch.Draw(mBlackOverlay, new Rectangle
                    (0, 0, (int)Game1.WindowSize.X,
                    (int)Game1.WindowSize.Y), mBlackOverlayColor);

            if (mAwaitingSelection)
                oChoiceMenu.Draw(mSpriteBatch);
        }

        public void DrawLostPieces(SpriteBatch mSpriteBatch, Vector2 Position, int Size)
        {
            Vector2 Seperation = new Vector2(0.375f, 0.45f);
            int x = 0;
            Texture2D CurrentTexture = mPieceTextures[0].Texture;
            Player PieceType = Player.Black;
            LostPieces RefLostPieces = mBlackLostPieces;
            /*
             * Black Pieces
             */
            CurrentTexture = FindTexture(Piece.Pawn, PieceType);
            for (int i = 0; i < RefLostPieces.Pawns; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X - Size * 1.25 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Rook, PieceType);
            for (int i = 0; i < RefLostPieces.Rooks; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X - Size * 1.25 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Bishop, PieceType);
            for (int i = 0; i < RefLostPieces.Bishops; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X - Size * 1.25 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Knight, PieceType);
            for (int i = 0; i < RefLostPieces.Knights; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X - Size * 1.25 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Queen, PieceType);
            for (int i = 0; i < RefLostPieces.Queen; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X - Size * 1.25 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            /*
             * White Pieces
             */
            x = 0;
            PieceType = Player.White;
            RefLostPieces = mWhiteLostPieces;

            CurrentTexture = FindTexture(Piece.Pawn, PieceType);
            for (int i = 0; i < RefLostPieces.Pawns; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X + Size * 8 + Size * 0.04 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Rook, PieceType);
            for (int i = 0; i < RefLostPieces.Rooks; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X + Size * 8 + Size * 0.04 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Bishop, PieceType);
            for (int i = 0; i < RefLostPieces.Bishops; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X + Size * 8 + Size * 0.04 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Knight, PieceType);
            for (int i = 0; i < RefLostPieces.Knights; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X + Size * 8 + Size * 0.04 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }

            CurrentTexture = FindTexture(Piece.Queen, PieceType);
            for (int i = 0; i < RefLostPieces.Queen; i++)
            {
                Vector2 PiecePosition = new Vector2((x - (int)(x / 3) * 3) * Size * Seperation.X, ((int)(x / 3)) * Size * Seperation.Y);
                mSpriteBatch.Draw(CurrentTexture, new Rectangle(
                    (int)(Position.X + Size * 8 + Size * 0.04 + PiecePosition.X),
                    (int)(Position.Y + Size * 0.75 + PiecePosition.Y),
                    Size / 2, Size / 2), Color.White);
                x++;
            }
        }

        private Texture2D FindTexture(Piece Piece, Player Player)
        {
            for (int i = 0; i < mPieceTextures.Count(); i++)
                if (mPieceTextures[i].Piece == Piece &&
                    mPieceTextures[i].Ownership == Player)
                {
                    return mPieceTextures[i].Texture;
                }
            return null;
        }
    }
}
