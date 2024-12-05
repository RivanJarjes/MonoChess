using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Chess
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;
        Grid mGrid;
        int GridSize;
        Vector2 GridPosition;
        public static Vector2 WindowSize;
        public static bool WindowActive = true;
        SpriteFont mFont;
        Texture2D WhiteWon;
        Texture2D BlackWon;
        Texture2D Tie;
        public static Texture2D CursorHover;
        public static Texture2D CursorGrab;
        public static Vector2 MouseGridPosition;
        public Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            mGraphics.PreferredBackBufferWidth = 1280;
            mGraphics.PreferredBackBufferHeight = 720;
            mGraphics.ApplyChanges();
            WindowSize.X = mGraphics.PreferredBackBufferWidth;
            WindowSize.Y = mGraphics.PreferredBackBufferHeight;
            Window.Title = "Chess";

            mGrid = new Grid(Content, GraphicsDevice);
            GridSize = 82;
            GridPosition = new Vector2(
                    mGraphics.PreferredBackBufferWidth / 2 - GridSize * 8 / 2,
                    mGraphics.PreferredBackBufferHeight / 2 - GridSize * 8 / 2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            mSpriteBatch = new SpriteBatch(GraphicsDevice);
            mFont = Content.Load<SpriteFont>("SpriteFont");
            WhiteWon = Content.Load<Texture2D>("WhiteWon");
            BlackWon = Content.Load<Texture2D>("BlackWon");
            Tie = Content.Load<Texture2D>("Tie");
            CursorHover = Content.Load<Texture2D>("CursorHover");
            CursorGrab = Content.Load<Texture2D>("CursorGrab");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (mGrid.GameOver && Input.MouseClick())
                mGrid = new Grid(Content, GraphicsDevice);

            WindowActive = this.IsActive;

            Input.Update();
            MouseGridPosition = new Vector2(
                (int)((Input.MouseLocation().X - GridPosition.X) / GridSize),
                (int)((Input.MouseLocation().Y - GridPosition.Y) / GridSize));

            if (Input.MouseLocation().X < GridPosition.X ||
                Input.MouseLocation().Y < GridPosition.Y)
                MouseGridPosition = new Vector2(-1, -1);
            if (Input.MouseLocation().X > GridPosition.X + GridSize * 8 ||
                Input.MouseLocation().Y > GridPosition.Y + GridSize * 8)
                MouseGridPosition = new Vector2(-1, -1);

            if (Input.MouseClick() || Input.MouseRelease())
            {
                mGrid.Selection = MouseGridPosition;
            }
            mGrid.Update();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.FromNonPremultiplied(48, 46, 43, 255));
            mSpriteBatch.Begin(SpriteSortMode.Immediate);
            mGrid.Draw(mSpriteBatch, GridPosition, GridSize);
            
            if (mGrid.GameOver)
            {
                if (mGrid.Winner == Player.White)
                    mSpriteBatch.Draw(WhiteWon, new Vector2(
                        mGraphics.PreferredBackBufferWidth / 2 - WhiteWon.Width / 2,
                        mGraphics.PreferredBackBufferHeight / 2 - WhiteWon.Height / 2),
                        Color.White);
                if (mGrid.Winner == Player.Black)
                    mSpriteBatch.Draw(BlackWon, new Vector2(
                        mGraphics.PreferredBackBufferWidth / 2 - BlackWon.Width / 2,
                        mGraphics.PreferredBackBufferHeight / 2 - BlackWon.Height / 2),
                        Color.White);
                if (mGrid.Winner == Player.None)
                    mSpriteBatch.Draw(Tie, new Vector2(
                        mGraphics.PreferredBackBufferWidth / 2 - BlackWon.Width / 2,
                        mGraphics.PreferredBackBufferHeight / 2 - BlackWon.Height / 2),
                        Color.White);
            }
            mSpriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}