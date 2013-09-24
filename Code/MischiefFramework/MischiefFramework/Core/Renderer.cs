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

        private static SpriteBatch spriteBatch;
        private static Model SphereModel;

        internal static Camera CharacterCamera;

        internal static Texture2D WhitePixel;
        internal static Rectangle FullScreenRect;

        internal static float RenderTime = 0.0f;
        internal static Vector2 HalfPixel = Vector2.Zero * -0.5f;

        private static QuadRenderer quadrenderer;

        internal static void Initialize() {
            CharacterCamera = new Camera();

            if (Effect3D == null) Effect3D = ResourceManager.LoadAsset<Effect>("Shaders/OpaqueShader");
            if (EffectTransparent == null) EffectTransparent = ResourceManager.LoadAsset<Effect>("Shaders/TransparentShader");
            if (EffectPostProcessing == null) EffectPostProcessing = ResourceManager.LoadAsset<Effect>("Shaders/PostProcessing");
            if (EffectAnimated == null) EffectAnimated = ResourceManager.LoadAsset<Effect>("Shaders/AnimatedOpaque");

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

            Vector3 v = Vector3.Zero;
            v.X = -1;
            v.Y = -2;
            v.Z = -3;
            DrawDirectionalLight(v, Color.White);

            Game.device.BlendState = BlendState.Opaque;
            Game.device.DepthStencilState = DepthStencilState.None;
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;

            Game.device.SetRenderTarget(null);

            //Combine everything
            EffectPostProcessing.Parameters["LightMap"].SetValue(LightRT);
            EffectPostProcessing.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, EffectPostProcessing);
            spriteBatch.Draw(ColorRT, FullScreenRect, Color.White);
            spriteBatch.End();

            if (Player.Input.GetJump()) {
                FileStream f0 = new FileStream("C:\\Users\\Paul\\Desktop\\ColourRT.png", FileMode.CreateNew);
                ColorRT.SaveAsPng(f0, 4000, 4000);
                FileStream f1 = new FileStream("C:\\Users\\Paul\\Desktop\\NormalRT.png", FileMode.CreateNew);
                NormalRT.SaveAsPng(f1, 4000, 4000);
                FileStream f2 = new FileStream("C:\\Users\\Paul\\Desktop\\DepthRT.png", FileMode.CreateNew);
                DepthRT.SaveAsPng(f2, 4000, 4000);
            }

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

            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;
            Game.device.DepthStencilState = DepthStencilState.Default;
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
