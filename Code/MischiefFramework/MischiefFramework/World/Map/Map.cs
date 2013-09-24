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

namespace MischiefFramework.World.Map {
    internal class Map : Asset, IOpaque {
        private Model model;

        private Matrix position;
        private Matrix precalculated;

        public Map() {
            model = ResourceManager.LoadAsset<Model>("Map/Basic Geometry");

            Renderer.Add(this);
            AssetManager.AddAsset(this);

            position = Matrix.Identity;

            MeshHelper.ChangeEffectUsedByModel(model, Renderer.Effect3D);
        }

        public void RenderOpaque() {
            MeshHelper.DrawModel(position, model);
        }
    }
}
