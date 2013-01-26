using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class Camera
    {
        public Vector3 Target { get; set; }
        public Vector3 Position { get; set; }
        public float FOV { get; set; }
        public float AspectRatio { get; set; }
        public float NearPlane { get; set; }
        public float FarPlane { get; set; }
        public Vector3 UpVector { get; set; }
        public Matrix ViewMatrix { get; set;}
        public Matrix ProjectionMatrix { get; set; }
        public Vector3 Velocity { get; set; }
        private Vector3 lastPosition = Vector3.Zero;

        public Camera(Vector3 target, Vector3 position, float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            UpVector = Vector3.Up;
            Target = target;
            Position = position;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            AspectRatio = aspectRatio;
            FOV = fov;
            Velocity = Vector3.Zero;
            UpdateViewMatrix();
            UpdateProjectionMatrix();
        }

        public virtual void Update(GameTime time)
        {
            Velocity = (Position - lastPosition) / (float)time.ElapsedGameTime.TotalSeconds;
            lastPosition = Position;
            UpdateViewMatrix();
        }

        public void UpdateViewMatrix()
        {
            ViewMatrix = Matrix.CreateLookAt(Position, Target, UpVector);
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlane, FarPlane);
        }

        public bool IsInView(BoundingBox boundingSphere)
        {
            return  GetFrustrum().Intersects(boundingSphere);
        }

        public BoundingFrustum GetFrustrum()
        {
            return new BoundingFrustum(ViewMatrix * ProjectionMatrix);
        }
    }

}
