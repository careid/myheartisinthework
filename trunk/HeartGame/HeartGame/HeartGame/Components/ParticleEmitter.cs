using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Scale;
        public float Angle;
        public float AngularVelocity;
        public float LifeRemaining;
        public Color Tint;
    }

    public class EmitterData
    {
        public int MaxParticles;
        public int ParticlesPerFrame;
        public bool ReleaseOnce;
        public float EmissionFrequency;
        public Vector3 ConstantAccel;
        public float MinScale;
        public float MaxScale;
        public float GrowthSpeed;
        public float Damping;
        public float MinAngle;
        public float MaxAngle;
        public float MinAngular;
        public float MaxAngular;
        public float AngularDamping;
        public float LinearDamping;
        public Texture2D Texture;
        public Animation Animation;
        public float EmissionRadius;
        public float EmissionSpeed;
        public float ParticleDecay;
    }

    public class ParticleEmitter : TintableComponent
    {
        public BillboardList Sprites { get; set; }
        public List<Particle> Particles { get; set; }
        public EmitterData Data { get; set; }
        public Timer TriggerTimer { get; set; }
        private static Camera m_camera = null;

        public static Matrix MatrixFromParticle(Particle particle)
        {
            Matrix trans = Matrix.Identity;
            Matrix scl = Matrix.Identity;
            Matrix rot = Matrix.CreateRotationZ(particle.Angle);

            scl = Matrix.CreateScale(particle.Scale);
            trans.Translation = particle.Position;

            return scl * rot * trans;
        }

        public ParticleEmitter(ComponentManager manager, string name, GameComponent parent, Matrix localTransform, EmitterData emitterData) :
            base(manager, name, parent, localTransform, Vector3.Zero, Vector3.Zero, false)
        {
            Particles = new List<Particle>();
            Data = emitterData;
            Sprites = new BillboardList(manager, "Sprites", this, Matrix.Identity, emitterData.Texture, 0);
            Sprites.AddAnimation(Data.Animation);
            Sprites.CurrentAnimation = Data.Animation;
            Data.Animation.Play();
            
            TriggerTimer = new Timer(Data.EmissionFrequency, Data.ReleaseOnce);
        }

        public float rand(float min, float max)
        {
            return (float)(RandomHelper.random.NextDouble() * (max - min) + min);
        }

        public Vector3 randVec(float scale)
        {
            return new Vector3(rand(-scale, scale) * 0.5f, rand(-scale, scale) * 0.5f, rand(-scale, scale) * 0.5f);
        }

        public void Trigger(int num, Vector3 origin, Color tint)
        {
            for (int i = 0; i < num; i++)
            {
                if (Particles.Count < Data.MaxParticles)
                {
                    Particle toAdd = new Particle();

                    bool sampleFound = false;

                    Vector3 sample = new Vector3(99999, 99999, 9999);
                    
                    while (!sampleFound)
                    {
                        sample = randVec(Data.EmissionRadius);

                        if (sample.Length() < Data.EmissionRadius)
                        {
                            sampleFound = true;
                        }
                    }

                    toAdd.Position = sample + origin;
                    toAdd.Velocity = randVec(Data.EmissionSpeed);

                    toAdd.Scale = rand(Data.MinScale, Data.MaxScale);
                    toAdd.Angle = rand(Data.MinAngle, Data.MaxAngle);
                    toAdd.AngularVelocity = rand(Data.MinAngular, Data.MaxAngular);
                    toAdd.LifeRemaining = 1.0f;
                    toAdd.Tint = tint;
                    Particles.Add(toAdd);
                }
            }

        }

        public static int CompareZDepth(Particle A, Particle B)
        {
            if (A == B)
            {
                return 0;
            }

            if ((m_camera.Position - A.Position).LengthSquared() < (m_camera.Position - B.Position).LengthSquared())
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public override void Update(GameTime gameTime, Camera camera)
        {
            m_camera = camera;
            Sprites.Tint = Tint;
            Sprites.FrustrumCull = FrustrumCull;
            Sprites.LocalTransforms.Clear();
            Sprites.Rotations.Clear();
            List<Particle> toRemove = new List<Particle>();

            if (TriggerTimer.HasTriggered)
            {
                Trigger(Data.ParticlesPerFrame, Vector3.Zero, Tint);
            }
            else
            {
                TriggerTimer.Update(gameTime);
            }

            Particles.Sort(CompareZDepth);

            for(int i = 0; i < Particles.Count; i++)
            {
                Particle p = Particles[i];
                p.Position += p.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                p.Angle += (float)(p.AngularVelocity * gameTime.ElapsedGameTime.TotalSeconds);
                p.Velocity += Data.ConstantAccel * (float)gameTime.ElapsedGameTime.TotalSeconds;
                p.Velocity *= Data.LinearDamping;
                p.AngularVelocity *= Data.AngularDamping;
                p.LifeRemaining -= Data.ParticleDecay * (float)gameTime.ElapsedGameTime.TotalSeconds ;
                p.Scale += Data.GrowthSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                p.Scale = Math.Max(p.Scale, 0.0f);
                if (p.LifeRemaining < 0)
                {
                    toRemove.Add(p);
                }

                Sprites.AddTransform(MatrixFromParticle(p), p.Angle, p.Tint);
            }

            foreach (Particle p in toRemove)
            {
                Particles.Remove(p);
            }

            

            base.Update(gameTime, camera);
        }

    }
}
