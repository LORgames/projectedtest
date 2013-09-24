using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MischiefFramework.Cache;

namespace MischiefFramework.States {
    internal class LogoScreen : IState {
        private Texture2D logo;
        private SpriteBatch sb;

        private float displayTime = 2.5f;

        public LogoScreen() {
            #if DEBUG
                displayTime = 0.0f;            
            #endif
            logo = ResourceManager.LoadAsset<Texture2D>("HUD/LOR logo");
            sb = new SpriteBatch(Game.device);
        }

        public bool Update(GameTime gt) {
            displayTime -= (float)gt.ElapsedGameTime.TotalSeconds;

            if (displayTime < 0.0f) {
                StateManager.Remove(this);
                StateManager.Push(new IntroState());
            }

            return false;
        }

        public bool Render(GameTime gt) {
            Game.device.Clear(Color.White);
            
            sb.Begin();

            Vector2 position = Vector2.Zero;
            position.X = (Game.device.Viewport.Width-logo.Width)/2;
            position.Y = (Game.device.Viewport.Height-logo.Height)/2;

            sb.Draw(logo, position, Color.White);

            sb.End();

            return false;
        }

        public bool OnRemove() {
            sb.Dispose();
            logo.Dispose();

            return false;
        }

    }
}
