using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HeartGame
{

    public interface BoundedObject
    {
        BoundingBox GetBoundingBox();
    }

    public class Octree
    {
        public int MaxDepth { get; set; }
        public int MaxObjectsPerNode { get; set; }
        public int MinObjectsPerNode { get; set; }
        public BoundingBox Bounds { get; set; }
        public OctreeNode Root { get; set; }
        public bool DebugDraw { get; set; }

        public Octree(BoundingBox bounds, int maxDepth, int maxObjectsPerNode, int minObjectsPerNode)
        {
            Bounds = bounds;
            MaxDepth = maxDepth;
            MaxObjectsPerNode = maxObjectsPerNode;
            MinObjectsPerNode = minObjectsPerNode;
            Root = new OctreeNode(Bounds, this, 0, null);
            DebugDraw = false;
        }

        public void ExpandAndRebuild()
        {
            List<BoundedObject> objectsInTree = Root.MergeRecursive();

            BoundingBox bounds = new BoundingBox(Bounds.Min * 2.0f, Bounds.Max * 2.0f);
            Bounds = bounds;

            
            Root = new OctreeNode(Bounds, this, 0, null);

            MaxDepth++;

   

            foreach (BoundedObject loc in objectsInTree)
            {
                Root.AddObjectRecursive(loc);
            }

        }
    }

    public class OctreeNode
    {


        public enum NodeType
        {
            UpperBackRight,
            UpperBackLeft,
            UpperFrontRight,
            UpperFrontLeft,
            LowerBackRight,
            LowerBackLeft,
            LowerFrontRight,
            LowerFrontLeft
        }

        public OctreeNode[] Children = { null, null, null, null, null, null, null, null };

        public BoundingBox Bounds { get; set; }

        public HashSet<BoundedObject> Objects { get; set; }

        public Octree Tree { get; set; }

        public OctreeNode Parent { get; set; }

        public int Depth { get; set; }

        public bool HasChild(NodeType node)
        {
            return Children[(int)node] != null;
        }

        public bool HasChildren()
        {
            for (int i = 0; i < 8; i++)
            {
                if (HasChild((NodeType)i))
                {
                    return true;
                }
            }

            return false;
        }

        public Mutex NodeMutex { get; set; }

        public OctreeNode(BoundingBox bounds, Octree tree, int depth, OctreeNode parent)
        {
            Bounds = bounds;
            Objects = new HashSet<BoundedObject>();
            Tree = tree;
            Parent = parent;
            Depth = depth;
            NodeMutex = new Mutex();
        }


        public T GetComponentIntersecting<T>(Vector3 vect) where T : BoundedObject
        {
            NodeMutex.WaitOne();
            if (Bounds.Contains(vect) != ContainmentType.Disjoint)
            {
                foreach (BoundedObject o in Objects)
                {
                    if (o is T && o.GetBoundingBox().Contains(vect) != ContainmentType.Disjoint)
                    {
                        NodeMutex.ReleaseMutex();
                        return (T)o;
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = Children[i];
                    if (child != null)
                    {
                        T got = child.GetComponentIntersecting<T>(vect);

                        if (got != null)
                        {
                            NodeMutex.ReleaseMutex();
                            return got;
                        }
                    }
                }
            }


            NodeMutex.ReleaseMutex();
            return default(T);
        }

        public void GetComponentsIntersecting<T>(BoundingBox box, HashSet<T> set) where T : BoundedObject
        {
            NodeMutex.WaitOne();
            if (Bounds.Intersects(box) || Bounds.Contains(box) != ContainmentType.Disjoint)
            {
                foreach (BoundedObject o in Objects)
                {
                    if (o is T && o.GetBoundingBox().Intersects(box))
                    {
                        set.Add((T)o);
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = Children[i];
                    if (child != null)
                    {
                        child.GetComponentsIntersecting(box, set);
                    }
                }

            }

            NodeMutex.ReleaseMutex();


        }

        public void GetComponentsIntersecting<T>(BoundingSphere sphere, HashSet<T> set) where T : BoundedObject
        {
            NodeMutex.WaitOne();
            if (Bounds.Intersects(sphere) || Bounds.Contains(sphere) != ContainmentType.Disjoint)
            {
                foreach (BoundedObject o in Objects)
                {
                    if(o is T && o.GetBoundingBox().Intersects(sphere))
                        set.Add((T)o);
                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = Children[i];
                    if (child != null)
                        child.GetComponentsIntersecting(sphere, set);
                }
            }
            NodeMutex.ReleaseMutex();

        }

        public void GetComponentsIntersecting<T>(BoundingFrustum frustrum, HashSet<T> set) where T : BoundedObject
        {
            NodeMutex.WaitOne();
            if (Bounds.Intersects(frustrum) || Bounds.Contains(frustrum) != ContainmentType.Disjoint)
            {
                foreach (BoundedObject o in Objects)
                {
                    if(o is T && o.GetBoundingBox().Intersects(frustrum))
                        set.Add((T)o);
                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = Children[i];
                    if (child != null)
                        child.GetComponentsIntersecting(frustrum, set);
                }
            }

            NodeMutex.ReleaseMutex();


        }

        public void GetComponentsIntersecting<T>(Ray ray, HashSet<T> set) where T : BoundedObject
        {
            NodeMutex.WaitOne();
            if (ray.Intersects(Bounds) != null || Bounds.Contains(ray.Position) != ContainmentType.Disjoint)
            {
                foreach (BoundedObject o in Objects)
                {
                    if (o is T && ray.Intersects(o.GetBoundingBox()) != null)
                    {
                        set.Add((T)o);
                    }

                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = Children[i];
                    if (child != null)
                    {
                        child.GetComponentsIntersecting(ray, set);
                    }
                }
            }
            NodeMutex.ReleaseMutex();

        }

        public bool ExistsInTreeRecursive(BoundedObject component)
        {
            if (Objects.Contains(component))
            {
                return true;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    OctreeNode node = Children[i];
                    if (node != null)
                    {
                        if (node.ExistsInTreeRecursive(component))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool NeedsUpdateRecursive(BoundedObject component)
        {
            if (Objects.Contains(component))
            {
                if (!component.GetBoundingBox().Intersects(Bounds))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (HasChildren())
            {
                bool shouldUpdate = false;
                for(int i = 0; i < 8; i++)
                {
                    OctreeNode node = Children[i];
                    if (node != null)
                    {
                        if (node.NeedsUpdateRecursive(component))
                        {
                            shouldUpdate = true;
                        }
                    }
                }

                return shouldUpdate;
            }
            else
            {
                return false;
            }
            
        }

        public void AddObjectRecursive(BoundedObject component)
        {
            NodeMutex.WaitOne();
            if (Parent == null && !component.GetBoundingBox().Intersects(Bounds))
            {
                Tree.ExpandAndRebuild();
                NodeMutex.ReleaseMutex();
                return;
            }

            if (component.GetBoundingBox().Intersects(Bounds) && !HasChildren())
            {
                Objects.Add(component);

                if (Objects.Count > Tree.MaxObjectsPerNode && Depth < Tree.MaxDepth)
                {
                    Split();
                }
            }

            else
            {
                for(int i = 0; i < 8; i++)
                {
                    OctreeNode node = Children[i];
                    if (node != null)
                    {
                        node.AddObjectRecursive(component);
                    }

                }
            }

            NodeMutex.ReleaseMutex();
        }

        public bool ContainsObjectRecursive(BoundedObject component)
        {
            NodeMutex.WaitOne();
            if (Objects.Contains(component))
            {
                NodeMutex.ReleaseMutex();
                return true;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    OctreeNode node = Children[i];
                    if (node != null)
                    {
                        if (node.ContainsObjectRecursive(component))
                        {
                            NodeMutex.ReleaseMutex();
                            return true;
                        }
                    }
                }
            }
            NodeMutex.ReleaseMutex();
            return false;
        }

        public bool RemoveObjectRecursive(BoundedObject component)
        {
            NodeMutex.WaitOne();
            if (Objects.Contains(component))
            {
   
                if (CountObjectsRecursive() < Tree.MinObjectsPerNode && HasChildren())
                {
                    MergeRecursive();
                }
                else if (!HasChildren() && CountObjectsRecursive() < Tree.MinObjectsPerNode)
                {
                    Parent.MergeRecursive();
                }


                Objects.Remove(component);

                NodeMutex.ReleaseMutex();
                return true;
            }
            else
            {
                bool toReturn = false;
                for(int i = 0; i < 8; i++)
                {
                    OctreeNode node = Children[i];
                    if (node != null)
                    {
                        toReturn = node.RemoveObjectRecursive(component) || toReturn;
                    }

                }

                Objects.Remove(component);
                NodeMutex.ReleaseMutex();
                return toReturn;
            }
        }

        public List<BoundedObject> MergeRecursive()
        {
            NodeMutex.WaitOne();
            List<BoundedObject> toReturn = new List<BoundedObject>();

            for (int i = 0; i < 8; i++)
            {
                OctreeNode node = Children[i];
                if (node != null)
                {
                    toReturn.AddRange(node.MergeRecursive());
                }
            }

            for (int i = 0; i < 8; i++)
            {
                if (Children[i] != null)
                {
                    Children[i].Objects.Clear();
                }
                Children[i] = null;
            }

            List<BoundedObject> toAdd = new List<BoundedObject>();
            toAdd.AddRange(toReturn);


            toReturn.AddRange(Objects);

            foreach(BoundedObject component in toAdd)
            {
                Objects.Add(component);
            }

            NodeMutex.ReleaseMutex();
            return toReturn;
        }

        public int CountObjectsRecursive()
        {

            int toReturn = Objects.Count;

            for (int i = 0; i < 8; i++)
            {
                OctreeNode node = Children[i];
                if(node != null)
                    toReturn += node.CountObjectsRecursive();
            }

            return toReturn;
        }

        public void Split()
        {
            NodeMutex.WaitOne();
            Vector3 extents = Bounds.Max - Bounds.Min;
            Vector3 xExtents = new Vector3(extents.X, 0, 0) / 2.0f;
            Vector3 yExtents = new Vector3(0, extents.Y, 0) / 2.0f;
            Vector3 zExtents = new Vector3(0, 0, extents.Z) / 2.0f;
            Vector3 halfExtents = extents / 2.0f;
            Vector3 center = Bounds.Min + extents / 2.0f;
            for (int i = 0; i < 8; i++)
            {
                NodeType nodeType = (NodeType)i;

                switch(nodeType)
                {
                    case NodeType.LowerBackLeft:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min, center), Tree, Depth + 1, this);
                        break;

                    case NodeType.LowerBackRight:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + xExtents,
                                                                     Bounds.Min + xExtents + halfExtents), Tree, Depth + 1, this);
                        break;

                    case NodeType.LowerFrontLeft:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + zExtents,
                                                                     Bounds.Min + halfExtents + zExtents), Tree, Depth + 1, this);
                        break;

                    case NodeType.LowerFrontRight:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + zExtents + xExtents,
                                                                     Bounds.Min + halfExtents + zExtents + xExtents), Tree, Depth + 1, this);
                        break;

                    case NodeType.UpperBackLeft:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + yExtents,
                                                                     Bounds.Min + yExtents + halfExtents), Tree, Depth + 1, this);
                        break;

                    case NodeType.UpperBackRight:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + yExtents + xExtents,
                                                                     Bounds.Min + yExtents + xExtents + halfExtents), Tree, Depth + 1, this);
                        break;

                    case NodeType.UpperFrontLeft:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + yExtents + zExtents,
                                                                     Bounds.Min + yExtents + zExtents + halfExtents), Tree, Depth + 1, this);
                        break;

                    case NodeType.UpperFrontRight:
                        Children[i] = new OctreeNode(new BoundingBox(Bounds.Min + yExtents + xExtents + zExtents,
                                             Bounds.Min + yExtents + zExtents + xExtents + halfExtents), Tree, Depth + 1, this);
                        break;
                }


                foreach (BoundedObject o in Objects)
                {
                    Children[i].AddObjectRecursive(o);
                }
            }

            Objects.Clear();

            NodeMutex.ReleaseMutex();
        }

        public void Draw()
        {
            if (Tree.DebugDraw)
            {
                if (Objects.Count > 0 && !HasChildren())
                {
                    SimpleDrawing.DrawBox(Bounds, Color.White, 0.01f);
                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeNode child = Children[i];
                    if (child != null)
                    {
                        child.Draw();
                    }
                }
            }

        }
    }
}