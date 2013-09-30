using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Cache;
using MischiefFramework.Core.Interfaces;
using MischiefFramework.Core;
using Microsoft.Xna.Framework;
using MischiefFramework.Core.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace MischiefFramework.World.Map {
    internal class Sun : Asset, IShadowLight {
        float time = 3.0f;
        bool night = false;

        Color Colour;

        Camera x;
        RenderTarget2D ShadowRT; //lighting

        public Sun() {
            x = new Camera(16, 16, 10f, 60);
            x.LookAt = Vector3.Zero;
            x.Up = Vector3.Up;
            x.GenerateMatrices();

            Colour = Color.White;

            ShadowRT = new RenderTarget2D(Game.device, 2048, 2048, false, SurfaceFormat.Single, DepthFormat.Depth24);

            AssetManager.AddAsset(this);
            Renderer.Add(this);
        }

        public override void AsyncUpdate(float dt) {
            //time += dt;
            //time = 6;

            if(time > 12) { time -= 12; night = !night; };

            float timeDeg = time / 12 * (float)Math.PI;

            x.Position = Vector3.Up * 40 * (float)Math.Sin(timeDeg) + Vector3.Right * 40 * (float)Math.Cos(timeDeg);
            x.GenerateMatrices();

            if (night) {
                Colour = Color.Navy;
            } else {
                Colour = new Color(255, 238 - 136 * (float)Math.Cos(timeDeg), 255 * (float)Math.Sin(timeDeg));
            }
        }

        public void RenderShadow() {
            MeshHelper.EffectID = 1;
            MeshHelper.RenderCamera = x;

            Game.device.SetRenderTarget(ShadowRT);

            Game.device.Clear(Color.Black);
            Game.device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            Renderer.RenderOpaque_Extern();
        }

        public void RenderLight() {
            Renderer.EffectLightsDirectional.Parameters["shadowMap"].SetValue(ShadowRT);
            Renderer.EffectLightsDirectional.Parameters["lightViewProjection"].SetValue(x.ViewProjection);

            Renderer.DrawDirectionalLight(Vector3.Down, Colour, 1);
        }
    }
}
