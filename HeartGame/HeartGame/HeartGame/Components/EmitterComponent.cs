using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    class EmitterComponent : TintableComponent
    {
        public string Emitter { get; set; }
        public bool TriggerOnDeath { get; set; }
        public int TriggerAmount { get; set; }
        public bool TriggerInBox { get; set; }
        public int BoxTriggerTimes { get; set; }
        public ParticleManager ParticleManager { get; set; }
        public EmitterComponent(ParticleManager particles, string emitter, ComponentManager manager, string name, GameComponent parent, Matrix localTransform, Vector3 boundingBoxExtents, Vector3 boundingBoxPos) :
            base(manager, name, parent, localTransform, boundingBoxExtents, boundingBoxPos, false) 
        {
            ParticleManager = particles;
            Emitter = emitter;
            TriggerOnDeath = true;
            TriggerAmount = 10;
            BoxTriggerTimes = 10;
            TriggerInBox = true;

        }

        public void Trigger()
        {
            Vector3 p = GlobalTransform.Translation;
            if (TriggerInBox)
            {
                Vector3 ext = GetBoundingBox().Max - GetBoundingBox().Min;
                for (int i = 0; i < BoxTriggerTimes; i++)
                {

                    Vector3 triggerPos = GetBoundingBox().Min + new Vector3((float)RandomHelper.random.NextDouble() * ext.X,
                                                                            (float)RandomHelper.random.NextDouble() * ext.Y,
                                                                            (float)RandomHelper.random.NextDouble() * ext.Z)
                                                                            ;
                    ParticleManager.Emitters[Emitter].Trigger(TriggerAmount, triggerPos, Tint);
                }
            }
            else
            {
                ParticleManager.Emitters[Emitter].Trigger(TriggerAmount, p, Tint);
            }
        }

        public override void Die()
        {
            if (TriggerOnDeath)
            {
                SoundManager.PlaySound("explode", GlobalTransform.Translation);
                Trigger();
            }
            base.Die();
        }

    }
}
