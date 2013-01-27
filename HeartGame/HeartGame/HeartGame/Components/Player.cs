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

        public AnimationPair walkLow;
        public AnimationPair walkHigh;
        public AnimationPair idleLow;
        public AnimationPair idleHigh;
        public Dictionary<ChargeState, AnimationPair> IdleDictionary;
        public Dictionary<ChargeState, AnimationPair> WalkDictionary;
        public const float MAX_DEFIB_CHARGE = 1.0f;
        public const float HIGH_DEFIB_CHARGE = 0.4f;
        public float Score { get; set; }
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
            Score = 0.0f;
            DefibCharge = 0;
            DefibChargeRate = 0.5f;

            Texture2D sprites = content.Load<Texture2D>(spritesheet);

            IdleDictionary = new Dictionary<ChargeState, AnimationPair>();
            IdleDictionary[ChargeState.NO_CHARGE] = idle;
            WalkDictionary = new Dictionary<ChargeState, AnimationPair>();
            WalkDictionary[ChargeState.NO_CHARGE] = walk;
            
            List<Point> lowIdle = new List<Point>();
            lowIdle.Add(new Point(0, 1));
            lowIdle.Add(new Point(1, 1));
            lowIdle.Add(new Point(2, 1));
            lowIdle.Add(new Point(3, 1));

            idleLow = new AnimationPair(new Animation(graphics, sprites, tag + "_low_left", 32, 32, lowIdle, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_low_right", 32, 32, lowIdle, true, Color.White, 10.0f, 0.8f, 1, true));
            IdleDictionary[ChargeState.LOW_CHARGE] = idleLow;

            List<Point> lowWalk = new List<Point>();
            lowWalk.Add(new Point(0, 2));
            lowWalk.Add(new Point(1, 2));
            lowWalk.Add(new Point(2, 2));
            lowWalk.Add(new Point(3, 2));

            walkLow = new AnimationPair(new Animation(graphics, sprites, tag + "_low_left", 32, 32, lowWalk, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_low_right", 32, 32, lowWalk, true, Color.White, 10.0f, 0.8f, 1, true));
            WalkDictionary[ChargeState.LOW_CHARGE] = walkLow;

            List<Point> highIdle = new List<Point>();
            highIdle.Add(new Point(0, 3));
            highIdle.Add(new Point(1, 3));
            highIdle.Add(new Point(2, 3));
            highIdle.Add(new Point(3, 3));

            idleHigh = new AnimationPair(new Animation(graphics, sprites, tag + "_high_left", 32, 32, highIdle, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_high_right", 32, 32, highIdle, true, Color.White, 10.0f, 0.8f, 1, true));
            IdleDictionary[ChargeState.HIGH_CHARGE] = idleHigh;
            IdleDictionary[ChargeState.MAX_CHARGE] = idleHigh;

            List<Point> highWalk = new List<Point>();
            highWalk.Add(new Point(0, 4));
            highWalk.Add(new Point(1, 4));
            highWalk.Add(new Point(2, 4));
            highWalk.Add(new Point(3, 4));

            walkHigh = new AnimationPair(new Animation(graphics, sprites, tag + "_high_left", 32, 32, highWalk, true, Color.White, 10.0f, 0.8f, 1, false),
                        new Animation(graphics, sprites, tag + "_high_right", 32, 32, highWalk, true, Color.White, 10.0f, 0.8f, 1, true));
            WalkDictionary[ChargeState.HIGH_CHARGE] = walkHigh;
            WalkDictionary[ChargeState.MAX_CHARGE] = walkHigh;

        }

        public override void  Update(GameTime gameTime, Camera camera)
        {
            base.Update(gameTime, camera);
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
            if (Velocity.Length() < 1.0f)
            {
                orienter.Transform(IdleDictionary[currentCharge]);
            }
            else
            {
                orienter.Transform(WalkDictionary[currentCharge]);
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
                    SoundManager.StopSounds("defibCharge");
                    DefibCharge = 0.0f;
                    break;
                default: break;
            }
        }
    }
}
