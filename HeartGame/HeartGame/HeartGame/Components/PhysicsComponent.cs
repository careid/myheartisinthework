﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class PhysicsComponent : LocatableComponent
    {
        public Vector3 AngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }
        public float Mass { get; set; }
        public float I { get; set; }
        public float LinearDamping { get; set; }
        public float AngularDamping { get; set; }
        public float Restitution { get; set; }
        public float Friction { get; set; }
        public Vector3 Gravity { get; set; }
        public bool OrientWithVelocity { get; set; }
        public Vector3 PreviousPosition { get; set; }
        private bool m_applyGravityThisFrame = true;
        public bool IsSleeping { get; set; }
        private bool m_overrideSleepThisFrame = true;

        public PhysicsComponent(ComponentManager manager, string name, GameComponent parent, Matrix localTransform,
            Vector3 boundingBoxExtents, Vector3 boundingBoxPos, float mass, float i, float linearDamping, float angularDamping) :
            base(manager, name, parent, localTransform, boundingBoxExtents, boundingBoxPos) 
        {
            Mass = mass;
            Velocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            I = i;
            LinearDamping = linearDamping;
            AngularDamping = angularDamping;
            Gravity = new Vector3(0, -20, 0);
            Restitution = 0.99f;
            Friction = 0.99f;
            OrientWithVelocity = false;
            IsSleeping = false;
            PreviousPosition = LocalTransform.Translation;
        }

        public override void Update(GameTime gameTime, Camera camera)
        {

            if (Velocity.LengthSquared() < 0.1f)
            {
                IsSleeping = true;
            }
            else
            {
                IsSleeping = false;
            }

            float dt = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            if (m_applyGravityThisFrame)
            {
                ApplyForce(Gravity, dt);
            }
            else
            {
                m_applyGravityThisFrame = true;
            }

            Matrix transform = LocalTransform;
            PreviousPosition = transform.Translation;
            transform.Translation = LocalTransform.Translation + Velocity * dt;


            if (!OrientWithVelocity)
            {
                Matrix dA = Matrix.Identity;
                dA *= Matrix.CreateRotationX(AngularVelocity.X * dt);
                dA *= Matrix.CreateRotationY(AngularVelocity.Y * dt);
                dA *= Matrix.CreateRotationZ(AngularVelocity.Z * dt);

                transform = dA * transform;
            }
            else
            {
                if (Velocity.Length() > 0.4f)
                {
                    Matrix newTransform = Matrix.CreateRotationY((float)Math.Atan2(Velocity.X, -Velocity.Z));
                    newTransform.Translation = transform.Translation;
                    transform = newTransform;
                }
            }

            LocalTransform = transform;

            if (Math.Abs(Velocity.Y) < 0.1f)
            {
                Velocity = new Vector3(Velocity.X * Friction, Velocity.Y, Velocity.Z * Friction);
            }

            Velocity *= LinearDamping;
            AngularVelocity *= AngularDamping;
            UpdateBoundingBox();
            
            base.Update(gameTime, camera);
        }

        public override void ReceiveMessageRecursive(Message messageToReceive)
        {
            switch (messageToReceive.Type)
            {
                case Message.MessageType.OnChunkModified:
                    m_overrideSleepThisFrame = true;
                    break;
            }


            base.ReceiveMessageRecursive(messageToReceive);
        }


        public virtual void HandleCollisions(List<BoundingBox> boxes, float dt)
        {
            if (Velocity.LengthSquared() < 0.1f)
            {
                return;
            }


            foreach(BoundingBox box in boxes)
            {
                BoundingBox voxAABB = box;
                if (BoundingBox.Intersects(voxAABB))
                {
                    Contact contact = new Contact();

                    if (TestStaticAABBAABB(BoundingBox, voxAABB, ref contact))
                    {

                        Matrix m = LocalTransform;
                        m.Translation += contact.nEnter * contact.penetration;

                        if (contact.nEnter.Y > 0.9)
                        {
                            m_applyGravityThisFrame = false;
                        }

                        Vector3 newVelocity = (contact.nEnter * Vector3.Dot(Velocity, contact.nEnter));
                        Velocity = (Velocity - newVelocity) * Restitution;


                        LocalTransform = m;
                        UpdateBoundingBox();
                    }
                }
            }

                    
                    

                
            

        }


        class Contact
        {
            public bool isIntersecting = false;
            public Vector3 nEnter = Vector3.Zero;
            public float penetration;
        }

        private static bool TestStaticAABBAABB(BoundingBox s1, BoundingBox s2, ref Contact contact)
        {
            BoundingBox a = s1;
            BoundingBox b = s2;

            // [Minimum Translation Vector]
            float mtvDistance = float.MaxValue;             // Set current minimum distance (max float value so next value is always less)
            Vector3 mtvAxis = new Vector3();                // Axis along which to travel with the minimum distance

            // [Axes of potential separation]
            // • Each shape must be projected on these axes to test for intersection:
            //          
            // (1, 0, 0)                    A0 (= B0) [X Axis]
            // (0, 1, 0)                    A1 (= B1) [Y Axis]
            // (0, 0, 1)                    A1 (= B2) [Z Axis]

            // [X Axis]
            if (!TestAxisStatic(Vector3.UnitX, a.Min.X, a.Max.X, b.Min.X, b.Max.X, ref mtvAxis, ref mtvDistance))
            {
                return false;
            }

            // [Y Axis]
            if (!TestAxisStatic(Vector3.UnitY, a.Min.Y, a.Max.Y, b.Min.Y, b.Max.Y, ref mtvAxis, ref mtvDistance))
            {
                return false;
            }

            // [Z Axis]
            if (!TestAxisStatic(Vector3.UnitZ, a.Min.Z, a.Max.Z, b.Min.Z, b.Max.Z, ref mtvAxis, ref mtvDistance))
            {
                return false;
            }

            contact.isIntersecting = true;

            // Calculate Minimum Translation Vector (MTV) [normal * penetration]
            contact.nEnter = Vector3.Normalize(mtvAxis);

            // Multiply the penetration depth by itself plus a small increment
            // When the penetration is resolved using the MTV, it will no longer intersect
            contact.penetration = (float)Math.Sqrt(mtvDistance) * 1.001f;

            return true;
        }

        private static bool TestAxisStatic(Vector3 axis, float minA, float maxA, float minB, float maxB, ref Vector3 mtvAxis, ref float mtvDistance)
        {
            // [Separating Axis Theorem]
            // • Two convex shapes only overlap if they overlap on all axes of separation
            // • In order to create accurate responses we need to find the collision vector (Minimum Translation Vector)   
            // • Find if the two boxes intersect along a single axis 
            // • Compute the intersection interval for that axis
            // • Keep the smallest intersection/penetration value
            float axisLengthSquared = Vector3.Dot(axis, axis);

            // If the axis is degenerate then ignore
            if (axisLengthSquared < 1.0e-8f)
            {
                return true;
            }

            // Calculate the two possible overlap ranges
            // Either we overlap on the left or the right sides
            float d0 = (maxB - minA);   // 'Left' side
            float d1 = (maxA - minB);   // 'Right' side

            // Intervals do not overlap, so no intersection
            if (d0 <= 0.0f || d1 <= 0.0f)
            {
                return false;
            }

            // Find out if we overlap on the 'right' or 'left' of the object.
            float overlap = (d0 < d1) ? d0 : -d1;

            // The mtd vector for that axis
            Vector3 sep = axis * (overlap / axisLengthSquared);

            // The mtd vector length squared
            float sepLengthSquared = Vector3.Dot(sep, sep);

            // If that vector is smaller than our computed Minimum Translation Distance use that vector as our current MTV distance
            if (sepLengthSquared < mtvDistance)
            {
                mtvDistance = sepLengthSquared;
                mtvAxis = sep;
            }

            return true;
        }

        public void ApplyForce(Vector3 force, float dt)
        {
            Velocity += (force / Mass) * dt;
        }

        public void ApplyTorque(Vector3 torque, float dt)
        {
            AngularVelocity += (torque / I) / dt;
        }

    }
}
