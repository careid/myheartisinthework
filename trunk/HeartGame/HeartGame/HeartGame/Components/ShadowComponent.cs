using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class ShadowComponent : BillboardSpriteComponent
    {
        public float GlobalScale { get; set; }
        public Timer UpdateTimer { get; set; }
        public float Height { get; set; }

        public ShadowComponent(ComponentManager manager, string name, GameComponent parent, Matrix localTransform, Texture2D spriteSheet) :
            base(manager, name, parent, localTransform, spriteSheet, false)
        {
            OrientsToCamera = false;
            GlobalScale = 1.0f;
            DrawBoundingBox = false;
            UpdateTimer = new Timer(0.5f,false);
            Tint = Color.White;
            Height = 0.1f;

        }

        public override void Update(GameTime gameTime,Camera camera)
        {
            if (HasMoved && UpdateTimer.HasTriggered)
            {
                LocatableComponent p = (LocatableComponent)Parent;

                Vector3 pos = p.GlobalTransform.Translation;
                pos.Y = Height;

                    
                Matrix newTrans = LocalTransform;
                newTrans.Translation = (pos - p.GlobalTransform.Translation) + new Vector3(0, -2.15f, 0);
                LocalTransform = newTrans;
                    
                    
              
                UpdateTimer.HasTriggered = false;
            }
            else
            {
                UpdateTimer.Update(gameTime);
            }

            base.Update(gameTime, camera);
        }

    }
    
}
