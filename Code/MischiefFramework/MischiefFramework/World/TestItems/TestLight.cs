using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Core;
using MischiefFramework.Core.Interfaces;
using Microsoft.Xna.Framework;

namespace MischiefFramework.World.TestItems {
    internal class TestLight : ILight {
        public TestLight() {
            Renderer.Add(this);
        }

        public void RenderLight() {
            Renderer.DrawPointLight(Vector3.Up, Color.Blue, 25.0f, 10.0f);
        }
    }
}
