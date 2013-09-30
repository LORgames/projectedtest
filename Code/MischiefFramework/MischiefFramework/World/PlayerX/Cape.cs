using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MischiefFramework.Core.Interfaces;
using MischiefFramework.Cache;
using MischiefFramework.Core.Helpers;
using MischiefFramework.Core;
using Microsoft.Xna.Framework;

namespace MischiefFramework.World.PlayerX {
    internal class Cape {
        private Model[] QCModels;
        private const double DT = 1f / 24.0f;

        public Cape() {
            QCModels = new Model[28];

            for (int i = 0; i < 28; i++) {
                string f = "Meshes/Character/Cape_OBJ_Seq/Cape." + (i+1).ToString("D4");
                QCModels[i] = ResourceManager.LoadAsset<Model>(f);
                MeshHelper.ChangeEffectUsedByModel(QCModels[i], Renderer.Effect3D);
            }
        }

        public void RenderOpaque(Matrix m, double currentTime) {
            int frameID = (int)Math.Round(currentTime / DT);

            MeshHelper.DrawModel(m, QCModels[frameID]);
        }
    }
}
