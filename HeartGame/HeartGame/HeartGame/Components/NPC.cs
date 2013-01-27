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
            Walking,
            Idle
        }

        public AnimationPair dead;
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

            List<Point> deadFrame = new List<Point>();
            deadFrame.Add(new Point(0, 1));
            
            dead = new AnimationPair (new Animation(graphics, sprites, name + "_dead_left", 32, 32, deadFrame, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, name + "_dead_right", 32, 32, deadFrame, true, Color.White, 10.0f, 0.8f, 1, true));
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            switch (State)
            {
                case NPCState.Dead:
                    orienter.Transform(dead);
                    velocityController.targetVelocity = Vector3.Zero;
                    break;

                case NPCState.Walking:
                    orienter.Transform(walk);
                    WalkTimer.Update(gameTime);

                    Vector3 normalized = (Target - GlobalTransform.Translation);
                    normalized.Normalize();

                    velocityController.targetVelocity = velocityController.MaxSpeed * normalized;
                    velocityController.IsTracking = true;

                    if (WalkTimer.HasTriggered)
                    {
                        State = NPCState.Dead;
                        orienter.Transform(dead);
                    }


                    break;
            }

            base.Update(gameTime, camera);
        }
    }
}
