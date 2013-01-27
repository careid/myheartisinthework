using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class VelocityController : GameComponent
    {
        public PhysicsComponent Component { get; set; }
        public PIDController Controller { get; set;}
        public Vector3 targetVelocity;
        public bool IsTracking { get; set; }
        public float MaxSpeed { get; set; }

        public VelocityController(PhysicsComponent component) :
            base(component.Manager, "VelocityController", component)
        {
            Component = component;
            Controller = new PIDController(20.0f, 0.5f, 0.0f);
            targetVelocity = Vector3.Zero;
            IsTracking = false;
            MaxSpeed = 10;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, Camera camera)
        {
            if (IsTracking && Component.GlobalTransform.Translation.Y < 1.0f)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Vector3 force = Controller.GetOutput(dt, targetVelocity, Component.Velocity);
                force.Y = 0;
                Component.ApplyForce(force, dt);
            }

            base.Update(gameTime, camera);
        }

    }
}
