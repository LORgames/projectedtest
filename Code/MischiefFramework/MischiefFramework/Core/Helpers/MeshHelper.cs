using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MischiefFramework.Core.Helpers {
    class MeshHelper {
        internal static Camera RenderCamera;
        internal static int EffectID;

        internal static bool Tranparent = false;

        internal static void ChangeEffectUsedByModel(Model model, Effect replacementEffect, bool clone = true) {
            // Table mapping the original effects to our replacement versions.
            Dictionary<Effect, Effect> effectMapping = new Dictionary<Effect, Effect>();

            if (model == null || replacementEffect == null) {
                throw new Exception("This Model Cannot Be Converted Because Something Is Wrong! (@TODO: Better errors)");
            }

            if (!(model.Meshes[0].Effects[0] is BasicEffect) && !(model.Meshes[0].Effects[0] is SkinnedEffect)) {
                return;
            }

            foreach (ModelMesh mesh in model.Meshes) {
                // Scan over all the effects currently on the mesh.
                foreach (Effect oldEffect in mesh.Effects) {
                    if(oldEffect is BasicEffect) {
                        // If we haven't already seen this effect...
                        if (!effectMapping.ContainsKey(oldEffect)) {
                            // Make a clone of our replacement effect. We can't just use
                            // it directly, because the same effect might need to be
                            // applied several times to different parts of the model using
                            // a different texture each time, so we need a fresh copy each
                            // time we want to set a different texture into it.
                            Effect newEffect = clone?replacementEffect.Clone():replacementEffect;
                            BasicEffect oldBasicEffect = oldEffect as BasicEffect;

                            // Copy across the texture from the original effect.
                            newEffect.Parameters["Texture"].SetValue(oldBasicEffect.Texture);
                            newEffect.Parameters["TextureEnabled"].SetValue(oldBasicEffect.TextureEnabled);
                            newEffect.Parameters["MaterialAmbientColor"].SetValue(new Vector4(oldBasicEffect.DiffuseColor, oldBasicEffect.Alpha));
                            newEffect.Parameters["MaterialDiffuseColor"].SetValue(new Vector4(oldBasicEffect.DiffuseColor, oldBasicEffect.Alpha));

                            newEffect.Parameters["MaterialSpecularColor"].SetValue(new Vector4(oldBasicEffect.SpecularColor, oldBasicEffect.Alpha));
                            newEffect.Parameters["MaterialSpecularPower"].SetValue(oldBasicEffect.SpecularPower);

                            effectMapping.Add(oldEffect, newEffect);
                        }
                    } else if (oldEffect is SkinnedEffect) {
                        // If we haven't already seen this effect...
                        if (!effectMapping.ContainsKey(oldEffect)) {
                            Effect newEffect = replacementEffect.Clone();
                            SkinnedEffect oldSkinnedEffect = oldEffect as SkinnedEffect;

                            newEffect.Parameters["Texture"].SetValue(oldSkinnedEffect.Texture);
                            newEffect.Parameters["MaterialAmbientColor"].SetValue(new Vector4(oldSkinnedEffect.DiffuseColor, 1f));
                            newEffect.Parameters["MaterialDiffuseColor"].SetValue(new Vector4(oldSkinnedEffect.DiffuseColor, 1f));
                            newEffect.Parameters["MaterialSpecularColor"].SetValue(new Vector4(oldSkinnedEffect.SpecularColor, 1f));
                            newEffect.Parameters["MaterialSpecularPower"].SetValue(oldSkinnedEffect.SpecularPower);

                            effectMapping.Add(oldEffect, newEffect);
                        }
                    }
                }

                // Now that we've found all the effects in use on this mesh,
                // update it to use our new replacement versions.
                foreach (ModelMeshPart meshPart in mesh.MeshParts) {
                    meshPart.Effect = effectMapping[meshPart.Effect];
                }
            }
        }

        internal static void DrawModel(Matrix world, Model model) {
            // Look up the bone transform matrices.
            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model.
            foreach (ModelMesh mesh in model.Meshes) {
                Matrix localWorld;
                Matrix WorldViewProjection;
                Matrix WorldInverse;

                Matrix.Multiply(ref transforms[mesh.ParentBone.Index], ref world, out localWorld);

                if (!RenderCamera.Frustum.Intersects(mesh.BoundingSphere.Transform(localWorld))) continue;

                Matrix.Multiply(ref localWorld, ref RenderCamera.ViewProjection, out WorldViewProjection);
                Matrix.Invert(ref localWorld, out WorldInverse);

                for (int i = 0; i < mesh.Effects.Count; i++) {
                    Effect effect = mesh.Effects[i];

                    // Specify which effect technique to use.
                    effect.CurrentTechnique = effect.Techniques[EffectID];

                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(RenderCamera.View);
                    effect.Parameters["Projection"].SetValue(RenderCamera.Projection);
                    effect.Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
                    effect.Parameters["WorldInverseTranspose"].SetValueTranspose(WorldInverse);

                    if (Tranparent) {
                        effect.Parameters["DepthTexture"].SetValue(Renderer.DepthRT);
                        effect.Parameters["PreRenderedScene"].SetValue(Renderer.ColorRT);
                    }
                }

                try {
                    mesh.Draw();
                } catch (System.Exception e) {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }
        }

        internal static void DrawBuffers(Effect effect, Matrix world, int startVertex, int numVertex, int startIndex, int totalPrimitives) {
            // Draw the model.
            Matrix WorldViewProjection;
            Matrix WorldInverse;

            Matrix.Multiply(ref world, ref RenderCamera.ViewProjection, out WorldViewProjection);
            Matrix.Invert(ref world, out WorldInverse);

            // Specify which effect technique to use.
            effect.CurrentTechnique = effect.Techniques[EffectID];

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(RenderCamera.View);
            effect.Parameters["Projection"].SetValue(RenderCamera.Projection);
            effect.Parameters["WorldViewProjection"].SetValue(WorldViewProjection);
            effect.Parameters["WorldInverseTranspose"].SetValueTranspose(WorldInverse);

            effect.CurrentTechnique.Passes[0].Apply();
            Game.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, startVertex, numVertex, startIndex, totalPrimitives);
        }
    }
}
