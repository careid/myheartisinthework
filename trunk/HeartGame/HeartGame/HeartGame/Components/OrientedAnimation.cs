using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class OrientedAnimation
    {
        public enum Orientation
        {
            Right,
            Left
        }

        public Animation RightAnimation { get; set; }
        public Animation LeftAnimation { get; set; }
        public String Name;
        public Orientation CurrentOrientation { get; set; }

        public OrientedAnimation(Animation leftAnimation, Animation rightAnimation)
        {
            RightAnimation = rightAnimation;
            LeftAnimation = leftAnimation;
            CurrentOrientation = Orientation.Left;
            Name = leftAnimation.Name + "," + rightAnimation.Name;
        }

        public OrientedAnimation(Animation leftAnimation)
        {
            LeftAnimation = leftAnimation;
            RightAnimation = null;
            CurrentOrientation = Orientation.Left;
            Name = leftAnimation.Name;
        }

        public bool Oriented()
        {
            return RightAnimation == null;
        }

        public Animation GetAnimation()
        {
            if (CurrentOrientation == Orientation.Left || RightAnimation == null)
                return LeftAnimation;
            else
                return RightAnimation;
        }

        public void SetLeft()
        {
            CurrentOrientation = Orientation.Left;
        }

        public void SetRight()
        {
            CurrentOrientation = Orientation.Right;
        }

        public IEnumerator<Animation> GetEnumerator()
        {
            List<Animation> anims = new List<Animation>(2);
            anims.Add(LeftAnimation);
            if (RightAnimation != null)
                anims.Add(RightAnimation);
            return anims.GetEnumerator();
        }
    }
    public class OrientableBillboardSpriteComponent : BillboardSpriteComponent
    {
        public Dictionary<string, OrientedAnimation> OrientedAnimations { get; set; }
        public OrientedAnimation CurrentOrientedAnimation { get; set; }

        public OrientableBillboardSpriteComponent(ComponentManager manager, string name,
            GameComponent parent, Matrix localTransform, Texture2D spriteSheet) :
            base(manager, name, parent, localTransform, spriteSheet, false)
        {
            OrientedAnimations = new Dictionary<string, OrientedAnimation>();
        }

        public void AddOrientedAnimation(OrientedAnimation animation)
        {
            if (CurrentOrientedAnimation == null)
            {
                CurrentOrientedAnimation = animation;
            }
            foreach (Animation anim in animation)
            {
                AddAnimation(anim);
            }
            OrientedAnimations[animation.Name] = animation;
        }
        public void AddOrientedAnimation(Animation animation)
        {
            AddOrientedAnimation(new OrientedAnimation(animation));
        }

        public void AddOrientedAnimation(Animation leftAnimation, Animation rightAnimation)
        {
            AddOrientedAnimation(new OrientedAnimation(leftAnimation, rightAnimation));
        }

        public OrientedAnimation GetOrientedAnimation(string name)
        {
            if (OrientedAnimations.ContainsKey(name))
            {
                return OrientedAnimations[name];
            }

            return null;
        }

        public void SetCurrentOrientedAnimation(string name)
        {
            OrientedAnimation anim = GetOrientedAnimation(name);

            if (anim != null)
            {
                CurrentOrientedAnimation = anim;
                CurrentAnimation = anim.GetAnimation();
            }
        }

        public override void Update(GameTime gameTime,  Camera camera)
        {
            if (CurrentOrientedAnimation.Oriented())
            {
                CalculateCurrentOrientation(camera);
                CurrentAnimation = CurrentOrientedAnimation.GetAnimation();
            }

            base.Update(gameTime, camera);
        }

        public void CalculateCurrentOrientation(Camera camera)
        {
            float xComponent = Vector3.Dot(camera.ViewMatrix.Forward, GlobalTransform.Left);
            float yComponent = Vector3.Dot(camera.ViewMatrix.Forward, GlobalTransform.Forward);

            float angle = (float)Math.Atan2(yComponent, xComponent);

            if (angle > -MathHelper.PiOver2 + 0.01f && angle < MathHelper.PiOver2 - 0.01f)
            {
                CurrentOrientedAnimation.SetLeft();
            }
            else if (angle < -MathHelper.PiOver2 - 0.01f || angle > MathHelper.PiOver2 + 0.01f)
            {
                CurrentOrientedAnimation.SetRight();
            }
        }
    }
}
