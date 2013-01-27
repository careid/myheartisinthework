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
        BillboardSpriteComponent monitor;
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
            monitor = new BillboardSpriteComponent(componentManager, "heart", this, translation, heartsprite, false);
            monitor.OrientsToCamera = true;
            
            List<Point> aliveP = new List<Point>();
            aliveP.Add(new Point(0, 0));
            aliveP.Add(new Point(1, 0));
            aliveP.Add(new Point(2, 0));
            aliveP.Add(new Point(3, 0));
            
            monitor.AddAnimation(new Animation(graphics, heartsprite, "alive", 32, 10, aliveP, true, Color.White, 5.0f, 0.7f, 0.2f, false));
            
            List<Point> deadP = new List<Point>();
            deadP.Add(new Point(4, 0));
            
            monitor.AddAnimation(new Animation(graphics, heartsprite, "dead", 32, 10, deadP, true, Color.White, 5.0f, 0.7f, 0.2f, false));
            foreach (Animation anim in monitor.Animations.Values)
                anim.Play();

            image.LocalTransform = Matrix.CreateTranslation(new Vector3(0, -0.25f, 0));
        }

        public void SetTag(string tag)
        {
            Tags.Add(tag);
            this.tag = tag;
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            monitor.SetCurrentAnimation("dead");
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
                    string[] deathSounds = { "manDying1", "manDying2"};
                    SoundManager.PlaySound(deathSounds[RandomHelper.random.Next(0, 2)], GlobalTransform.Translation);
                }
            }
            else if (State.Equals("walk"))
            {
                monitor.SetCurrentAnimation("alive");
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
                }
            }
            SetAnimation();
            base.Update(gameTime, camera);
        }
    }
}
