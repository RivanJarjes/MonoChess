using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Chess
{
    public enum Piece
    {
        Bishop = 'b',
        King = 'k',
        Knight = 'n',
        Pawn = 'p',
        Queen = 'q',
        Rook = 'r',
        None = ' '
    }

    public enum Player
    {
        White = 'w',
        Black = 'b',
        None = ' '
    }
    public class Cell
    {
        private Texture2D mTexture;
        private Piece mPiece;
        private Player mOwnership;
        private ContentManager mContent;
        private bool mOriginal;
        private bool mCheck;
        private bool mGrabbed;
        private bool mEnPassant;
        private int mLastMove;

        public ContentManager Content
        {
            get { return mContent; }
            set { mContent = value; }
        }
        public Piece Piece
        {
            get { return mPiece; }
            set { mPiece = value; SetTexture(); }
        }
        public bool Check
        {
            get { return mCheck; }
            set { mCheck = value; SetTexture(); }
        }
        public Player Ownership
        {
            get { return mOwnership; }
            set { mOwnership = value; }
        }
        public bool Original
        {
            get { return mOriginal; }
            set { mOriginal = value; }
        }

        public bool Grabbed
        {
            get { return mGrabbed; }
            set { mGrabbed = value; }
        }

        public int LastMove
        {
            get { return mLastMove; }
            set { mLastMove = value; }
        }

        public bool EnPassant
        {
            get { return mEnPassant; }
            set { mEnPassant = value; }
        }

        public Cell(ContentManager Content)
        {
            this.mContent = Content;
            mOwnership = Player.None;
            mPiece = Piece.None;
            mOriginal = true;
            mLastMove = 0;
            mEnPassant = false;
            SetTexture();
        }

        private void SetTexture()
        {
            if (mPiece == Piece.None ||
                mOwnership == Player.None)
            {
                mTexture = null;
                return;
            }
            string TextureName = Convert.ToString((char)mOwnership) + 
                Convert.ToString((char)mPiece);
            if (mCheck && mPiece == Piece.King)
                TextureName = "c" + TextureName;
            mTexture = mContent.Load<Texture2D>(TextureName);
        }

        public void Draw(SpriteBatch mSpriteBatch, Vector2 Position, int Size)
        {
            if (mTexture == null)
                return;

            mSpriteBatch.Draw(mTexture, 
                new Rectangle((int)Position.X, (int)Position.Y, Size, Size), 
                Color.White);
        }
    }
}
