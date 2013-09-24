using System;
using System.Collections.Generic;
using MischiefFramework.Cache;
using MischiefFramework.Core;
using MischiefFramework.Core.Interfaces;
using Microsoft.Xna.Framework;
using System.Threading;
using MischiefFramework.Networking;
using MischiefFramework.World.Information;
using MischiefFramework.World.Containers;
using MischiefFramework.World.TestItems;
using MischiefFramework.World.Map;

namespace MischiefFramework.States {
    internal class PlayingState : IState {
        private Camera mainCam;

        //Camera stats
        private float cameraX = (float)Math.PI / 4.0f;  // XZ Angle
        private float cameraY = (float)Math.PI / 6.0f;  // Y Angle
        private float cameraZoom = 1500.0f;               // Camera Zoom
        private float cameraOffsetX = 0.0f;             // Where the camera is looking in X
        private float cameraOffsetY = 2.0f;             // where the camera is looking in Y
        private float cameraOffsetZ = 0.0f;             // where the camera is looking in Z

        private WorldController worldController;

        public PlayingState() {
            Renderer.Initialize();

            mainCam = new Camera();
            Renderer.CharacterCamera = mainCam;

            MischiefFramework.Core.Renderer.Add(new MischiefFramework.World.Information.InfoPanel());

            PlayerInput.SetMouseLock(true);

            worldController = new WorldController();

            //TestController.CreateTest();
            new Map();
        }

        public bool Update(GameTime gameTime) {
            Renderer.Update(gameTime);
            Player.Input.Update(gameTime);

            //cameraX += Player.Input.AimX();
            //cameraY += Player.Input.AimY();

            cameraZoom = Math.Max(cameraZoom, 6.0f);
            cameraY = Math.Min(Math.Max(cameraY, (float)Math.PI/6.0f), (float)Math.PI/2.01f);

            //cameraOffsetX -= Player.Input.GetY() * (float)Math.Cos(cameraX) - Player.Input.GetX() * (float)Math.Sin(cameraX);
            //cameraOffsetZ -= Player.Input.GetY() * (float)Math.Sin(cameraX) + Player.Input.GetX() * (float)Math.Cos(cameraX);

            mainCam.LookAt.X = cameraOffsetX;
            mainCam.LookAt.Y = cameraOffsetY;
            mainCam.LookAt.Z = cameraOffsetZ;

            mainCam.Position.X = cameraZoom * (float)(Math.Cos(cameraX) * Math.Cos(cameraY)) + cameraOffsetX;
            mainCam.Position.Y = cameraZoom * (float)(Math.Sin(cameraY)) + cameraOffsetY;
            mainCam.Position.Z = cameraZoom * (float)(Math.Sin(cameraX) * Math.Cos(cameraY)) + cameraOffsetZ;
            mainCam.GenerateMatrices();

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
