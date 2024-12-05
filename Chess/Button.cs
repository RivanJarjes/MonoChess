using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Chess
{
    class Button
    {
        private Texture2D mCurrentTexture;
        private Texture2D mButtonTexture;
        private Texture2D mButtonHoveredTexture;
        private Vector2 mPosition;
        private int mSize;
        private bool mPressed;

        public bool Pressed
        {
            get { return mPressed; }
            set { mPressed = value; }
        }

        public Button(Texture2D ButtonTexture, Texture2D ButtonHoveredTexture)
        {
            this.mButtonTexture = ButtonTexture;
            this.mButtonHoveredTexture = ButtonHoveredTexture;
            this.mPosition = Vector2.Zero;
            this.mSize = 10;
            this.mCurrentTexture = this.mButtonTexture;
        }

        public void Update()
        {
            if (Input.MouseLocation().X >= mPosition.X &&
                Input.MouseLocation().X < mPosition.X + mSize &&
                Input.MouseLocation().Y >= mPosition.Y &&
                Input.MouseLocation().Y < mPosition.Y + mSize)
            {
                mCurrentTexture = mButtonHoveredTexture;
                Input.ClickCursor();
                if (Input.MouseClick())
                    mPressed = true;
            }
            else
                mCurrentTexture = mButtonTexture;
        }

        public void Draw(SpriteBatch mSpriteBatch, Vector2 Position, int Size)
        {
            mPosition = Position;
            mSize = Size;
            mSpriteBatch.Draw(mCurrentTexture, new Rectangle(
                (int)mPosition.X, (int)mPosition.Y, Size, Size), 
                Color.White);
        }
    }
}
