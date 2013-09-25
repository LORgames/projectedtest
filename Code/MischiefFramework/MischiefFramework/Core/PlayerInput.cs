using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MischiefFramework.Core {
    public abstract class PlayerInput {
        //Mouse locking to center of screen
        protected static bool locked = true;

        public static void SetMouseLock(bool lockState) {
            locked = lockState;
            Game.instance.IsMouseVisible = !locked;
        }

        // Actions
        protected bool stateJump;
        protected bool stateFireLeft;
        protected bool stateFireRight;
        protected bool stateEquipLeft;
        protected bool stateEquipRight;

        // Commands
        protected bool stateStart = false;
        protected bool stateBack = false;
        protected bool stateMenuBack = false;

        // Stick directions
        protected float dx = 0.0f;
        protected float dy = 0.0f;
        protected float ax = 0.0f;
        protected float ay = 0.0f;
        protected Vector2 cursorPosition = Vector2.Zero;

        // Who am I?
        protected PlayerIndex myPlayer;

        // Command Getters
        public bool GetStart() { return stateStart; }
        public bool GetBack() { return stateBack; }
        public bool GetMenuBack() { return stateMenuBack; }

        // Action Getters
        public bool GetJump() { return stateJump; }

        public bool GetFireLeft() { return stateFireLeft; }
        public bool GetFireRight() { return stateFireRight; }

        public bool GetEquipLeft() { return stateEquipLeft; }
        public bool GetEquipRight() { return stateEquipRight; }

        // Get Coordinate things
        public float GetX() { return dx; }
        public float GetY() { return dy; }
        public float AimX() { return ax; }
        public float AimY() { return ay; }

        internal Vector2 GetCursorPosition() { return cursorPosition; }

        // Functions that need to be called
        public abstract void Update(GameTime dt);
        public virtual void Rumble() { }
        public virtual void Rumble(float sine_motor, float cos_motor, float time) { }
    }

    public class InputGamepad : PlayerInput {
        private TimeSpan vibration_time_remaining;

        public InputGamepad(PlayerIndex myPlayer) {
            this.myPlayer = myPlayer;
            vibration_time_remaining = TimeSpan.Zero;
        }

        public override void Update(GameTime dt) {
            GamePadState currentGamePadState = GamePad.GetState(myPlayer);

            if (vibration_time_remaining >= TimeSpan.Zero) {
                vibration_time_remaining -= dt.ElapsedGameTime;
                if (vibration_time_remaining < TimeSpan.Zero) {
                    RumbleOff();
                }
            }

            stateStart = currentGamePadState.IsButtonDown(Buttons.Start);
            stateBack = currentGamePadState.IsButtonDown(Buttons.Back);
            stateMenuBack = currentGamePadState.IsButtonDown(Buttons.B);

            stateJump = currentGamePadState.IsButtonDown(Buttons.A);
            stateFireLeft = currentGamePadState.IsButtonDown(Buttons.LeftTrigger);
            stateFireRight = currentGamePadState.IsButtonDown(Buttons.RightTrigger);

            stateEquipLeft = currentGamePadState.IsButtonDown(Buttons.LeftShoulder);
            stateEquipRight = currentGamePadState.IsButtonDown(Buttons.RightShoulder);

            dx = currentGamePadState.ThumbSticks.Left.X;
            dy = currentGamePadState.ThumbSticks.Left.Y;
            ax = currentGamePadState.ThumbSticks.Right.X;
            ay = currentGamePadState.ThumbSticks.Right.Y;
        }

        public override void Rumble() {
            Rumble(1.0f, 1.0f, 0.25f);
        }

        public override void Rumble(float sine_motor, float cos_motor, float time) {
            vibration_time_remaining = TimeSpan.FromSeconds(time);
            GamePad.SetVibration(myPlayer, sine_motor, cos_motor);
        }

        private void RumbleOff() {
            Rumble(0.0f, 0.0f, 0.0f);
        }
    }

    public class InputKeyboardMouse : PlayerInput {
        private int prevX;
        private int prevY;

        public InputKeyboardMouse() {
            MouseState currentMouseState = Mouse.GetState();

            prevX = currentMouseState.X;
            prevY = currentMouseState.Y;
        }

        public override void Update(GameTime dt) {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            //Update states
            stateStart = currentKeyboardState.IsKeyDown(Keys.Enter);
            stateBack = currentKeyboardState.IsKeyDown(Keys.Escape);
            stateMenuBack = currentKeyboardState.IsKeyDown(Keys.Escape);

            stateJump = currentKeyboardState.IsKeyDown(Keys.Space);

            stateFireLeft = currentMouseState.LeftButton == ButtonState.Pressed;
            stateFireRight = currentMouseState.RightButton == ButtonState.Pressed;

            stateEquipLeft = currentKeyboardState.IsKeyDown(Keys.Q);
            stateEquipRight = currentKeyboardState.IsKeyDown(Keys.E);

            dx = 0.0f;
            dy = 0.0f;
            ax = 0.0f;
            ay = 0.0f;

            if (currentKeyboardState.IsKeyDown(Keys.D)) {
                dx = 1.0f;
            } else if (currentKeyboardState.IsKeyDown(Keys.A)) {
                dx = -1.0f;
            }

            if (currentKeyboardState.IsKeyDown(Keys.W)) {
                dy = 1.0f;
            } else if (currentKeyboardState.IsKeyDown(Keys.S)) {
                dy = -1.0f;
            }

            //ax = ((float)(currentMouseState.X - prevX) / 10.0f);
            //ay = -((float)(currentMouseState.Y - prevY) / 10.0f);

            ax = ((float)currentMouseState.X / Game.device.ScissorRectangle.Width)*2 - 1;
            ay = ((float)currentMouseState.Y / Game.device.ScissorRectangle.Height)*2 - 1;

            prevX = currentMouseState.X;
            prevY = currentMouseState.Y;

            // Update mouse
            cursorPosition.X = currentMouseState.X;
            cursorPosition.Y = currentMouseState.Y;

            if (locked) {
                //TODO: Don't hardcode res
                prevX = 1280 / 2;
                prevY = 800 / 2;
                Mouse.SetPosition(prevX, prevY);
            }
        }
    }
}
