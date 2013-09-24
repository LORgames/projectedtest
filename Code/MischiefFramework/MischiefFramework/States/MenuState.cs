using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MischiefFramework.Cache;

namespace MischiefFramework.States {
    class MenuState : IState {
        private SpriteBatch renderTarget;
        private SpriteFont font;

        private Core.Helpers.MenuHelper menu;

        private delegate bool ActiveDelegate();
        private delegate void StartDelegate();
        private delegate void BackDelegate();

        public MenuState(GraphicsDevice device) {
            renderTarget = new SpriteBatch(device);
            font = ResourceManager.LoadAsset<SpriteFont>("Fonts/MenuFont");

            menu = new Core.Helpers.MenuHelper(device.Viewport, Core.Helpers.Positions.CENTER, new BackDelegate(Quit));
            menu.AddTextMenuItem("Play", ref font, Color.White, Color.Red, new StartDelegate(PlayGame));
            menu.AddTextMenuItem("Settings", ref font, Color.White, Color.Red, new StartDelegate(Settings));
            menu.AddTextMenuItem("Quit", ref font, Color.White, Color.Red, new StartDelegate(Quit));
            menu.Update(0f);
        }

        public void PlayGame() {
            StateManager.Push(new PlayingState());
        }

        public void Settings() {
            //StateManager.Push(new SettingsState(Game.device));
        }

        public void Quit() {
            Game.instance.Exit();
        }

        public bool Update(GameTime gameTime) {
            Player.Input.Update(gameTime);
            menu.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            return false;
        }

        public bool Render(GameTime gameTime) {
            menu.Render(renderTarget);
            return false;
        }

        public bool OnRemove() {
            renderTarget.Dispose();
            return true; //All cleaned up
        }
    }
}
