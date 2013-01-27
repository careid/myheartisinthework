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

        public bool IsIdle { get; set; }

        public OrientedAnimation(ComponentManager manager, string name,
            GameComponent parent, Matrix localTransform, Texture2D spriteSheet,
            Animation rightAnimation, Animation leftAnimation,
            Animation rightAnimationIdle, Animation leftAnimationIdle) :
            base(manager, name, parent, localTransform, spriteSheet, false)
        {
            OrientationMap = new Dictionary<Orientation, Animation>();
            IdleMap = new Dictionary<Orientation, Animation>();
            OrientationMap[Orientation.Right] = rightAnimation;
            OrientationMap[Orientation.Left] = leftAnimation;
            IdleMap[Orientation.Right] = rightAnimationIdle;
            IdleMap[Orientation.Left] = leftAnimationIdle;
            
            AddAnimation(rightAnimation);
            AddAnimation(leftAnimation);
            
            AddAnimation(rightAnimationIdle);
            AddAnimation(leftAnimationIdle);

            foreach (Animation a in Animations.Values)
            {
                a.Play();
            }

            IsIdle = false;
        }

        public override void Update(GameTime gameTime,  Camera camera)
        {
            CalculateCurrentOrientation(camera);

            if (!IsIdle)
            {
                CurrentAnimation = OrientationMap[CurrentOrientation];
            }
            else
            {
                CurrentAnimation = IdleMap[CurrentOrientation];
            }

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
        
    }
}
