using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Core.Interfaces;
using Microsoft.Xna.Framework;
using BEPUphysics;

namespace MischiefFramework.World.Containers {
    internal class WorldController : IDisposable {
        internal static Space space;

        public WorldController() {
            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);
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
