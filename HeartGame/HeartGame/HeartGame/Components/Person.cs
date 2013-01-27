using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HeartGame
{
    public class Person : PhysicsComponent
    {
        public VelocityController velocityController;
        public static float maxSpeed = 3.0f;
        public Person(string name, Vector3 position,
                      ComponentManager componentManager,
                      ContentManager content,
                      GraphicsDevice graphics,
                      string spritesheet) :
            base(componentManager, name, componentManager.RootComponent, Matrix.CreateTranslation(position),  new Vector3(0.5f, 1.0f, 0.5f),
            new Vector3(0.0f, -0.3f, 0.0f),  1.0f, 1.0f, 0.999f, 0.999f)
        {
            Matrix shadowTransform = Matrix.CreateRotationX((float)Math.PI * 0.5f);
            //shadowTransform.Translation = new Vector3(0.0f, -0.31f, 0.0f);
            ShadowComponent shadow = new ShadowComponent(componentManager, "shadow", this, shadowTransform, content.Load<Texture2D>("shadowcircle"));
            shadow.OrientsToCamera = false;
            List<Point> shP = new List<Point>();
            shP.Add(new Point(0, 0));
            Animation shadowAnimation = new Animation(graphics, content.Load<Texture2D>("shadowcircle"), "sh", 32, 32, shP, false, Color.White, 1, 0.7f, 0.7f, false);
            shadow.AddAnimation(shadowAnimation);
            shadowAnimation.Play();
            shadow.SetCurrentAnimation("sh");

            Tags.Add("Walker");

            velocityController = new VelocityController(this);
            velocityController.IsTracking = true;
        }

        public void MoveInDirection(Vector3 _TargetVelocity)
        {
            if (_TargetVelocity.LengthSquared() >= 1)
            {
                _TargetVelocity.Normalize();
                _TargetVelocity *= velocityController.MaxSpeed;
            }
            velocityController.targetVelocity = _TargetVelocity;
        }

        public void PerformActions(List<Event> events)
        {
            foreach (Event e in events)
            {
                Console.Out.WriteLine("Event = {0}", e);
                switch (e)
                {
                    case Event.W_PRESS:
                        velocityController.targetVelocity.X += -velocityController.MaxSpeed;
                        break;
                    case Event.W_RELEASE:
                        velocityController.targetVelocity.X -= -velocityController.MaxSpeed;
                        break;

                    case Event.A_PRESS:
                        velocityController.targetVelocity.Z += velocityController.MaxSpeed;
                        break;
                    case Event.A_RELEASE:
                        velocityController.targetVelocity.Z -= velocityController.MaxSpeed;
                        break;
                    
                    case Event.S_PRESS:
                        velocityController.targetVelocity.X += velocityController.MaxSpeed;
                        break;
                    case Event.S_RELEASE:
                        velocityController.targetVelocity.X -= velocityController.MaxSpeed;
                        break;

                    case Event.D_PRESS:
                        velocityController.targetVelocity.Z += -velocityController.MaxSpeed;
                        break;
                    case Event.D_RELEASE:
                        velocityController.targetVelocity.Z -= -velocityController.MaxSpeed;
                        break;

                    default: break;
                }
            }

            Console.Out.WriteLine("{0}", velocityController.targetVelocity);
        }

    }
}
