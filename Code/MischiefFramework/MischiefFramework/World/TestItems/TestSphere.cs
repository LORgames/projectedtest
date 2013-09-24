using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MischiefFramework.Cache;
using MischiefFramework.Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using MischiefFramework.World.Containers;
using MischiefFramework.Core.Helpers;
using MischiefFramework.Core;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;

namespace MischiefFramework.World.TestItems {
    internal class TestSphere : Asset, IOpaque {

        private Sphere body;
        private Model model;

        private Matrix position;
        private Matrix precalculated;

        public TestSphere(bool isStatic, Vector3 position, float size) {
            if(isStatic) {
                body = new Sphere(position, size);
            } else {
                body = new Sphere(position, size, 1.0f);
            }

            WorldController.space.Add(body);

            model = ResourceManager.LoadAsset<Model>("Meshes/TestObjects/ball");
            precalculated = Matrix.CreateScale(size);

            Renderer.Add(this);
            AssetManager.AddAsset(this);

            MeshHelper.ChangeEffectUsedByModel(model, Renderer.Effect3D);
        }

        public override void AsyncUpdate(float dt) {
            position = precalculated * Matrix.CreateFromQuaternion(body.Orientation) * Matrix.CreateTranslation(body.Position);
        }

        public void RenderOpaque() {
            MeshHelper.DrawModel(position, model);
        }

    }
}
