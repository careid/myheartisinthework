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
        public Timer DieTimer { get; set; }
        public float MaxWalkTime { get; set; }
        public float MaxDieTime { get; set; }
        protected Vector3 Target;
        

        public Hospital Team
        {
            set 
            { 
                this.Target = value.Component.LocalTransform.Translation;
                this.team = value;
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
            MaxDieTime = 1.0f;
            WalkTimer = new Timer((int)(10000*position.X) % 7, true);
            DieTimer = new Timer(MaxDieTime, false);
            Target = Vector3.Zero;
            OrientWithVelocity = true;
            Texture2D sprites = content.Load<Texture2D>(spritesheet);

            List<Point> deadFrame = new List<Point>();
            deadFrame.Add(new Point(0, 1));
            
            OrientedAnimation dead = new OrientedAnimation(new Animation(graphics, sprites, name + "_dead_left", 32, 32, deadFrame, true, Color.White, 14.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, name + "_dead_right", 32, 32, deadFrame, true, Color.White, 14.0f, 0.8f, 1, true));
            image.AddOrientedAnimation(dead);
            AnimationState["dead"] = dead.Name;

            List<Point> dyingFrame = new List<Point>();
            dyingFrame.Add(new Point(0, 2));
            dyingFrame.Add(new Point(1, 2));
            dyingFrame.Add(new Point(2, 2));
            dyingFrame.Add(new Point(3, 2));
            dyingFrame.Add(new Point(4, 2));

            OrientedAnimation dying = new OrientedAnimation(new Animation(graphics, sprites, name + "_dying_left", 32, 32, dyingFrame, false, Color.White, 7.0f, 0.8f, 1, false),
            new Animation(graphics, sprites, name + "_dying_right", 32, 32, dyingFrame, false, Color.White, 7.0f, 0.8f, 1, true));
            image.AddOrientedAnimation(dying);
            dying.Play();

            AnimationState["dying"] = dying.Name;

            List<Point> flyFrame = new List<Point>();
            flyFrame.Add(new Point(0, 3));

            OrientedAnimation fly = new OrientedAnimation(new Animation(graphics, sprites, name + "_fly_left", 32, 32, flyFrame, true, Color.White, 2.0f, 0.8f, 1, true),
                        new Animation(graphics, sprites, name + "_fly_right", 32, 32, flyFrame, true, Color.White, 2.0f, 0.8f, 1, false));
            image.AddOrientedAnimation(fly);
            AnimationState["fly"] = fly.Name;

            State = "walk";
            SetAnimation();

            Matrix translation = Matrix.CreateTranslation(new Vector3(0, 1, 0));
            Texture2D heartsprite = content.Load<Texture2D>("heartrate");
            BillboardSpriteComponent monitor = new BillboardSpriteComponent(componentManager, "heart", this, translation, heartsprite, false);
            monitor.OrientsToCamera = true;
            List<Point> monP = new List<Point>();
            monP.Add(new Point(0, 0));
            Animation monitorAnimation = new Animation(graphics, heartsprite, "mon", 32, 10, monP, false, Color.White, 1, 0.7f, 0.2f, false);
            monitor.AddAnimation(monitorAnimation);
            monitorAnimation.Play();
        }

        public void SetTag(string tag)
        {
            Tags.Add(tag);
            this.tag = tag;
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            if (GlobalTransform.Translation.Y >= 1)
            {
                State = "fly";
            }
            else if(State.Equals("fly"))
            {
                State = "walk";
            }
            if (State.Equals("dead"))
            {
                velocityController.targetVelocity = Vector3.Zero;
            }
            else if (State.Equals("dying"))
            {
                velocityController.targetVelocity = Vector3.Zero;
                DieTimer.Update(gameTime);
                if (DieTimer.HasTriggered)
                {
                    State = "dead";
                }
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
                    State = "dying";
                    DieTimer.Reset(MaxDieTime);
                    image.GetOrientedAnimation(AnimationState["dying"]).Reset();

                    Vector3 oldTranslation = image.LocalTransform.Translation;
                    image.LocalTransform = Matrix.CreateTranslation(new Vector3(oldTranslation.X, oldTranslation.Y-0.25f, oldTranslation.Z));
                }
            }
            SetAnimation();
            base.Update(gameTime, camera);
        }
    }
}
