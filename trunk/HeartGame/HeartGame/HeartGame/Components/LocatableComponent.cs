using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class LocatableComponent : GameComponent, BoundedObject
    {
        public Matrix GlobalTransform
        { 
            get
            {
                return m_globalTransform;
            }
            set
            {
                m_globalTransform = value;

                if (AddToOctree)
                {
                    if (IsActive && HasMoved && (m_octree.Root.NeedsUpdateRecursive(this) || !m_octree.Root.ExistsInTreeRecursive(this)))
                    {
                        m_octree.Root.RemoveObjectRecursive(this);
                        m_octree.Root.AddObjectRecursive(this);
                    }
                }
            }
        }
        public Matrix LocalTransform 
        { 
            get 
            { 
                return m_localTransform; 
            }
            
            set 
            { 
                m_localTransform = value;
                HasMoved = true;
            } 
        }
        public BoundingBox BoundingBox { get; set; }
        public Vector3 BoundingBoxPos { get; set; }
        public bool DrawBoundingBox { get; set; }
        public bool DepthSort { get; set; }
        public bool FrustrumCull { get; set; }
        public bool DrawInFrontOfSiblings { get; set; }
        public bool IsStocked { get; set; }
        public static Octree m_octree = new Octree(new BoundingBox(new Vector3(-1000, -1000, -1000), new Vector3(1000, 1000, 1000)), 10, 50, 2);
        public bool HasMoved 
        {
            get
            {
                return m_hasMoved;
            }
            set
            {
                m_hasMoved = value;

                if (AddToOctree)
                {
                    foreach (GameComponent child in Children.Values)
                    {
                        if (child is LocatableComponent)
                        {
                            ((LocatableComponent)child).HasMoved = value;
                        }
                    }
                }
            }
        }


        protected Matrix m_localTransform = Matrix.Identity;
        protected Matrix m_globalTransform = Matrix.Identity;
        private bool m_hasMoved = true;
        public bool AddToOctree { get; set; }

        public LocatableComponent(ComponentManager manager, string name, GameComponent parent, Matrix localTransform, Vector3 boundingBoxExtents, Vector3 boundingBoxPos) :
            this(manager, name, parent, localTransform, boundingBoxExtents, boundingBoxPos, true)
        {
            DrawInFrontOfSiblings = false;
        }
        public LocatableComponent(ComponentManager manager, string name, GameComponent parent, Matrix localTransform,  Vector3 boundingBoxExtents, Vector3 boundingBoxPos, bool addToOctree) :
            base(manager, name, parent)
        {
            AddToOctree = addToOctree;
            BoundingBoxPos = boundingBoxPos;
            DrawBoundingBox = false;
            BoundingBox = new BoundingBox(localTransform.Translation - boundingBoxExtents / 2.0f + boundingBoxPos, localTransform.Translation + boundingBoxExtents / 2.0f + boundingBoxPos);

            LocalTransform = localTransform;
            HasMoved = true;
            DepthSort = true;
            FrustrumCull = true;
            DrawInFrontOfSiblings = false;
            IsStocked = false;

        }


        public bool Intersects(BoundingSphere sphere)
        {
            return (sphere.Intersects(BoundingBox));
        }


        public bool Intersects(BoundingFrustum fr)
        {
            return (fr.Intersects(BoundingBox));
        }

        public bool Intersects(BoundingBox box)
        {
            return (box.Intersects(BoundingBox));
        }

        public bool Intersects(Ray ray)
        {
            return (ray.Intersects(BoundingBox) != null);
        }

        public override void Update(GameTime gameTime, Camera camera)
        {


            base.Update(gameTime, camera);
        }

        public void UpdateTransformsRecursive()
        {
            if (Parent is LocatableComponent)
            {
                LocatableComponent locatable = (LocatableComponent)Parent;

                if (HasMoved)
                {
                    GlobalTransform = LocalTransform * locatable.GlobalTransform;
                    m_hasMoved = false;
                }

            }
            else
            {
                if (HasMoved)
                {
                    GlobalTransform = LocalTransform;
                    m_hasMoved = false;
                }
            }

          
            foreach(KeyValuePair<uint, GameComponent> pair in Children)
            {
                if (pair.Value is LocatableComponent)
                {
                    LocatableComponent locatable = (LocatableComponent)pair.Value;

                    if (locatable.HasMoved)
                    {
                        locatable.UpdateTransformsRecursive();
                    }
                }
            }

            UpdateBoundingBox();

        }

        public BoundingBox GetBoundingBox()
        {
            return BoundingBox;
        }

        public override void Render(GameTime gameTime,  Camera camera, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Effect effect)
        {
            
            if (DrawBoundingBox)
            {
                SimpleDrawing.DrawBox(BoundingBox, Color.White, 0.02f);
            }
             
            base.Render(gameTime, camera, spriteBatch, graphicsDevice, effect);
        }

        public void UpdateBoundingBox()
        {
            Vector3 extents = BoundingBox.Max - BoundingBox.Min;
            BoundingBox = new BoundingBox(GlobalTransform.Translation - extents / 2.0f + BoundingBoxPos, GlobalTransform.Translation + extents / 2.0f + BoundingBoxPos);
        }

        public override void Die()
        {
            UpdateBoundingBox();
            if (AddToOctree)
            {
                m_octree.Root.RemoveObjectRecursive(this);
            }
            IsActive = false;
            IsVisible = false;
            HasMoved = false;
           
            base.Die();
        }
    }
}
