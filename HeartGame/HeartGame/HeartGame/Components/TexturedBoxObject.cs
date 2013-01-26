using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class TexturedBoxObject : TintableComponent
    {
        public BoxPrimitive Primitive { get; set;}
        public Texture2D Texture { get; set;}
        private static RasterizerState rasterState = new RasterizerState()
        {
            CullMode = CullMode.CullCounterClockwiseFace
        };

        public TexturedBoxObject(ComponentManager manager, string name, GameComponent parent, Matrix localTransform, Vector3 boundingBoxExtents, Vector3 boundingBoxPos, BoxPrimitive primitive, Texture2D tex) :
            base(manager, name, parent, localTransform, boundingBoxExtents, boundingBoxPos, false)
        {
            Primitive = primitive;
            Texture = tex;
        }

        public override void Render(GameTime gameTime,  Camera camera, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Effect effect)
        {

            base.Render(gameTime, camera, spriteBatch, graphicsDevice, effect);
            Texture2D originalText = effect.Parameters["xTexture"].GetValueTexture2D();

            RasterizerState r = graphicsDevice.RasterizerState;
            graphicsDevice.RasterizerState = rasterState;
            effect.Parameters["xTexture"].SetValue(Texture);


            DepthStencilState origDepthStencil = graphicsDevice.DepthStencilState;
            DepthStencilState newDepthStencil = DepthStencilState.DepthRead;
            graphicsDevice.DepthStencilState = newDepthStencil;



            Matrix oldWorld = effect.Parameters["xWorld"].GetValueMatrix();
            effect.Parameters["xView"].SetValue(camera.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.ProjectionMatrix);

            effect.Parameters["xWorld"].SetValue(GlobalTransform);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Primitive.Render(graphicsDevice);
            }

            effect.Parameters["xWorld"].SetValue(oldWorld);

            effect.Parameters["xTexture"].SetValue(originalText);

            graphicsDevice.DepthStencilState = origDepthStencil;
            graphicsDevice.RasterizerState = r;


        }


    }
}
