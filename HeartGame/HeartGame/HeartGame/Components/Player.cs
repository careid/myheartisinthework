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
    public class Player : Person
    {
        public enum ChargeState
        {
            NO_CHARGE,
            LOW_CHARGE,
            HIGH_CHARGE,
            MAX_CHARGE
        }
        public delegate void defibCallbackType(Player owner);
        public static defibCallbackType defib;
        public const float MAX_DEFIB_CHARGE = 1.0f;
        public const float HIGH_DEFIB_CHARGE = 0.4f;
        public float DefibCharge { get; set; }
        public float DefibChargeRate { get; set; }
        public bool Charging { get; set; }
        public ChargeState currentCharge { get; set; }

        public Player(string tag, Vector3 position,
                      ComponentManager componentManager,
                      ContentManager content,
                      GraphicsDevice graphics,
                      string spritesheet) :
            base(tag, position, componentManager, content, graphics, "surgeonwalk")
        {
            DefibCharge = 0;
            DefibChargeRate = 0.5f;

            Texture2D sprites = content.Load<Texture2D>(spritesheet);
            
            List<Point> lowIdle = new List<Point>();
            lowIdle.Add(new Point(0, 1));
            lowIdle.Add(new Point(1, 1));
            lowIdle.Add(new Point(2, 1));
            lowIdle.Add(new Point(3, 1));

            image.AddOrientedAnimation(new Animation(graphics, sprites, tag + "_low_left", 32, 32, lowIdle, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_low_right", 32, 32, lowIdle, true, Color.White, 10.0f, 0.8f, 1, true));

            List<Point> lowWalk = new List<Point>();
            lowWalk.Add(new Point(0, 2));
            lowWalk.Add(new Point(1, 2));
            lowWalk.Add(new Point(2, 2));
            lowWalk.Add(new Point(3, 2));

            image.AddOrientedAnimation(new Animation(graphics, sprites, tag + "_low_left", 32, 32, lowWalk, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_low_right", 32, 32, lowWalk, true, Color.White, 10.0f, 0.8f, 1, true));

            List<Point> highIdle = new List<Point>();
            highIdle.Add(new Point(0, 3));
            highIdle.Add(new Point(1, 3));
            highIdle.Add(new Point(2, 3));
            highIdle.Add(new Point(3, 3));

            image.AddOrientedAnimation(new Animation(graphics, sprites, tag + "_high_left", 32, 32, highIdle, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_high_right", 32, 32, highIdle, true, Color.White, 10.0f, 0.8f, 1, true));

            List<Point> highWalk = new List<Point>();
            highWalk.Add(new Point(0, 4));
            highWalk.Add(new Point(1, 4));
            highWalk.Add(new Point(2, 4));
            highWalk.Add(new Point(3, 4));

            image.AddOrientedAnimation(new Animation(graphics, sprites, tag + "_high_left", 32, 32, highWalk, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_high_right", 32, 32, highWalk, true, Color.White, 10.0f, 0.8f, 1, true));

        }

        public override void  Update(GameTime gameTime, Camera camera)
        {
            base.Update(gameTime, camera);
            
            if (allegiance != null)
            { Score = allegiance.Score; }

            if (Charging)
            {
                DefibCharge += (float)gameTime.ElapsedGameTime.TotalSeconds * DefibChargeRate;
            }
            if (DefibCharge == 0)
            {
                currentCharge = ChargeState.NO_CHARGE;
            }
            else if (DefibCharge < HIGH_DEFIB_CHARGE)
            {
                currentCharge = ChargeState.LOW_CHARGE;
            }
            else if (DefibCharge < MAX_DEFIB_CHARGE)
            {
                currentCharge = ChargeState.HIGH_CHARGE;
            }
            else
            {
                DefibCharge = MAX_DEFIB_CHARGE;
                currentCharge = ChargeState.MAX_CHARGE;
            }
        }

        public override void PerformAction(Event e)
        {
            base.PerformAction(e);
            switch (e)
            {
                case Event.SPACE_PRESS:
                    if (!Charging)
                    {
                        SoundManager.PlaySound("defibCharge", GlobalTransform.Translation);
                    }
                    Charging = true;
                    break;
                case Event.SPACE_RELEASE:
                    Charging = false;
                    Console.Out.WriteLine("defibbing in player performaction");
                    SoundManager.StopSounds("defibCharge");
                    if (DefibCharge >= 0.25f)
                        defib(this);
                    DefibCharge = 0.0f;
                    break;
                default: break;
            }
        }
    }
}
