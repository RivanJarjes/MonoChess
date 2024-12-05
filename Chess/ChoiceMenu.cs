
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Chess
{
    public enum ChoiceStatus
    {
        None,
        Hovered,
        Clicked
    }
    class ChoiceMenu
    {
        private Color mBackgroundColor;
        private Color mChoiceColor;
        private Color mChoiceHoveredColor;
        private Color mChoiceClickedColor;
        private Texture2D mBackgroundTexture;
        private Texture2D mChoiceTexture;
        private Texture2D mChoiceHoveredTexture;
        private Texture2D mChoiceClickedTexture;
        private Vector2 mWindowSize;
        private int mTileSize;
        private int mTileDivider;
        private bool mIsActive;

        public struct Choice
        {
            public ChoiceStatus Status;
            public Texture2D Icon;
            public Choice(Texture2D Icon = null)
            {
                this.Status = ChoiceStatus.None;
                this.Icon = Icon;
            }
        }

        private Choice[] mChoices;
        private int mSelection;

        public bool IsActive
        {
            get { return mIsActive; }
            set { mIsActive = value; }
        }
        public int Selection
        {
            get { return mSelection; }
            set { mSelection = value; }
        }

        public ChoiceMenu(ContentManager Content, GraphicsDevice mGraphics, Vector2 WindowSize, Choice[] Choices = null)
        {
            mIsActive = true;
            mBackgroundColor = Color.FromNonPremultiplied(133, 169, 78, 255);
            mChoiceColor = Color.FromNonPremultiplied(238, 238, 210, 255);
            mChoiceHoveredColor = Color.FromNonPremultiplied(233, 220, 161, 255);
            mChoiceClickedColor = Color.FromNonPremultiplied(219, 219, 168, 255);
            mBackgroundTexture = new Texture2D(mGraphics, 1, 1);
            mBackgroundTexture.SetData(new[] { mBackgroundColor });
            mChoiceTexture = new Texture2D(mGraphics, 1, 1);
            mChoiceTexture.SetData(new[] { mChoiceColor });
            mChoiceHoveredTexture = new Texture2D(mGraphics, 1, 1);
            mChoiceHoveredTexture.SetData(new[] { mChoiceHoveredColor });
            mChoiceClickedTexture = new Texture2D(mGraphics, 1, 1);
            mChoiceClickedTexture.SetData(new[] { mChoiceClickedColor });
            mWindowSize = WindowSize;
            mChoices = Choices;
            if (Choices == null)
                mChoices = new Choice[]
                {
                    new Choice(),
                    new Choice(),
                    new Choice(),
                    new Choice()
                };
            mSelection = 0;
            mTileSize = 64;
            mTileDivider = 12;
        }

        public void Update()
        {
            if (!mIsActive)
                return;

            Vector2 ChoiceMenuSize = new Vector2(
                mChoices.Count() * mTileSize + (mChoices.Count() + 1) * mTileDivider,
                mTileSize + mTileDivider * 2);
            for (int i = 0; i < mChoices.Length; i++)
            {
                Vector2 ChoiceCoord = new Vector2(
                    (int)(mWindowSize.X / 2) - (int)(ChoiceMenuSize.X / 2) + mTileDivider * (i + 1) + mTileSize * (i),
                    (int)(mWindowSize.Y / 2) - (int)(ChoiceMenuSize.Y / 2) + mTileDivider);

                if (Input.MouseLocation().X >= ChoiceCoord.X &&
                    Input.MouseLocation().X < ChoiceCoord.X + mTileSize &&
                    Input.MouseLocation().Y >= ChoiceCoord.Y &&
                    Input.MouseLocation().Y < ChoiceCoord.Y + mTileSize)
                {
                    Input.ClickCursor();

                    if (Input.MouseHeld())
                        mChoices[i].Status = ChoiceStatus.Clicked;
                    else
                        mChoices[i].Status = ChoiceStatus.Hovered;

                    if (Input.MouseRelease())
                        mSelection = i + 1;
                }else
                {
                    mChoices[i].Status = ChoiceStatus.None;
                }
            }
        }

        public void Draw(SpriteBatch mSpriteBatch)
        {
            if (!mIsActive)
                return;

            Vector2 ChoiceMenuSize = new Vector2(
                mChoices.Count() * mTileSize + (mChoices.Count() + 1) * mTileDivider,
                mTileSize + mTileDivider * 2);

            mSpriteBatch.Draw(mBackgroundTexture, new Rectangle
                ((int)(mWindowSize.X / 2) - (int)(ChoiceMenuSize.X / 2),
                (int)(mWindowSize.Y / 2) - (int)(ChoiceMenuSize.Y / 2),
                (int)ChoiceMenuSize.X,
                (int)ChoiceMenuSize.Y), Color.White);

            for (int i = 0; i < mChoices.Count(); i++)
            {
                Texture2D Texture = mChoiceTexture;
                switch(mChoices[i].Status)
                {
                    case ChoiceStatus.None:
                        Texture = mChoiceTexture;
                        break;
                    case ChoiceStatus.Hovered:
                        Texture = mChoiceHoveredTexture;
                        break;
                    case ChoiceStatus.Clicked:
                        Texture = mChoiceClickedTexture;
                        break;
                }

                mSpriteBatch.Draw(Texture, new Rectangle
                    ((int)(mWindowSize.X / 2) - (int)(ChoiceMenuSize.X / 2) + mTileDivider * (i + 1) + mTileSize * (i),
                    (int)(mWindowSize.Y / 2) - (int)(ChoiceMenuSize.Y / 2) + mTileDivider,
                    (int)mTileSize,
                    (int)mTileSize), Color.White);

                if (mChoices[i].Icon != null)
                    mSpriteBatch.Draw(mChoices[i].Icon, new Rectangle
                        ((int)(mWindowSize.X / 2) - (int)(ChoiceMenuSize.X / 2) + mTileDivider * (i + 1) + mTileSize * (i),
                        (int)(mWindowSize.Y / 2) - (int)(ChoiceMenuSize.Y / 2) + mTileDivider,
                        (int)mTileSize,
                        (int)mTileSize), Color.White);
            }
        }
    }
}
