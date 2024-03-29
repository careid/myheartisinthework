﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    
    public class BatchBillboard : BillboardSpriteComponent
    {
        public List<Matrix> LocalTransforms { get; set; }
        public List<float> Rotations { get; set; }
        public List<Color> Tints { get; set; }
        public float CullDistance = 1000.0f;
        BatchBillboardPrimitive Primitive;
        Point Frame;
        int Width = 32;
        int Height = 32;
        private static RasterizerState rasterState = new RasterizerState()
        {
            CullMode = CullMode.None,
        };
        GraphicsDevice graphicsDevice;

        public BatchBillboard(ComponentManager manager,
                              string name,
                              GameComponent parent,
                              Matrix localTransform,
                              Texture2D spriteSheet,
                              int numBillboards, GraphicsDevice graphi) :
            base(manager, name, parent, localTransform, spriteSheet, false)
        {
            LocalTransforms = new List<Matrix>(numBillboards);
            Rotations = new List<float>(numBillboards);
            Tints = new List<Color>(numBillboards);
            FrustrumCull = false;
            Width = spriteSheet.Width;
            Height = spriteSheet.Height;
            Frame = new Point(0, 0);
            graphicsDevice = graphi;
        }

        public void AddTransform(Matrix transform, float rotation, Color tint, bool rebuild)
        {
            LocalTransforms.Add(transform);
            Rotations.Add(rotation);
            Tints.Add(tint);
            if (rebuild)
            {
                RebuildPrimitive();
            }

        }

        public void RebuildPrimitive()
        {
            if (Primitive != null && Primitive.VertexBuffer != null )
            {
                Primitive.VertexBuffer.Dispose();
            }

            Primitive = new BatchBillboardPrimitive(graphicsDevice, SpriteSheet, Width, Height, Frame, 1.0f, 1.0f, false, LocalTransforms, Tints);
        }

        public void RemoveTransform(int index)
        {
            if (index >= 0 && index < LocalTransforms.Count)
            {
                LocalTransforms.RemoveAt(index);
                Rotations.RemoveAt(index);
                Tints.RemoveAt(index);
            }
            Primitive = new BatchBillboardPrimitive(graphicsDevice, SpriteSheet, Width, Height, Frame, 1.0f, 1.0f, false, LocalTransforms, Tints);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            //base.Update(gameTime, chunks, camera);
        }

        public override void Render(GameTime gameTime,
                                    Camera camera,
                                    SpriteBatch spriteBatch,
                                    GraphicsDevice graphicsDevice,
                                    Effect effect)
        {

            if (Primitive == null)
            {
                Primitive = new BatchBillboardPrimitive(graphicsDevice, SpriteSheet, Width, Height, Frame, 1.0f, 1.0f, false, LocalTransforms, Tints);
            }


            effect.Parameters["xTint"].SetValue(new Vector4(1, 1, 1, 1));
            if (IsVisible && (camera.Position - GlobalTransform.Translation).LengthSquared() < CullDistance)
            {
                Texture2D originalText = effect.Parameters["xTexture"].GetValueTexture2D();
                RasterizerState r = graphicsDevice.RasterizerState;
                graphicsDevice.RasterizerState = rasterState;
                effect.Parameters["xTexture"].SetValue(SpriteSheet);

                DepthStencilState origDepthStencil = graphicsDevice.DepthStencilState;
                DepthStencilState newDepthStencil = DepthStencilState.DepthRead;
                graphicsDevice.DepthStencilState = newDepthStencil;


                Matrix oldWorld = effect.Parameters["xWorld"].GetValueMatrix();
                effect.Parameters["xWorld"].SetValue(GlobalTransform);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Primitive.Render(graphicsDevice);
                }
                effect.Parameters["xWorld"].SetValue(oldWorld);


                effect.Parameters["xTexture"].SetValue(originalText);

                if (origDepthStencil != null)
                {
                    graphicsDevice.DepthStencilState = origDepthStencil;
                }

                if (r != null)
                {
                    graphicsDevice.RasterizerState = r;
                }
            }        

        }

    }
     
}
