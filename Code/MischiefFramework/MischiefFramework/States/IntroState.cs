using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MischiefFramework.Core;
using MischiefFramework.Cache;

namespace MischiefFramework.States {
    internal class IntroState : IState {
        private SpriteBatch renderTarget;

        private SpriteFont font;

        private List<PlayerInput> inputs;

        public IntroState() {
            renderTarget = new SpriteBatch(Game.device);
            font = Cache.ResourceManager.LoadAsset<SpriteFont>("Fonts/MenuFont");

            inputs = new List<PlayerInput>();

            inputs.Add(new InputGamepad(PlayerIndex.One));
            inputs.Add(new InputGamepad(PlayerIndex.Two));
            inputs.Add(new InputGamepad(PlayerIndex.Three));
            inputs.Add(new InputGamepad(PlayerIndex.Four));
            inputs.Add(new InputKeyboardMouse());

            PlayerInput.SetMouseLock(false);
        }

        public bool Update(GameTime gameTime) {
            foreach (PlayerInput input in inputs) {
                input.Update(gameTime);

                if (input.GetStart()) {
                    Cache.Player.Input = input;
                    StateManager.Push(new MenuState(Game.device));
                    break;
                }
            }

            return false;
        }

        public bool Render(GameTime gameTime) {
            Vector2 corner = Vector2.Zero;
            corner.X = Game.device.Viewport.TitleSafeArea.Left;
            corner.Y = Game.device.Viewport.TitleSafeArea.Top;

            renderTarget.Begin();
            renderTarget.DrawString(font, "Press Enter Or Start to begin!", corner, Color.White);

            corner.X = Game.device.Viewport.Width / 2;
            corner.Y = Game.device.Viewport.Height / 2;

            renderTarget.End();

            return false;
        }

        public bool OnRemove() {
            renderTarget.Dispose();
            return true; //All cleaned up
        }
    }
}
