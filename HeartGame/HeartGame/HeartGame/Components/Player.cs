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
        public float Score { get; set; }
        public float DefibCharge { get; set; }
        public float DefibChargeRate { get; set; }
        public bool Charging { get; set; }

        public Player(string name, Vector3 position,
                      ComponentManager componentManager,
                      ContentManager content,
                      GraphicsDevice graphics,
                      string spritesheet) :
            base(name, position, componentManager, content, graphics, spritesheet)
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
            if (DefibCharge > 1.0f)
            {
                DefibCharge = 1.0f;
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
