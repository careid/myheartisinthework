using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{



    public class BillboardSpriteComponent : TintableComponent
    {
        public Dictionary<string, Animation> Animations { get; set; }
        public Texture2D SpriteSheet { get; set; }
        public Animation CurrentAnimation { get; set; }
        private static Matrix InvertY =  Matrix.CreateScale(1, -1, 1);
        public bool OrientsToCamera { get; set; }
        public float BillboardRotation { get; set; }

        private static RasterizerState rasterState = new RasterizerState()
        {
            CullMode = CullMode.None,
        };

        public BillboardSpriteComponent(ComponentManager manager, string name, GameComponent parent, Matrix localTransform, Texture2D spriteSheet, bool addToOctree) :
            base(manager, name, parent, localTransform, Vector3.Zero, Vector3.Zero, addToOctree)
        {
            SpriteSheet = spriteSheet;
            Animations = new Dictionary<string, Animation> ();
            OrientsToCamera = true;
            BillboardRotation = 0.0f;
        }

        public void AddAnimation(Animation animation)
        {
            if (CurrentAnimation == null)
            {
                CurrentAnimation = animation;
            }
            Animations[animation.Name] = animation;
        }

        public Animation GetAnimation(string name)
        {
            if (Animations.ContainsKey(name))
            {
                return Animations[name];
            }

            return null;
        }

        public void SetCurrentAnimation(string name)
        {
            Animation anim = GetAnimation(name);

            if (anim != null)
            {
                CurrentAnimation = anim;
            }
        }


        public override void ReceiveMessageRecursive(Message messageToReceive)
        {
            switch (messageToReceive.Type)
            {
                case Message.MessageType.OnChunkModified:
                    HasMoved = true;
                    break;
            }


            base.ReceiveMessageRecursive(messageToReceive);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            if (IsActive)
            {
                if (CurrentAnimation != null)
                {
                    CurrentAnimation.Update(gameTime);
                }

            }


            base.Update(gameTime, camera);
        }

        public override void Render(GameTime gameTime,
                                    Camera camera,
                                    SpriteBatch spriteBatch,
                                    GraphicsDevice graphicsDevice,
                                    Effect effect)
        {

            base.Render(gameTime, camera, spriteBatch, graphicsDevice, effect);
            
            if (IsVisible)
            {



                RasterizerState r = graphicsDevice.RasterizerState;
                graphicsDevice.RasterizerState = rasterState;
                effect.Parameters["xTexture"].SetValue(SpriteSheet);

                DepthStencilState origDepthStencil = graphicsDevice.DepthStencilState;
                DepthStencilState newDepthStencil = DepthStencilState.DepthRead;
                graphicsDevice.DepthStencilState = newDepthStencil;


                if (CurrentAnimation != null)
                {
                    Matrix oldWorld = effect.Parameters["xWorld"].GetValueMatrix();

                    if (OrientsToCamera)
                    {
                        float xscale = GlobalTransform.Left.Length();
                        float yscale = GlobalTransform.Up.Length();
                        float zscale = GlobalTransform.Forward.Length();
                        Matrix rot = Matrix.CreateRotationZ(BillboardRotation);
                        
                        Matrix bill = Matrix.CreateBillboard(GlobalTransform.Translation, camera.Position, camera.UpVector, null);
                        Matrix noTransBill = bill;
                        noTransBill.Translation = Vector3.Zero;

                        Matrix worldRot = Matrix.CreateScale(new Vector3(xscale, yscale, zscale)) * rot * noTransBill;
                        worldRot.Translation = bill.Translation;

                        effect.Parameters["xWorld"].SetValue(worldRot);
                    }
                    else
                    {
                        effect.Parameters["xWorld"].SetValue(GlobalTransform);
                    }

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        CurrentAnimation.Primitives[CurrentAnimation.CurrentFrame].Render(graphicsDevice);
                    }
                    effect.Parameters["xWorld"].SetValue(oldWorld);
                }



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
