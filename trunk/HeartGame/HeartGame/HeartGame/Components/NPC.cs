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
