using Microsoft.Xna.Framework;

namespace MischiefFramework.Core {
    internal class Camera {
        internal Matrix View;
        internal Matrix Projection;
        internal Matrix ViewProjection;

        internal Vector3 Position;
        internal Vector3 LookAt;
        internal Vector3 Up;

        internal BoundingFrustum Frustum;

        internal Camera(float w, float h) {
            View = Matrix.Identity;
            Projection = Matrix.Identity;
            ViewProjection = Matrix.Identity;

            Position = Vector3.Forward;
            LookAt = Vector3.Zero;
            Up = Vector3.Up;

            //TODO: Store these values rather than hardcode them
            Matrix.CreateOrthographic(w, h, 0.1f, 5000.0f, out Projection);

            Frustum = new BoundingFrustum(Matrix.Identity);

            GenerateMatrices();
        }

        internal void GenerateMatrices() {
            Matrix.CreateLookAt(ref Position, ref LookAt, ref Up, out View);
            Matrix.Multiply(ref View, ref Projection, out ViewProjection);
            Frustum.Matrix = ViewProjection;
        }
    }
}
