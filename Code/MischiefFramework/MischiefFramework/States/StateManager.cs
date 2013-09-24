using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
//#if DEBUG
    using Microsoft.Xna.Framework.Graphics;
//#endif

namespace MischiefFramework.States {
    internal class StateManager {
//#if DEBUG
        private static SpriteBatch debugSB;
        private static SpriteFont debugFNT;
        private static GameTime lastUpdate = new GameTime();
//#endif

        private static List<IState> stateStack;

        internal static void Initilize() {
            stateStack = new List<IState>();

//#if DEBUG
            debugSB = new SpriteBatch(Game.device);
            debugFNT = MischiefFramework.Cache.ResourceManager.LoadAsset<SpriteFont>("Fonts/MenuFont");
//#endif
        }

        internal static void Push(IState newState) {
            lock (stateStack) {
                stateStack.Add(newState);
            }
        }

        internal static void Pop() {
            lock (stateStack) {
                if (stateStack.Count > 0) {
                    stateStack.RemoveAt(stateStack.Count - 1);
                }
            }
        }

        internal static void Remove(IState oldState) {
            lock (stateStack) {
                if (stateStack.Contains(oldState)) {
                    stateStack.Remove(oldState);
                } else {
                    throw new Exception("Removing a state that does not exist in the scene");
                }
            }
        }

        internal static void Update(GameTime gameTime) {
//#if DEBUG
            lastUpdate = gameTime;
//#endif

            for (int i = stateStack.Count - 1; i >= 0; i--) {
                if (!stateStack[i].Update(gameTime)) {
                    break;
                }
            }
        }

        internal static void Render(GameTime gameTime) {
            for (int i = stateStack.Count - 1; i >= 0; i--) {
                if (!stateStack[i].Render(gameTime)) {
                    break;
                }
            }

//#if DEBUG
            debugSB.Begin();
            debugSB.DrawString(debugFNT, Math.Round(1 / lastUpdate.ElapsedGameTime.TotalSeconds).ToString() + "fps (Update)" + (lastUpdate.IsRunningSlowly ? "[SLOW]" : "") + "\n" + Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds).ToString() + "fps (Draw)" + (lastUpdate.IsRunningSlowly ? "[SLOW]" : ""), Vector2.UnitY * ((float)Game.device.Viewport.TitleSafeArea.Bottom - 50.0f), Color.White);
            debugSB.End();
//#endif
        }
    }
}
