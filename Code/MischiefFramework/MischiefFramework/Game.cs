using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MischiefFramework.States;
using MischiefFramework.Cache;
using MischiefFramework.World.Information;

namespace MischiefFramework {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    internal class Game : Microsoft.Xna.Framework.Game {
        internal static GraphicsDeviceManager graphics;
        internal static GraphicsDevice device;

        internal static Game instance;

        internal Game() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            instance = this;

            Window.Title = "Projected Test";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 4000;
            graphics.PreferredBackBufferHeight = 4000;
            graphics.ApplyChanges();

            base.IsMouseVisible = true;

            device = graphics.GraphicsDevice;

            device.DeviceReset += new EventHandler<EventArgs>(device_DeviceReset);

            ResourceManager.SetContent(Content);
            StateManager.Initilize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            StateManager.Push(new LogoScreen());
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            AssetManager.Flush();
            ResourceManager.Flush();
        }

        /// <summary>
        /// Catches exit to allow us to hard shutdown threads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnExiting(Object sender, EventArgs args) {
            base.OnExiting(sender, args);

            if (GameInformation.networkInterface != null)
                GameInformation.networkInterface.Shutdown();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            StateManager.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            StateManager.Render(gameTime);

            base.Draw(gameTime);
        }

        /// <summary>
        /// This is called when the device is reset. SUPER DIRTY FIX
        /// TODO: Clean this up and make it right with the world.
        /// </summary>
        protected void device_DeviceReset(object sender, EventArgs e) {
            for (int i = 0; i < 4; i++) {
                device.VertexSamplerStates[i] = SamplerState.PointClamp;
            }
            for (int i = 0; i < 16; i++) {
                device.SamplerStates[i] = SamplerState.PointClamp;
            }
        }
    }
}
