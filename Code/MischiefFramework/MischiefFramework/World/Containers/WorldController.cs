using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Core.Interfaces;
using Microsoft.Xna.Framework;
using BEPUphysics;
using MischiefFramework.World.PlayerX;
using MischiefFramework.World.Map;
using MischiefFramework.World.TestItems;

namespace MischiefFramework.World.Containers {
    internal class WorldController : IDisposable {
        internal static Space space;

        public WorldController() {
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

            new Map.Map();
            new SimpleCharacterControllerInput(space);

            new Sun();

            new TestBox(new Vector3(2, 0.5f, 0), 1, space);
            new TestBox(new Vector3(4, 0.5f, 0), 1, space);
            new TestBox(new Vector3(-3, 0.5f, 0), 1, space);
        }

        public void Update(float dt) {
            //Do nothing?
            space.Update();
        }

        public void Dispose() {
            space.Dispose();
            space = null;
        }
    }
}
