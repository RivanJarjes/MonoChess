using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Chess
{
    public enum CurrentCursor
    {
        Normal,
        Click,
        Hover,
        Grab
    }
    class Input
    {
        private static MouseState mMouseState, mLastMouseState;
        private static CurrentCursor mCurrentCursor;
        private static MouseCursor MouseHover = MouseCursor.FromTexture2D(Game1.CursorHover, 10, 10);
        private static MouseCursor MouseGrab = MouseCursor.FromTexture2D(Game1.CursorGrab, 7, 6);

        public static void Update()
        {
            mLastMouseState = mMouseState;
            mMouseState = Mouse.GetState();
        }

        public static Vector2 MouseLocation()
        {
            return new Vector2(mMouseState.X, mMouseState.Y);
        }

        public static bool MouseClick()
        {
            if (mMouseState.LeftButton == ButtonState.Pressed &&
                mLastMouseState.LeftButton != ButtonState.Pressed &&
                MouseLocation().X > 0 && MouseLocation().X < Game1.WindowSize.X &&
                MouseLocation().Y > 0 && MouseLocation().Y < Game1.WindowSize.Y &&
                Game1.WindowActive)
                return true;
            return false;
        }

        public static bool MouseHeld()
        {
            if (mMouseState.LeftButton == ButtonState.Pressed &&
                MouseLocation().X > 0 && MouseLocation().X < Game1.WindowSize.X &&
                MouseLocation().Y > 0 && MouseLocation().Y < Game1.WindowSize.Y &&
                Game1.WindowActive)
                return true;
            return false;
        }
        public static bool MouseRelease()
        {
            if (mMouseState.LeftButton != ButtonState.Pressed &&
                mLastMouseState.LeftButton == ButtonState.Pressed &&
                MouseLocation().X > 0 && MouseLocation().X < Game1.WindowSize.X &&
                MouseLocation().Y > 0 && MouseLocation().Y < Game1.WindowSize.Y &&
                Game1.WindowActive)
                return true;

            if (mMouseState.LeftButton == ButtonState.Pressed &&
                ((MouseLocation().X < 0 || MouseLocation().X > Game1.WindowSize.X ||
                MouseLocation().Y < 0 || MouseLocation().Y > Game1.WindowSize.Y) ||
                !Game1.WindowActive))
                return true;

            return false;
        }
        public static void NormalCursor()
        {
            if (mCurrentCursor != CurrentCursor.Normal)
            {
                Mouse.SetCursor(MouseCursor.Arrow);
                mCurrentCursor = CurrentCursor.Normal;
            }
        }

        public static void ClickCursor()
        {
            if (mCurrentCursor != CurrentCursor.Click)
            {
                Mouse.SetCursor(MouseCursor.Hand);
                mCurrentCursor = CurrentCursor.Click;
            }
        }

        public static void HoverCursor()
        {
            if (mCurrentCursor != CurrentCursor.Hover)
            {
                Mouse.SetCursor(MouseHover);
                mCurrentCursor = CurrentCursor.Hover;
            }
        }
        public static void GrabCursor()
        {
            if (mCurrentCursor != CurrentCursor.Grab)
            {
                Mouse.SetCursor(MouseGrab);
                mCurrentCursor = CurrentCursor.Grab;
            }
        }
    }
}
