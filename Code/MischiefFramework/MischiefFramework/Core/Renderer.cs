using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using MischiefFramework.Cache;
using MischiefFramework.Core.Interfaces;
using MischiefFramework.Core.Helpers;
using System.IO;

namespace MischiefFramework.Core {
    internal class Renderer {
        //Paul's Effects
        internal static Effect Effect3D;
        internal static Effect EffectTransparent;
        internal static Effect EffectAnimated;
        internal static Effect EffectPostProcessing;

        //Stolen Lighting Effects
        internal static Effect EffectLightsPoint;
        internal static Effect EffectLightsDirectional;
        internal static Effect EffectFXAA;

        //The render bin's LOLOLOL SORT OF! Hah!
        private static List<IHeadsUpDisplay> HeadsUpDisplays = new List<IHeadsUpDisplay>();
        private static List<IShadowLight> Shadows = new List<IShadowLight>();
        private static List<IOpaque> OpaqueObjects = new List<IOpaque>();
        private static List<ITransparent> TransparentObjects = new List<ITransparent>();
        private static List<ILight> Lights = new List<ILight>();

        //The render targets
        internal static RenderTarget2D ColorRT; //color and specular intensity
        internal static RenderTarget2D NormalRT; //normals + specular power
        internal static RenderTarget2D DepthRT; //depth
        internal static RenderTarget2D LightRT; //lighting
        internal static RenderTarget2D PostDrawRT; //Final Render before AA etc

        private static SpriteBatch spriteBatch;
        private static Model SphereModel;

        internal static Camera CharacterCamera;

        internal static Texture2D WhitePixel;
        internal static Rectangle FullScreenRect;

        internal static float RenderTime = 0.0f;
        internal static Vector2 HalfPixel = Vector2.Zero * -0.5f;

        private static QuadRenderer quadrenderer;

        private static bool FXAA_use = true;
        #region FXAA_Settings
            // This effects sub-pixel AA quality and inversely sharpness.
            //   Where N ranges between,
            //     N = 0.50 (default)
            //     N = 0.33 (sharper)
            private static float FXAA_N = 0.40f;

            // Choose the amount of sub-pixel aliasing removal.
            // This can effect sharpness.
            //   1.00 - upper limit (softer)
            //   0.75 - default amount of filtering
            //   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
            //   0.25 - almost off
            //   0.00 - completely off
            private static float FXAA_subPixelAliasingRemoval = 0.75f;

            // The minimum amount of local contrast required to apply algorithm.
            //   0.333 - too little (faster)
            //   0.250 - low quality
            //   0.166 - default
            //   0.125 - high quality 
            //   0.063 - overkill (slower)
            private static float FXAA_edgeTheshold = 0.166f;

            // Trims the algorithm from processing darks.
            //   0.0833 - upper limit (default, the start of visible unfiltered edges)
            //   0.0625 - high quality (faster)
            //   0.0312 - visible limit (slower)
            // Special notes when using FXAA_GREEN_AS_LUMA,
            //   Likely want to set this to zero.
            //   As colors that are mostly not-green
            //   will appear very dark in the green channel!
            //   Tune by looking at mostly non-green content,
            //   then start at zero and increase until aliasing is a problem.
            private static float FXAA_edgeThesholdMin = 0f;

            // This does not effect PS3, as this needs to be compiled in.
            //   Use FXAA_CONSOLE__PS3_EDGE_SHARPNESS for PS3.
            //   Due to the PS3 being ALU bound,
            //   there are only three safe values here: 2 and 4 and 8.
            //   These options use the shaders ability to a free *|/ by 2|4|8.
            // For all other platforms can be a non-power of two.
            //   8.0 is sharper (default!!!)
            //   4.0 is softer
            //   2.0 is really soft (good only for vector graphics inputs)
            private static float FXAA_consoleEdgeSharpness = 8.0f;

            // This does not effect PS3, as this needs to be compiled in.
            //   Use FXAA_CONSOLE__PS3_EDGE_THRESHOLD for PS3.
            //   Due to the PS3 being ALU bound,
            //   there are only two safe values here: 1/4 and 1/8.
            //   These options use the shaders ability to a free *|/ by 2|4|8.
            // The console setting has a different mapping than the quality setting.
            // Other platforms can use other values.
            //   0.125 leaves less aliasing, but is softer (default!!!)
            //   0.25 leaves more aliasing, and is sharper
            private static float FXAA_consoleEdgeThreshold = 0.125f;

            // Trims the algorithm from processing darks.
            // The console setting has a different mapping than the quality setting.
            // This only applies when FXAA_EARLY_EXIT is 1.
            // This does not apply to PS3, 
            // PS3 was simplified to avoid more shader instructions.
            //   0.06 - faster but more aliasing in darks
            //   0.05 - default
            //   0.04 - slower and less aliasing in darks
            // Special notes when using FXAA_GREEN_AS_LUMA,
            //   Likely want to set this to zero.
            //   As colors that are mostly not-green
            //   will appear very dark in the green channel!
            //   Tune by looking at mostly non-green content,
            //   then start at zero and increase until aliasing is a problem.
            private static float FXAA_consoleEdgeThresholdMin = 0f;
        #endregion

        internal static void Initialize() {
            CharacterCamera = new Camera(1, 1);

            if (Effect3D == null) Effect3D = ResourceManager.LoadAsset<Effect>("Shaders/OpaqueShader");
            if (EffectTransparent == null) EffectTransparent = ResourceManager.LoadAsset<Effect>("Shaders/TransparentShader");
            if (EffectPostProcessing == null) EffectPostProcessing = ResourceManager.LoadAsset<Effect>("Shaders/PostProcessing");
            if (EffectAnimated == null) EffectAnimated = ResourceManager.LoadAsset<Effect>("Shaders/AnimatedOpaque");

            if (EffectFXAA == null) EffectFXAA = ResourceManager.LoadAsset<Effect>("Shaders/FXAA");
            
            if (EffectLightsPoint == null) EffectLightsPoint = ResourceManager.LoadAsset<Effect>("Shaders/PointLight");
            if (EffectLightsDirectional == null) EffectLightsDirectional = ResourceManager.LoadAsset<Effect>("Shaders/DirectionalLight");

            if (SphereModel == null) SphereModel = ResourceManager.LoadAsset<Model>("Meshes/Lighting/LightingSphere");

            System.Diagnostics.Debug.Assert(SphereModel != null);

            if (WhitePixel == null) {
                WhitePixel = new Texture2D(Game.device, 1, 1);
                Color[] WhitePixelColourArray = { Color.White };
                WhitePixel.SetData<Color>(WhitePixelColourArray);
            }

            if (quadrenderer == null) {
                quadrenderer = new QuadRenderer();
            }

            int backbufferWidth = Game.device.PresentationParameters.BackBufferWidth;
            int backbufferHeight = Game.device.PresentationParameters.BackBufferHeight;

            FullScreenRect = new Rectangle(0, 0, backbufferWidth, backbufferHeight);

            ColorRT = new RenderTarget2D(Game.device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            NormalRT = new RenderTarget2D(Game.device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            DepthRT = new RenderTarget2D(Game.device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            LightRT = new RenderTarget2D(Game.device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            PostDrawRT = new RenderTarget2D(Game.device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            spriteBatch = new SpriteBatch(Game.device);
        }

        internal static void ClearAll() {
            OpaqueObjects.Clear();
            TransparentObjects.Clear();
            Lights.Clear();
            Shadows.Clear();
            HeadsUpDisplays.Clear();
        }

        internal static void Add(Object obj) {
            if (obj is IOpaque) OpaqueObjects.Add(obj as IOpaque);
            if (obj is ITransparent) TransparentObjects.Add(obj as ITransparent);
            if (obj is ILight) Lights.Add(obj as ILight);
            if (obj is IShadowLight) Shadows.Add(obj as IShadowLight);
            if (obj is IHeadsUpDisplay) HeadsUpDisplays.Add(obj as IHeadsUpDisplay);
        }

        internal static void Remove(Object obj) {
            if (obj is IOpaque) OpaqueObjects.Remove(obj as IOpaque);
            if (obj is ITransparent) TransparentObjects.Remove(obj as ITransparent);
            if (obj is ILight) Lights.Remove(obj as ILight);
            if (obj is IShadowLight) Shadows.Remove(obj as IShadowLight);
            if (obj is IHeadsUpDisplay) HeadsUpDisplays.Remove(obj as IHeadsUpDisplay);
        }

        internal static void Update(GameTime gametime) {
            RenderTime = (float)gametime.TotalGameTime.TotalSeconds;

            if (Player.Input.GetFireLeft()) {
                FXAA_use = !FXAA_use;
            }
        }

        internal static void RenderOpaque_Extern() {
            for (int i = 0; i < OpaqueObjects.Count; i++) {
                OpaqueObjects[i].RenderOpaque();
            }
        }

        internal static void Render() {
            Game.device.DepthStencilState = DepthStencilState.Default;
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;
            Game.device.BlendState = BlendState.Opaque;

            for (int i = 0; i < Shadows.Count; i++) {
                Shadows[i].RenderShadow();
            }

            Game.device.DepthStencilState = DepthStencilState.Default;
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;
            Game.device.BlendState = BlendState.Opaque;

            MeshHelper.RenderCamera = CharacterCamera;
            MeshHelper.EffectID = 0;

            Game.device.SetRenderTargets(ColorRT, NormalRT, DepthRT);

            Game.device.Clear(Color.Black);
            Game.device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            for (int i = 0; i < OpaqueObjects.Count; i++) {
                OpaqueObjects[i].RenderOpaque();
            }

            Game.device.SetRenderTargets(null);

            Game.device.SetRenderTarget(LightRT);
            Game.device.Clear(Color.Transparent);

            Game.device.BlendState = BlendState.AlphaBlend;
            Game.device.DepthStencilState = DepthStencilState.None;

            for (int i = 0; i < Lights.Count; i++) {
                Lights[i].RenderLight();
            }

            Game.device.BlendState = BlendState.Opaque;
            Game.device.DepthStencilState = DepthStencilState.None;
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;

            Game.device.SetRenderTarget(PostDrawRT);

            //Combine everything
            EffectPostProcessing.Parameters["LightMap"].SetValue(LightRT);
            EffectPostProcessing.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, EffectPostProcessing);
            spriteBatch.Draw(ColorRT, FullScreenRect, Color.White);
            spriteBatch.End();


            #region FXAA_DRAW
            Viewport viewport = Game.device.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            EffectFXAA.Parameters["World"].SetValue(Matrix.Identity);
            EffectFXAA.Parameters["View"].SetValue(Matrix.Identity);
            EffectFXAA.Parameters["Projection"].SetValue(halfPixelOffset * projection);
            EffectFXAA.Parameters["InverseViewportSize"].SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
            EffectFXAA.Parameters["ConsoleSharpness"].SetValue(new Vector4(-FXAA_N / viewport.Width, -FXAA_N / viewport.Height, FXAA_N / viewport.Width, FXAA_N / viewport.Height));
            EffectFXAA.Parameters["ConsoleOpt1"].SetValue(new Vector4(-2.0f / viewport.Width, -2.0f / viewport.Height, 2.0f / viewport.Width, 2.0f / viewport.Height));
            EffectFXAA.Parameters["ConsoleOpt2"].SetValue(new Vector4(8.0f / viewport.Width, 8.0f / viewport.Height, -4.0f / viewport.Width, -4.0f / viewport.Height));
            EffectFXAA.Parameters["SubPixelAliasingRemoval"].SetValue(FXAA_subPixelAliasingRemoval);
            EffectFXAA.Parameters["EdgeThreshold"].SetValue(FXAA_edgeTheshold);
            EffectFXAA.Parameters["EdgeThresholdMin"].SetValue(FXAA_edgeThesholdMin);
            EffectFXAA.Parameters["ConsoleEdgeSharpness"].SetValue(FXAA_consoleEdgeSharpness);
            EffectFXAA.Parameters["ConsoleEdgeThreshold"].SetValue(FXAA_consoleEdgeThreshold);
            EffectFXAA.Parameters["ConsoleEdgeThresholdMin"].SetValue(FXAA_consoleEdgeThresholdMin);

            EffectFXAA.CurrentTechnique = EffectFXAA.Techniques[FXAA_use ? "FXAA" : "Standard"];


            Game.device.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, EffectFXAA);
            spriteBatch.Draw(PostDrawRT, Vector2.Zero, Color.White);
            spriteBatch.End();
            #endregion

            //Exit early to avoid expensive shaders if they aren't needed
            if (TransparentObjects.Count > 0) {
                MeshHelper.Tranparent = true;
                Game.device.BlendState = BlendState.AlphaBlend;
                Game.device.DepthStencilState = DepthStencilState.Default;
                for (int i = 0; i < TransparentObjects.Count; i++) {
                    TransparentObjects[i].RenderTransparent();
                }
                Game.device.DepthStencilState = DepthStencilState.None;
                MeshHelper.Tranparent = false;
            }

            spriteBatch.Begin();
            
            for (int i = 0; i < HeadsUpDisplays.Count; i++) {
                HeadsUpDisplays[i].RenderHeadsUpDisplay(spriteBatch);
            }

            spriteBatch.End();

#if DEBUG_PHYSICS
            Game.instance.ModelDrawer.Draw(CharacterCamera.View, CharacterCamera.Projection);
            Game.instance.ConstraintDrawer.Draw(CharacterCamera.View, CharacterCamera.Projection);
#endif
        }

        internal static void DrawDirectionalLight(Vector3 lightDirection, Color color, int techniqueId = 0) {
            EffectLightsDirectional.Parameters["colorMap"].SetValue(ColorRT);
            EffectLightsDirectional.Parameters["normalMap"].SetValue(NormalRT);
            EffectLightsDirectional.Parameters["depthMap"].SetValue(DepthRT);

            EffectLightsDirectional.Parameters["lightDirection"].SetValue(lightDirection);
            EffectLightsDirectional.Parameters["Color"].SetValue(color.ToVector3());

            EffectLightsDirectional.Parameters["halfPixel"].SetValue(HalfPixel);

            EffectLightsDirectional.Parameters["cameraPosition"].SetValue(CharacterCamera.Position);
            EffectLightsDirectional.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(CharacterCamera.View * CharacterCamera.Projection));

            if (EffectLightsDirectional.CurrentTechnique != EffectLightsDirectional.Techniques[techniqueId]) {
                EffectLightsDirectional.CurrentTechnique = EffectLightsDirectional.Techniques[techniqueId];
            }

            EffectLightsDirectional.Techniques[techniqueId].Passes[0].Apply();

            quadrenderer.Render(Vector2.One * -1, Vector2.One);
        }
        
        internal static void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity) {
            //set the G-Buffer parameters
            EffectLightsPoint.Parameters["colorMap"].SetValue(ColorRT);
            EffectLightsPoint.Parameters["normalMap"].SetValue(NormalRT);
            EffectLightsPoint.Parameters["depthMap"].SetValue(DepthRT);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            EffectLightsPoint.Parameters["World"].SetValue(sphereWorldMatrix);
            EffectLightsPoint.Parameters["View"].SetValue(CharacterCamera.View);
            EffectLightsPoint.Parameters["Projection"].SetValue(CharacterCamera.Projection);
            
            //light position
            EffectLightsPoint.Parameters["lightPosition"].SetValue(lightPosition);

            //set the color, radius and Intensity
            EffectLightsPoint.Parameters["Color"].SetValue(color.ToVector3());
            EffectLightsPoint.Parameters["lightRadius"].SetValue(lightRadius);
            EffectLightsPoint.Parameters["lightIntensity"].SetValue(lightIntensity);

            //parameters for specular computations
            EffectLightsPoint.Parameters["cameraPosition"].SetValue(CharacterCamera.Position);
            EffectLightsPoint.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(CharacterCamera.View * CharacterCamera.Projection));
            
            //size of a halfpixel, for texture coordinates alignment
            EffectLightsPoint.Parameters["halfPixel"].SetValue(HalfPixel);
            
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(CharacterCamera.Position, lightPosition);
            
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                Game.device.RasterizerState = RasterizerState.CullClockwise;
            else
                Game.device.RasterizerState = RasterizerState.CullCounterClockwise;

            Game.device.DepthStencilState = DepthStencilState.None;

            EffectLightsPoint.Techniques[0].Passes[0].Apply();
            
            foreach (ModelMesh mesh in SphereModel.Meshes) {
                foreach (ModelMeshPart meshPart in mesh.MeshParts) {
                    Game.device.Indices = meshPart.IndexBuffer;
                    Game.device.SetVertexBuffer(meshPart.VertexBuffer);
                    Game.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

#if DEBUG_PHYSICS
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;
            Game.device.DepthStencilState = DepthStencilState.Default;
#endif
        }

    }

    struct StandardVertex : IVertexType {
        private static VertexDeclaration vertex = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

        public StandardVertex(Vector3 position, Vector3 normal, Vector2 texture) {
            Position = new Vector4(position, 1.0f);
            Normal = normal;
            TextureCoordinate = texture;
        }

        public VertexDeclaration VertexDeclaration {
            get { return vertex; }
        }

        public Vector4 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
    }
}
