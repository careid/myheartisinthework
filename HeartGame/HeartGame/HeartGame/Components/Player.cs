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

        public const float MAX_DEFIB_CHARGE = 1.0f;
        public const float HIGH_DEFIB_CHARGE = 0.6f;
        public float Score { get; set; }
        public float DefibCharge { get; set; }
        public float DefibChargeRate { get; set; }
        public bool Charging { get; set; }
        public ChargeState currentCharge { get; set; }

        public Player(string name, Vector3 position,
                      ComponentManager componentManager,
                      ContentManager content,
                      GraphicsDevice graphics,
                      string spritesheet) :
            base("player", position, componentManager, content, graphics, "surgeonwalk")
        {
            Score = 0.0f;
            DefibCharge = 0;
            DefibChargeRate = 0.5f;
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
        }

        public void PerformActions(List<Event> events)
        {
            base.PerformActions(events);
            foreach (Event e in events)
            {
                switch (e)
                {
                    case Event.SPACE_PRESS:
                        if (!Charging)
                        {
                            SoundManager.PlaySound("defibCharge", GlobalTransform.Translation);
                        }
                        Charging = !Charging;
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
}
