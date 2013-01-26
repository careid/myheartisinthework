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
    class EntityFactory
    {



        public static GameComponent GenerateBlankBox(BoundingBox box, ComponentManager componentManager, ContentManager content, GraphicsDevice graphics, string texture)
        {
            Matrix transform = Matrix.CreateTranslation((box.Max - box.Min) * 0.5f + box.Min);
            Vector3 extents = box.Max - box.Min;
            LocatableComponent boxComponent = new LocatableComponent(componentManager, "box", componentManager.RootComponent, transform, extents, Vector3.Zero);


            Texture2D tex = content.Load<Texture2D>(texture);
            BoxPrimitive.BoxTextureCoords uvs = new BoxPrimitive.BoxTextureCoords(tex.Width, tex.Height, tex.Width, tex.Height, Point.Zero, Point.Zero, Point.Zero, Point.Zero, Point.Zero, Point.Zero);
            BoxPrimitive primitive = new BoxPrimitive(graphics, extents.X, extents.Y, extents.Z, uvs);

            TexturedBoxObject graphicalBox = new TexturedBoxObject(componentManager, "texturedbox", boxComponent, Matrix.CreateTranslation(-extents * 0.5f) , extents, Vector3.Zero, primitive, content.Load<Texture2D>(texture));
            boxComponent.FrustrumCull = false;
            graphicalBox.FrustrumCull = false;

            return boxComponent;
        }

        public static GameComponent GenerateWalker(Vector3 position,
                                                  ComponentManager componentManager,
                                                  ContentManager content,
                                                  GraphicsDevice graphics, 
                                                  string spritesheet)
        {

            PhysicsComponent testDwarf = null;
            OrientedAnimation testSprite = null;


            Matrix matrix = Matrix.Identity;
            matrix.Translation = position;
            testDwarf = new PhysicsComponent(componentManager, "walker", componentManager.RootComponent, matrix,  new Vector3(0.5f, 0.15f, 0.5f), new Vector3(0.0f, -0.25f, 0.0f),  1.0f, 1.0f, 0.999f, 0.999f, new Vector3(0, -10, 0));
            testDwarf.OrientWithVelocity = true;

            Texture2D dwarfSprites = content.Load<Texture2D>(spritesheet);

            List<Point> dwarfOffsets = new List<Point>();
            dwarfOffsets.Add(new Point(0, 0));

            Point offset = dwarfOffsets[RandomHelper.random.Next(0, dwarfOffsets.Count)];


            List<Point> forwardFrames = new List<Point>();
            forwardFrames.Add(new Point(0 + offset.X, 0 + offset.Y));
            forwardFrames.Add(new Point(1 + offset.X, 0+ offset.Y));
            forwardFrames.Add(new Point(2 + offset.X, 0+ offset.Y));
            forwardFrames.Add(new Point(1 + offset.X, 0 + offset.Y));
            forwardFrames.Add(new Point(0 + offset.X, 0 + offset.Y));
            forwardFrames.Add(new Point(1 + offset.X, 0 + offset.Y));
            forwardFrames.Add(new Point(2 + offset.X, 0 + offset.Y));
            forwardFrames.Add(new Point(3 + offset.X, 0 + offset.Y));

            Animation dwarfWalkForward = new Animation(graphics, dwarfSprites, "dwarf_walk_forward", 28, 35, forwardFrames, true, Color.White, 10.0f, 0.8f, 1, false);


            List<Point> rightFrames = new List<Point>();
            rightFrames.Add(new Point(0 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(1 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(2 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(1 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(0 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(1 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(2 + offset.X, 2 + offset.Y));
            rightFrames.Add(new Point(3 + offset.X, 2 + offset.Y));

            Animation dwarfWalkRight = new Animation(graphics, dwarfSprites, "dwarf_walk_right", 28, 35, rightFrames, true, Color.White, 10.0f, 0.8f, 1, false);

            List<Point> leftFrames = new List<Point>();
            leftFrames.Add(new Point(0 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(1 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(2 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(1 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(0 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(1 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(2 + offset.X, 1 + offset.Y));
            leftFrames.Add(new Point(3 + offset.X, 1 + offset.Y));

            Animation dwarfWalkLeft = new Animation(graphics, dwarfSprites, "dwarf_walk_left", 28, 35, leftFrames, true, Color.White, 10.0f, 0.8f, 1, false);

            List<Point> backFrames = new List<Point>();
            backFrames.Add(new Point(0 + offset.X, 3 + offset.Y));
            backFrames.Add(new Point(1 + offset.X, 3 + offset.Y));
            backFrames.Add(new Point(2 + offset.X, 3 + offset.Y));
            backFrames.Add(new Point(1 + offset.X, 3 + offset.Y));

            Animation dwarfWalkBack = new Animation(graphics, dwarfSprites, "dwarf_walk_back", 28, 35, backFrames, true, Color.White, 10.0f, 0.8f, 1, false);



            List<Point> forwardFramesIdle = new List<Point>();
            forwardFramesIdle.Add(new Point(1 + offset.X, 0 + offset.Y));
            forwardFramesIdle.Add(new Point(3 + offset.X, 0 + offset.Y));
            forwardFramesIdle.Add(new Point(1 + offset.X, 0 + offset.Y));

            Animation dwarfIdleForward = new Animation(graphics, dwarfSprites, "dwarf_idle_forward", 28, 35, forwardFramesIdle, true, Color.White, 2.0f, 0.8f, 1, false);


            List<Point> rightFramesIdle = new List<Point>();
            rightFramesIdle.Add(new Point(2 + offset.X, 2 + offset.Y));
            rightFramesIdle.Add(new Point(0 + offset.X, 2 + offset.Y));
            rightFramesIdle.Add(new Point(2 + offset.X, 2 + offset.Y));


            Animation dwarfIdleRight = new Animation(graphics, dwarfSprites, "dwarf_idle_right", 28, 35, rightFramesIdle, true, Color.White, 2.0f, 0.8f, 1, false);

            List<Point> leftFramesIdle = new List<Point>();
            leftFramesIdle.Add(new Point(1 + offset.X, 1 + offset.Y));
            leftFramesIdle.Add(new Point(3 + offset.X, 1 + offset.Y));
            leftFramesIdle.Add(new Point(1 + offset.X, 1 + offset.Y));

            Animation dwarfIdleLeft = new Animation(graphics, dwarfSprites, "dwarf_idle_left", 28, 35, leftFramesIdle, true, Color.White, 2.0f, 0.8f, 1, false);

            List<Point> backFramesIdle = new List<Point>();
            backFramesIdle.Add(new Point(1 + offset.X, 3 + offset.Y));

            Animation dwarfIdleBack = new Animation(graphics, dwarfSprites, "dwarf_idle_back", 28, 35, backFramesIdle, true, Color.White, 2.0f, 0.8f, 1, false);

            dwarfWalkForward.Play();
            dwarfWalkBack.Play();
            dwarfWalkLeft.Play();
            dwarfWalkRight.Play();
            dwarfIdleForward.Play();
            dwarfIdleBack.Play();
            dwarfIdleLeft.Play();
            dwarfIdleRight.Play();

            Matrix spriteMatrix = Matrix.Identity;
            spriteMatrix.Translation = new Vector3(0, 0.1f, 0);
            testSprite = new OrientedAnimation(componentManager, "testsprite", testDwarf, spriteMatrix, dwarfSprites, dwarfWalkRight, dwarfWalkLeft, dwarfWalkForward, dwarfWalkBack,
                dwarfIdleRight, dwarfIdleLeft, dwarfIdleForward, dwarfIdleBack);

            Matrix shadowTransform = Matrix.CreateRotationX((float)Math.PI * 0.5f);
            shadowTransform.Translation = new Vector3(0.0f, -0.5f, 0.0f);
            BillboardSpriteComponent shadow = new BillboardSpriteComponent(componentManager, "shadow", testDwarf, shadowTransform, content.Load<Texture2D>("shadowcircle"), false);
            shadow.OrientsToCamera = false;
            List<Point> shP = new List<Point>();
            shP.Add(new Point(0, 0));
            Animation shadowAnimation = new Animation(graphics, content.Load<Texture2D>("shadowcircle"), "sh", 32, 32, shP, false, Color.Black, 1, 0.7f, 0.7f, false);
            shadow.AddAnimation(shadowAnimation);
            shadowAnimation.Play();
            shadow.SetCurrentAnimation("sh");

            testDwarf.Tags.Add("Walker");


            return testDwarf;
        }
    }
}
