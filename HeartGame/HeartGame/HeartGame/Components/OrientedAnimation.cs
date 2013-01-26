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
            Left,
            Forward,
            Backward
        }

        public Dictionary<Orientation, Animation> OrientationMap { get; set; }
        public Dictionary<Orientation, Animation> IdleMap { get; set; }

        public Orientation CurrentOrientation { get; set; }

        public bool IsIdle { get; set; }

        public OrientedAnimation(ComponentManager manager, string name,
            GameComponent parent, Matrix localTransform, Texture2D spriteSheet,
            Animation rightAnimation, Animation leftAnimation, Animation forwardAnimation, Animation backwardAnimation,
            Animation rightAnimationIdle, Animation leftAnimationIdle, Animation forwardAnimationIdle, Animation backwardAnimationIdle) :
            base(manager, name, parent, localTransform, spriteSheet, false)
        {
            OrientationMap = new Dictionary<Orientation, Animation>();
            IdleMap = new Dictionary<Orientation, Animation>();
            OrientationMap[Orientation.Right] = rightAnimation;
            OrientationMap[Orientation.Left] = leftAnimation;
            OrientationMap[Orientation.Forward] = forwardAnimation;
            OrientationMap[Orientation.Backward] = backwardAnimation;
            IdleMap[Orientation.Right] = rightAnimationIdle;
            IdleMap[Orientation.Left] = leftAnimationIdle;
            IdleMap[Orientation.Forward] = forwardAnimationIdle;
            IdleMap[Orientation.Backward] = backwardAnimationIdle;

            AddAnimation(rightAnimation);
            AddAnimation(leftAnimation);
            AddAnimation(forwardAnimation);
            AddAnimation(backwardAnimation);

            AddAnimation(rightAnimationIdle);
            AddAnimation(leftAnimationIdle);
            AddAnimation(forwardAnimationIdle);
            AddAnimation(backwardAnimationIdle);


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



            if (angle > -MathHelper.PiOver4 && angle < MathHelper.PiOver4)
            {
                CurrentOrientation = Orientation.Left;
            }
            else if (angle > MathHelper.PiOver4 && angle < 3.0f * MathHelper.PiOver4)
            {
                CurrentOrientation = Orientation.Backward;
            }
            else if ((angle > 3.0f * MathHelper.PiOver4 ||  angle < -3.0f * MathHelper.PiOver4))
            {
                CurrentOrientation = Orientation.Right;
            }
            else
            {
                CurrentOrientation = Orientation.Forward;
            }
        }
        
    }
}
