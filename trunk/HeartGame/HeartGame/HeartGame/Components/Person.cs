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
        public enum Movement
        {
            Walking,
            Idle
        }

        public Hospital team;
        public float Score { get; set; }
        public VelocityController velocityController;
        public static float maxSpeed = 3.0f;
        public string tag;
        public Dictionary<String, String> AnimationState;
        public String State { get; set; }
        public OrientableBillboardSpriteComponent image;
        public ShadowComponent teamCircle;

        public Person(string _tag, Vector3 position,
                      ComponentManager componentManager,
                      ContentManager content,
                      GraphicsDevice graphics,
                      string spritesheet) :
            base(componentManager, "person", componentManager.RootComponent, Matrix.CreateTranslation(position),  new Vector3(0.5f, 1.0f, 0.5f),
            new Vector3(0.0f, -0.3f, 0.0f),  1.0f, 1.0f, 0.999f, 0.999f)
        {
            OrientWithVelocity = true;
            Texture2D sprites = content.Load<Texture2D>(spritesheet);
            tag = _tag;
            Tags.Add(tag);
            string name = "person";
            OrientWithVelocity = true;
            this.team = null;
            Score = 0.0f;
            AnimationState = new Dictionary<string,string>();

            List<Point> offsets = new List<Point>();
            offsets.Add(new Point(0, 0));

            Point offset = offsets[RandomHelper.random.Next(0, offsets.Count)];

            List<Point> rightFrames = new List<Point>();
            rightFrames.Add(new Point(1 + offset.X, 0 + offset.Y));
            rightFrames.Add(new Point(2 + offset.X, 0 + offset.Y));
            rightFrames.Add(new Point(3 + offset.X, 0 + offset.Y));
            rightFrames.Add(new Point(4 + offset.X, 0 + offset.Y));

            Animation walkRight = new Animation(graphics, sprites, name + "_walk_right", 32, 32, rightFrames, true, Color.White, 10.0f, 0.8f, 1, true);

            List<Point> leftFrames = new List<Point>();
            leftFrames.Add(new Point(1 + offset.X, 0 + offset.Y));
            leftFrames.Add(new Point(2 + offset.X, 0 + offset.Y));
            leftFrames.Add(new Point(3 + offset.X, 0 + offset.Y));
            leftFrames.Add(new Point(4 + offset.X, 0 + offset.Y));

            Animation walkLeft = new Animation(graphics, sprites, name + "_walk_left", 32, 32, leftFrames, true, Color.White, 10.0f, 0.8f, 1, false);

            List<Point> rightFramesIdle = new List<Point>();
            rightFramesIdle.Add(new Point(0 + offset.X, 0 + offset.Y));

            Animation idleRight = new Animation(graphics, sprites, name + "_idle_right", 32, 32, rightFramesIdle, true, Color.White, 2.0f, 0.8f, 1, true);

            List<Point> leftFramesIdle = new List<Point>();
            leftFramesIdle.Add(new Point(0 + offset.X, 0 + offset.Y));

            Animation idleLeft = new Animation(graphics, sprites, name + "_idle_left", 32, 32, leftFramesIdle, true, Color.White, 2.0f, 0.8f, 1, false);

            walkLeft.Play();
            walkRight.Play();
            idleLeft.Play();
            idleRight.Play();

            Matrix spriteMatrix = Matrix.Identity;
            image = new OrientableBillboardSpriteComponent(componentManager, "idlesprite", this, spriteMatrix, sprites);
            OrientedAnimation idle = new OrientedAnimation(idleLeft, idleRight);
            image.AddOrientedAnimation(idle);
            AnimationState["idle"] = idle.Name;
            OrientedAnimation walk = new OrientedAnimation(walkLeft, walkRight);
            image.AddOrientedAnimation(walk);
            AnimationState["walk"] = walk.Name;
            
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


            Matrix teamTransform = Matrix.CreateRotationX((float)Math.PI * 0.5f);
            //shadowTransform.Translation = new Vector3(0.0f, -0.31f, 0.0f);
            ShadowComponent team = new ShadowComponent(componentManager, "team", this, shadowTransform, content.Load<Texture2D>("circle"));
            team.OrientsToCamera = false;
            List<Point> circleP = new List<Point>();
            circleP.Add(new Point(0, 0));
            Animation circleAnimation = new Animation(graphics, content.Load<Texture2D>("circle"), "teamcircle", 32, 32, shP, false, Color.Red, 1, 0.9f, 0.9f, false);
            team.AddAnimation(circleAnimation);
            circleAnimation.Play();
            team.SetCurrentAnimation("teamcircle");
            teamCircle = team;

            Tags.Add("Walker");

            velocityController = new VelocityController(this);
            velocityController.IsTracking = true;
        }

        public void SetAnimation()
        {
            image.SetCurrentOrientedAnimation(AnimationState[State]);
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

        public virtual void PerformAction(Event e)
        {
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

    }
}
