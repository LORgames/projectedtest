using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MischiefFramework.Cache;
using MischiefFramework.Core;
using MischiefFramework.Core.Interfaces;
using MischiefFramework.Core.Helpers;
using BEPUphysics.DataStructures;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using MischiefFramework.World.Containers;

namespace MischiefFramework.World.Map {
    internal class Map : Asset, IOpaque {
        private Model model;
        private Effect effect;
        private Texture2D texture;

        private Matrix position;
        private Matrix precalculated;

        public Map() {
            model = ResourceManager.LoadAsset<Model>("Map/Basic Geometry");
            effect = ResourceManager.LoadAsset<Effect>("Shaders/GroundShader");
            texture = ResourceManager.LoadAsset<Texture2D>("Map/Test");

            Renderer.Add(this);
            AssetManager.AddAsset(this);

            position = Matrix.Identity;

            MeshHelper.ChangeEffectUsedByModel(model, effect, false);
            effect.Parameters["Texture"].SetValue(texture);
            effect.Parameters["TextureEnabled"].SetValue(true);


            Camera c = new Camera(40, 40);

            //Camera stats
            float cameraX = (float)Math.PI / 4.0f;  // XZ Angle
            float cameraY = (float)Math.PI / 6.0f;  // Y Angle
            float cameraZoom = 6.0f;                // Camera Zoom
            float cameraOffsetX = 0.0f;             // Where the camera is looking in X
            float cameraOffsetY = 2.0f;             // where the camera is looking in Y
            float cameraOffsetZ = 0.0f;             // where the camera is looking in Z

            cameraY = Math.Min(Math.Max(cameraY, (float)Math.PI / 6.0f), (float)Math.PI / 2.01f);

            c.LookAt.X = cameraOffsetX;
            c.LookAt.Y = cameraOffsetY;
            c.LookAt.Z = cameraOffsetZ;

            c.Position.X = cameraZoom * (float)(Math.Cos(cameraX) * Math.Cos(cameraY)) + cameraOffsetX;
            c.Position.Y = cameraZoom * (float)(Math.Sin(cameraY)) + cameraOffsetY;
            c.Position.Z = cameraZoom * (float)(Math.Sin(cameraX) * Math.Cos(cameraY)) + cameraOffsetZ;
            c.GenerateMatrices();

            effect.Parameters["CameraViewProjection"].SetValue(c.ViewProjection);

            Vector3[] vertices;
            int[] indices;
            TriangleMesh.GetVerticesAndIndicesFromModel(model, out vertices, out indices);
            var mesh = new StaticMesh(vertices, indices, new AffineTransform(new Vector3(0, -40, 0)));
            WorldController.space.Add(mesh);
        }

        public void RenderOpaque() {
            MeshHelper.DrawModel(position, model);
        }
    }
}
