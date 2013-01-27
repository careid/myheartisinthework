using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace HeartGame
{
    public class ComponentManager
    {
        public Dictionary<uint, GameComponent> Components { get; set; }
        private List<GameComponent> Removals { get; set; }
        private List<GameComponent> Additions { get; set; }
        public LocatableComponent RootComponent { get; set; }
        private static Camera m_camera { get; set; }
        public Mutex AdditionMutex { get; set; }
        public Mutex RemovalMutex { get; set; }
        public double DrawDistanceSquared = 1000;

        public ComponentManager()
        {
            Components = new Dictionary<uint, GameComponent>();
            Removals = new List<GameComponent>();
            Additions = new List<GameComponent>();
            m_camera = null;
            AdditionMutex = new Mutex();
            RemovalMutex = new Mutex();
        }


        #region picking

        public static List<T> FilterComponentsWithoutTag<T>(string tag, List<T> toFilter) where T : GameComponent
        {
            List<T> toReturn = new List<T>();

            foreach (T component in toFilter)
            {
                if (!component.Tags.Contains(tag))
                {
                    toReturn.Add(component);
                }
            }

            return toReturn;
        }

        public static List<T> FilterComponentsWithTag<T>(string tag, List<T> toFilter) where T : GameComponent
        {
            List<T> toReturn = new List<T>();

            foreach(T component in toFilter)
            {
                if(component.Tags.Contains(tag))
                {
                    toReturn.Add(component);
                }
            }

            return toReturn;
        }

        public bool IsUnderMouse(LocatableComponent component, MouseState mouse, Camera camera, Viewport viewPort)
        {
            List<LocatableComponent> viewable = new List<LocatableComponent>();
            Vector3 pos1 = viewPort.Unproject(new Vector3(mouse.X, mouse.Y, 0), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            Vector3 pos2 = viewPort.Unproject(new Vector3(mouse.X, mouse.Y, 1), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            Ray toCast = new Ray(pos1, dir);

            return component.Intersects(toCast);
        }


        public void GetComponentsUnderMouse(MouseState mouse,  Camera camera, Viewport viewPort, List<LocatableComponent> components)
        {

            Vector3 pos1 = viewPort.Unproject(new Vector3(mouse.X, mouse.Y, 0), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            Vector3 pos2 = viewPort.Unproject(new Vector3(mouse.X, mouse.Y, 1), camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            Vector3 dir = Vector3.Normalize(pos2 - pos1);

            Ray toCast = new Ray(pos1, dir);
            HashSet<LocatableComponent> set = new HashSet<LocatableComponent>();
            LocatableComponent.m_octree.Root.GetComponentsIntersecting<LocatableComponent>(toCast, set);

            components.AddRange(set);
        }

        public bool IsVisibleToCamera(LocatableComponent component, Camera camera)
        {
            BoundingFrustum frustrum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
            return (component.Intersects(frustrum));
        }

        public void GetComponentsVisibleToCamera(Camera camera, List<LocatableComponent> components)
        {
            BoundingFrustum frustrum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
            GetComponentsIntersecting(frustrum, components);
        }

        public void GetComponentsInVisibleToCamera(Camera camera, List<LocatableComponent> components)
        {
            BoundingFrustum frustrum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);

            foreach (GameComponent c in Components.Values)
            {
                if (c is LocatableComponent && !((LocatableComponent)c).Intersects(frustrum))
                {
                    components.Add((LocatableComponent)c);
                }
            }
        }

        public void GetComponentsIntersecting(BoundingSphere sphere, List<LocatableComponent> components)
        {
            HashSet<LocatableComponent> set = new HashSet<LocatableComponent>();
            LocatableComponent.m_octree.Root.GetComponentsIntersecting<LocatableComponent>(sphere, set);

            components.AddRange(set);
        }

        public void GetComponentsIntersecting(BoundingFrustum frustrum, List<LocatableComponent> components)
        {
            HashSet<LocatableComponent> set = new HashSet<LocatableComponent>();
            LocatableComponent.m_octree.Root.GetComponentsIntersecting<LocatableComponent>(frustrum, set);

            components.AddRange(set);
        }

        public void GetComponentsIntersecting(BoundingBox box, List<LocatableComponent> components)
        {
            HashSet<LocatableComponent> set = new HashSet<LocatableComponent>();
            LocatableComponent.m_octree.Root.GetComponentsIntersecting(box, set);

            components.AddRange(set);
        }

        public void GetComponentsIntersecting(Ray ray, List<LocatableComponent> components)
        {
            HashSet<LocatableComponent> set = new HashSet<LocatableComponent>();
            LocatableComponent.m_octree.Root.GetComponentsIntersecting<LocatableComponent>(ray, set);

            components.AddRange(set);
        }
        #endregion

        public void AddComponent(GameComponent component)
        {
            AdditionMutex.WaitOne();
            Additions.Add(component);
            AdditionMutex.ReleaseMutex();
        }

        public void RemoveComponent(GameComponent component)
        {
            RemovalMutex.WaitOne();
            Removals.Add(component);
            RemovalMutex.ReleaseMutex();
        }

        private void RemoveComponentImmediate(GameComponent component)
        {
            Components.Remove(component.GlobalID);

            List<GameComponent> children = component.GetAllChildrenRecursive();

            foreach (GameComponent child in children)
            {
                Components.Remove(child.GlobalID);
            }
        }

        private void AddComponentImmediate(GameComponent component)
        {
            Components[component.GlobalID] = component;
        }

        public void Update(GameTime gameTime, Camera camera)
        {

            if (RootComponent != null)
            {
                RootComponent.UpdateTransformsRecursive();
            }

            List<GameComponent> Removals = new List<GameComponent>();

            foreach (GameComponent component in Components.Values)
            {
                if (component.IsActive)
                {
                    component.Update(gameTime, camera);
                }

                if (component.IsDead)
                {
                    Removals.Add(component);
                    component.IsActive = false;
                    component.IsDead = true;
                }
            }


            AdditionMutex.WaitOne();
            foreach (GameComponent component in Additions)
            {
                AddComponentImmediate(component);
            }

            Additions.Clear();
            AdditionMutex.ReleaseMutex();

            RemovalMutex.WaitOne();
            foreach (GameComponent component in Removals)
            {
                RemoveComponentImmediate(component);
            }

            Removals.Clear();
            RemovalMutex.ReleaseMutex();
            
        }

        public HashSet<LocatableComponent> FrustrumCullLocatableComponents(Camera camera)
        {
            HashSet<LocatableComponent> visible = new HashSet<LocatableComponent>();
            LocatableComponent.m_octree.Root.GetComponentsIntersecting<LocatableComponent>(camera.GetFrustrum(), visible);

            return visible;
        }


        HashSet<LocatableComponent> visibleComponents = new HashSet<LocatableComponent>();
        List<LocatableComponent> zSorted = new List<LocatableComponent>();
        List<GameComponent> nonLocatable = new List<GameComponent>();
        public void Render(GameTime gameTime,
                                    Camera camera,
                                    SpriteBatch spriteBatch,
                                    GraphicsDevice graphicsDevice,
                                    Effect effect, 
                                    bool renderingForWater)
        {


            if (!renderingForWater)
            {
                zSorted.Clear();
                visibleComponents.Clear();
                nonLocatable.Clear();
                visibleComponents = FrustrumCullLocatableComponents(camera);

                m_camera = camera;
                foreach (GameComponent component in Components.Values)
                {
                    bool isLocatable = component is LocatableComponent;

                    if (isLocatable)
                    {
                        LocatableComponent loc = (LocatableComponent)component;

                        
                        if (((loc.GlobalTransform.Translation - camera.Position).LengthSquared() < DrawDistanceSquared &&
                            (visibleComponents.Contains(component) || visibleComponents.Contains(component.Parent as LocatableComponent)) || !(loc.FrustrumCull)))

                        {
                            if (loc != null && loc.DepthSort)
                            {
                                zSorted.Add(loc);
                            }
                            else if (loc != null)
                            {
                                nonLocatable.Add(component);
                            }
                        }
                    }
                    else
                    {
                        nonLocatable.Add(component);
                    }
                }

                zSorted.Sort(CompareZDepth);
            }



            foreach (GameComponent component in zSorted)
            {
                component.Render(gameTime, camera, spriteBatch, graphicsDevice, effect);
            }

            foreach (GameComponent component in nonLocatable)
            {
                component.Render(gameTime,camera, spriteBatch, graphicsDevice, effect);
            }

        }

        public static int CompareZDepth(LocatableComponent A, LocatableComponent B)
        {

            if (A == B)
            {
                return 0;
            }

            else if (A.Parent == B.Parent && A.DrawInFrontOfSiblings)
            {
                return 1;
            }
            else if (B.Parent == A.Parent && B.DrawInFrontOfSiblings)
            {
                return -1;
            }
            else if ((m_camera.Position - A.GlobalTransform.Translation).LengthSquared() < (m_camera.Position - B.GlobalTransform.Translation).LengthSquared())
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }


    }
}
