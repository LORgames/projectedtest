using System;
using Microsoft.Xna.Framework;
using MischiefFramework.Cache;
using MischiefFramework.Core;
using MischiefFramework.World.Containers;

#if DEBUG_PHYSICS
using BEPUphysics.Entities;
#endif

namespace MischiefFramework.States {
    internal class PlayingState : IState {
        private Camera mainCam;

        private WorldController worldController;

        public PlayingState() {
            Renderer.Initialize();

            //mainCam = new Camera(16, 9);
            //mainCam = new Camera(12, 6.75f);
            //mainCam = new Camera(8, 4.5f);
            mainCam = new Camera(4, 2.25f);
            //mainCam = new Camera(2, 1.125f);
            //mainCam = new Camera(1, 0.5625f);
            Renderer.CharacterCamera = mainCam;

            MischiefFramework.Core.Renderer.Add(new MischiefFramework.World.Information.InfoPanel());

            worldController = new WorldController();

#if DEBUG_PHYSICS
            foreach (Entity e in WorldController.space.Entities) {
                if (e.Tag != null && e.Tag.ToString() != "noDisplayObject") {
                    Game.instance.ModelDrawer.Add(e);
                } else //Remove the now unnecessary tag.
                    e.Tag = null;
            }
#endif
        }

        public bool Update(GameTime gameTime) {
            Renderer.Update(gameTime);
            Player.Input.Update(gameTime);

#if DEBUG_PHYSICS
            Game.instance.ConstraintDrawer.Update();
            Game.instance.ModelDrawer.Update();
#endif

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //TODO: Update things here
            worldController.Update(dt);
            AssetManager.Update(dt);

            return false;
        }

        public bool Render(GameTime gameTime) {
            Renderer.Render();
            return false;
        }

        public bool OnRemove() {
            worldController.Dispose();
            ResourceManager.Flush();
            AssetManager.Flush();
            Renderer.ClearAll();

            return true;
        }
    }
}
