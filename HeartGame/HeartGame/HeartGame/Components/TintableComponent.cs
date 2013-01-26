using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class TintableComponent : LocatableComponent
    {
        private bool firstIteration = true;
        public Color Tint { get; set; }
        public Color TargetTint { get; set; }
        public float TintChangeRate { get; set; }
        public Timer LightingTimer { get; set; }
        public bool colorAppplied = false;

        public TintableComponent(ComponentManager manager, string name, GameComponent parent, Matrix localTransform,  Vector3 boundingBoxExtents, Vector3 boundingBoxPos, bool octree) :
            base(manager, name, parent, localTransform, boundingBoxExtents, boundingBoxPos, octree)
        {
            Tint = Color.White;
            LightingTimer = new Timer(1.0f, false);
            TargetTint = Tint;
            TintChangeRate = 1.0f;
        }



        public override void Update(GameTime gameTime, Camera camera)
        {
        
            LightingTimer.Update(gameTime);
            Vector4 lerpTint = new Vector4((float)TargetTint.R / 255.0f, (float)TargetTint.G / 255.0f, (float)TargetTint.B / 255.0f, (float)TargetTint.A / 255.0f);
            Vector4 currTint = new Vector4((float)Tint.R / 255.0f, (float)Tint.G / 255.0f, (float)Tint.B / 255.0f, (float)Tint.A / 255.0f);

            Vector4 delta = lerpTint - currTint;
            lerpTint = currTint + delta * Math.Max(Math.Min(LightingTimer.CurrentTimeSeconds * TintChangeRate, 1.0f), 0.0f);

            Tint = new Color(lerpTint.X, lerpTint.Y, lerpTint.Z, lerpTint.W);

            

            base.Update(gameTime,camera);
        }

        public override void Render(GameTime gameTime,  Camera camera, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Effect effect)
        {
            if (IsVisible)
            {
                effect.Parameters["xTint"].SetValue(new Vector4(Tint.R, Tint.G, Tint.B, Tint.A));

                base.Render(gameTime,  camera, spriteBatch, graphicsDevice, effect);

            }
        }
    }
}
