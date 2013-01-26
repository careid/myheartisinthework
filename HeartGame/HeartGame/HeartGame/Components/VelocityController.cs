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
        public Vector3 TargetVelocity { get; set; }
        public bool IsTracking { get; set; }
        public bool WASDControl { get; set; }
        public float MaxSpeed { get; set; }

        public VelocityController(PhysicsComponent component) :
            base(component.Manager, "VelocityController", component)
        {
            Component = component;
            Controller = new PIDController(3.0f, 0.5f, 0.01f);
            TargetVelocity = Vector3.Zero;
            IsTracking = false;
            WASDControl = false;
            MaxSpeed = 10;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, Camera camera)
        {
            if (IsTracking)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                Component.ApplyForce(Controller.GetOutput(dt, TargetVelocity, Component.Velocity), dt);
            }

            if (WASDControl)
            {
                KeyboardState keyboardState = Keyboard.GetState();

                TargetVelocity = Vector3.Zero;

                if (keyboardState.IsKeyDown(Keys.D))
                {
                    TargetVelocity += new Vector3(0, 0, -1);
                }
                else if (keyboardState.IsKeyDown(Keys.A))
                {
                    TargetVelocity += new Vector3(0, 0, 1);
                }
                
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    TargetVelocity += new Vector3(1, 0, 0);
                }
                else if (keyboardState.IsKeyDown(Keys.W))
                {
                    TargetVelocity += new Vector3(-1, 0, 0);
                }

                if (TargetVelocity.LengthSquared() >= 1)
                {
                    TargetVelocity.Normalize();
                    TargetVelocity *= MaxSpeed;
                }
            }

            base.Update(gameTime, camera);
        }
    }
}
