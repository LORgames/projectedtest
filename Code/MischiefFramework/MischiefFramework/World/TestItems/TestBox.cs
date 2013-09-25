using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Cache;
using MischiefFramework.Core.Interfaces;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BEPUphysics;
using MischiefFramework.Core;
using MischiefFramework.Core.Helpers;
using BEPUphysics.Entities.Prefabs;

namespace MischiefFramework.World.TestItems {
    internal class TestBox : Asset, IOpaque {
        private readonly Entity mesh;

        private Model cube;
        private Matrix scale;
        private Matrix premul;

        public Matrix GraphicsTransform { get; set; }

        public TestBox(Vector3 position, float edgeLength, Space space) {
            cube = ResourceManager.LoadAsset<Model>("Meshes/TestObjects/Cube");

            Matrix.CreateScale(edgeLength/2.0f, out scale);

            mesh = new Box(position, edgeLength, edgeLength, edgeLength, 1000.0f);
            space.Add(mesh);

            mesh.Tag = this;
            mesh.CollisionInformation.Tag = this;

            MeshHelper.ChangeEffectUsedByModel(cube, Renderer.Effect3D);

            AssetManager.AddAsset(this);
            Renderer.Add(this);
        }

        public override void AsyncUpdate(float dt) {
            premul = scale * mesh.WorldTransform;
        }

        public void RenderOpaque () {
            if (mesh != null && cube != null) {
                MeshHelper.DrawModel(premul, cube);
            }
        }
    }
}
