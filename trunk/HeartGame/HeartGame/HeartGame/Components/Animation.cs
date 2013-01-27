using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class Animation
    {
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public string Name { get; set; }
        public List<Point> Frames { get; set; }
        public int CurrentFrame { get; set; }
        public bool IsPlaying { get; set; }
        public bool Loops { get; set; }
        public Color Tint { get; set; }
        public float FrameHZ { get; set; }
        private float FrameTimer { get; set; }
        public float WorldWidth { get; set; }
        public float WorldHeight { get; set; }
        public bool Flipped { get; set; }
        public List<BillboardPrimitive> Primitives { get; set; }
        public Texture2D SpriteSheet { get; set; }


        public Animation(GraphicsDevice device, Texture2D sheet, string name, int frameWidth, int frameHeight, List<Point> frames, bool loops, Color tint, float frameHZ, float worldWidth, float worldHeight, bool flipped)
        {
            Name = name;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            Frames = frames;
            CurrentFrame = 0;
            IsPlaying = false;
            Loops = loops;
            Tint = tint;
            FrameHZ = frameHZ + (float)RandomHelper.random.NextDouble();
            FrameTimer = 0.0f;
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            SpriteSheet = sheet;
            Primitives = new List<BillboardPrimitive>();
            Flipped = flipped;

            CreatePrimitives(device);

        }


        public void CreatePrimitives(GraphicsDevice device)
        {
            foreach (Point frame in Frames)
            {
                string key = Name + ": " + FrameWidth + "," + FrameHeight + frame.ToString() + "," + Flipped + "," + SpriteSheet.GetHashCode();
                if (!PrimitiveLibrary.BillboardPrimitives.ContainsKey(key))
                {
                    PrimitiveLibrary.BillboardPrimitives[key] = new BillboardPrimitive(device, SpriteSheet, FrameWidth, FrameHeight, frame, WorldWidth, WorldHeight, Flipped);
                }

                Primitives.Add(PrimitiveLibrary.BillboardPrimitives[key]);
            }
        }

        public Rectangle GetCurrentFrameRect()
        {
            Rectangle toReturn = new Rectangle(Frames[CurrentFrame].X, Frames[CurrentFrame].Y, FrameWidth, FrameHeight);
            return toReturn;
        }

        public void Reset()
        {
            CurrentFrame = 0;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
            CurrentFrame = 0;
        }

        public void Loop()
        {
            IsPlaying = true;
            Loops = true;
        }

        public void StopLooping()
        {
            IsPlaying = false;
            Loops = false;
        }



        public void Update(GameTime gameTime)
        {
            if (IsPlaying)
            {
                FrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (FrameTimer >= 1.0f / FrameHZ)
                {
                    NextFrame();
                    FrameTimer = 0.0f;
                }
            }

        }

        public void NextFrame()
        {
            CurrentFrame++;

            if (CurrentFrame >= Frames.Count)
            {
                if (Loops)
                {
                    CurrentFrame = 0;
                }
                else
                {
                    CurrentFrame = Frames.Count - 1;
                }
            }
        }
    }

}
