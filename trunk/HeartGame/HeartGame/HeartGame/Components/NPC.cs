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
        public Timer WalkTimer { get; set; }
        public float MaxWalkTime { get; set; }
        protected Vector3 Target;

        public Hospital Allegiance
        {
            set 
            { 
                this.Target = value.Component.LocalTransform.Translation;
                this.allegiance = value;
            }
        }

        public NPC(string name, Vector3 position,
                   ComponentManager componentManager,
                   ContentManager content,
                   GraphicsDevice graphics,
                  string spritesheet) 
            : base(name, position, componentManager, content, graphics, spritesheet)
        {
            MaxWalkTime = 5.0f;
            WalkTimer = new Timer(MaxWalkTime, true);
            Target = Vector3.Zero;
            OrientWithVelocity = true;
            Texture2D sprites = content.Load<Texture2D>(spritesheet);

            List<Point> deadFrame = new List<Point>();
            deadFrame.Add(new Point(0, 1));
            
            OrientedAnimation dead = new OrientedAnimation(new Animation(graphics, sprites, name + "_dead_left", 32, 32, deadFrame, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, name + "_dead_right", 32, 32, deadFrame, true, Color.White, 10.0f, 0.8f, 1, true));
            image.AddOrientedAnimation(dead);
            AnimationState["dead"] = dead.Name;
            State = "walk";
            SetAnimation();
        }

        public void SetTag(string tag)
        {
            Tags.Add(tag);
            this.tag = tag;
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            if (State.Equals("dead"))
            {
                velocityController.targetVelocity = Vector3.Zero;
            }
            else if (State.Equals("walk"))
            {
                WalkTimer.Update(gameTime);

                Vector3 normalized = (Target - GlobalTransform.Translation);
                normalized.Normalize();

                velocityController.targetVelocity = velocityController.MaxSpeed * normalized;
                velocityController.IsTracking = true;

                if (WalkTimer.HasTriggered)
                {
                    State = "dead";
                    SetAnimation();
                }
            }
            base.Update(gameTime, camera);
        }
    }
}
