using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class OrientedAnimation : BillboardSpriteComponent
    {
        public enum Orientation
        {
            Right,
            Left
        }

        public Dictionary<Orientation, Animation> OrientationMap { get; set; }
        public Dictionary<Orientation, Animation> IdleMap { get; set; }

        public Orientation CurrentOrientation { get; set; }

        public OrientedAnimation(ComponentManager manager, string name,
            GameComponent parent, Matrix localTransform, Texture2D spriteSheet,
            Animation rightAnimation, Animation leftAnimation) :
            base(manager, name, parent, localTransform, spriteSheet, false)
        {
            OrientationMap = new Dictionary<Orientation, Animation>();
            OrientationMap[Orientation.Right] = rightAnimation;
            OrientationMap[Orientation.Left] = leftAnimation;
            
            AddAnimation(rightAnimation);
            AddAnimation(leftAnimation);

            foreach (Animation a in Animations.Values)
            {
                a.Play();
            }
        }

        public override void Update(GameTime gameTime,  Camera camera)
        {
            CalculateCurrentOrientation(camera);

            CurrentAnimation = OrientationMap[CurrentOrientation];

            base.Update(gameTime, camera);
        }

        public void CalculateCurrentOrientation(Camera camera)
        {
            float xComponent = Vector3.Dot(camera.ViewMatrix.Forward, GlobalTransform.Left);
            float yComponent = Vector3.Dot(camera.ViewMatrix.Forward, GlobalTransform.Forward);

            float angle = (float)Math.Atan2(yComponent, xComponent);



            if (angle > -MathHelper.PiOver2 && angle < MathHelper.PiOver2)
            {
                CurrentOrientation = Orientation.Left;
            }
            else
            {
                CurrentOrientation = Orientation.Right;
            }
        }

        public void Transform(Animation rightAnimation, Animation leftAnimation)
        {
            OrientationMap = new Dictionary<Orientation, Animation>();
            OrientationMap[Orientation.Right] = rightAnimation;
            OrientationMap[Orientation.Left] = leftAnimation;

            Animations = new Dictionary<string, Animation>();

            AddAnimation(rightAnimation);
            AddAnimation(leftAnimation);

            foreach (Animation a in Animations.Values)
            {
                a.Play();
            }
        }
        
    }
}
