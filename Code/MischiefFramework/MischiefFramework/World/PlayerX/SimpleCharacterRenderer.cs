using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MischiefFramework.Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MischiefFramework.Cache;
using MischiefFramework.Core.Helpers;
using MischiefFramework.Core;
using SkinnedModel;
using BEPUphysics.Entities.Prefabs;

namespace MischiefFramework.World.PlayerX {

    internal class SimpleCharacterRenderer : Asset, IOpaque {
        private Model m_model;
        public SkinningData skinningData;
        public AnimationPlayer m_animplayer;

        public Cape myCape;

        private Matrix worldFinal = Matrix.Identity;
        private Matrix worldRotation = Matrix.CreateRotationY((float)Math.PI);
        private Matrix worldTransform = Matrix.CreateScale(0.005f);

        //private Matrix internal_offset = Matrix.CreateTranslation(new Vector3(1.5f, 0, 0));

        private Capsule m_body;
        //private int boneIndex = 0;

        public SimpleCharacterRenderer(Capsule Body) {
            m_body = Body;

            myCape = new Cape();

            m_model = ResourceManager.LoadAsset<Model>("Meshes/Character/test walk");
            MeshHelper.ChangeEffectUsedByModel(m_model, Renderer.EffectAnimated);

            // Look up our custom skinning information.
            skinningData = m_model.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            m_animplayer = new AnimationPlayer(skinningData);
            m_animplayer.StartClip(skinningData.AnimationClips["Walk"]);

            Renderer.Add(this);
            AssetManager.AddAsset(this);
        }

        public override void Update(float dt) {
            /*if (state == PlayerState.Die) {
                if (!playedStartDie && !playedEndDie) {
                    m_animplayer.StartClip(skinningData.AnimationClips["End"]);
                    playedStartDie = true;
                } else if (playedStartDie && !playedEndDie) {
                    if (m_animplayer.CurrentTime.TotalSeconds + dt > skinningData.AnimationClips["End"].Duration.TotalSeconds) {
                        m_animplayer.StartClip(skinningData.AnimationClips["LoopEnd"]);
                        playedEndDie = true;
                    }
                }
            } else if (state == PlayerState.Plant) {

            } else if(Math.Abs(Player.Input.GetX()) > 0.05f || Math.Abs(Player.Input.GetY()) > 0.05f) {
                if (Player.playerCharacter.Fatigue < SimpleCharacterController.MAX_FATIGUE / 2 && state != PlayerState.Walk) {
                    m_animplayer.StartClip(skinningData.AnimationClips["walk"]);
                    state = PlayerState.Walk;
                } else if (Player.playerCharacter.Fatigue > SimpleCharacterController.MAX_FATIGUE / 2 && state != PlayerState.Run) {
                    m_animplayer.StartClip(skinningData.AnimationClips["run"]);
                    state = PlayerState.Run;
                }
            } else if(state != PlayerState.Idle) {
                m_animplayer.StartClip(skinningData.AnimationClips["idle"]);
                state = PlayerState.Idle;
            }*/

            m_animplayer.Update(TimeSpan.FromSeconds(dt), true, Matrix.Identity);
        }

        public void RenderOpaque() {
            Matrix[] mats = new Matrix[m_model.Bones.Count];
            Matrix[] bones = m_animplayer.GetSkinTransforms();

            foreach (ModelMesh mesh in m_model.Meshes) {
                foreach (Effect effect in mesh.Effects) {
                    effect.Parameters["Bones"].SetValue(bones);
                }
            }

            Matrix m = worldRotation * Matrix.CreateFromQuaternion(m_body.Orientation) * worldTransform * Matrix.CreateTranslation(m_body.Position - Vector3.Up * 0.923f);

            myCape.RenderOpaque(m, m_animplayer.CurrentTime.TotalSeconds);

            MeshHelper.DrawModel(m, m_model);
        }
    }
}

