﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class DeathComponentSpawner : LocatableComponent
    {
        public List<LocatableComponent> Spawns { get; set; }
        public float ThrowSpeed { get; set; }

        public DeathComponentSpawner(ComponentManager manager, string name, GameComponent parent, Matrix localTransform, Vector3 boundingExtents, Vector3 boundingBoxPos, List<LocatableComponent> spawns) :
            base(manager, name, parent, localTransform, boundingExtents, boundingBoxPos)
        {
            Spawns = spawns;
            ThrowSpeed = 1.0f;
        }

        public override void Die()
        {
            foreach (LocatableComponent locatable in Spawns)
            {
                locatable.SetVisibleRecursive(true);
                locatable.SetActiveRecursive(true);

                if (locatable is PhysicsComponent)
                {
                    Vector3 diff = (locatable.GlobalTransform.Translation - GlobalTransform.Translation);
                    diff.Normalize();
                    ((PhysicsComponent)locatable).Velocity += diff * ThrowSpeed;
                }

                Manager.RootComponent.AddChild(locatable);
            }

            base.Die();
        }
    }
}
