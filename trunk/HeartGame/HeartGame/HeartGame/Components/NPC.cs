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
    public class NPC : Person
    {
        public enum NPCState
        {
            Dead,
            Walking
        }

        public NPCState State { get; set; }
        public Timer WalkTimer { get; set; }
        public float MaxWalkTime { get; set; }
        public Vector3 Target { get; set; }

        public NPC(string name, Vector3 position,
                   ComponentManager componentManager,
                   ContentManager content,
                   GraphicsDevice graphics,
                  string spritesheet) 
            : base(name, position, componentManager, content, graphics, spritesheet)
        {
            State = NPCState.Walking;
            MaxWalkTime = 5.0f;
            WalkTimer = new Timer(MaxWalkTime, true);
            Target = Vector3.Zero;
            OrientWithVelocity = true;
            Texture2D sprites = content.Load<Texture2D>(spritesheet);

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
            // spriteMatrix.Translation = new Vector3(0, 0.1f, 0);
            OrientedAnimation testSprite = new OrientedAnimation(componentManager, "testsprite", this, spriteMatrix, sprites, walkRight, walkLeft, idleRight, idleLeft);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            switch (State)
            {
                case NPCState.Dead:
                    velocityController.targetVelocity = Vector3.Zero;
                    break;

                case NPCState.Walking:
                    WalkTimer.Update(gameTime);

                    Vector3 normalized = (Target - GlobalTransform.Translation);
                    normalized.Normalize();

                    velocityController.targetVelocity = velocityController.MaxSpeed * normalized;
                    velocityController.IsTracking = true;

                    if (WalkTimer.HasTriggered)
                    {
                        State = NPCState.Dead;
                    }


                    break;
            }

            base.Update(gameTime, camera);
        }
    }
}
