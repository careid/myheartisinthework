using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace HeartGame
{
    public class Sound3D
    {
        public SoundEffectInstance EffectInstance;
        public Vector3 Position;
        public bool HasStarted;
        public string Name;
    }

    public class SoundManager
    {
        public static List<Sound3D> ActiveSounds = new List<Sound3D>();
        public static AudioListener Listener = new AudioListener();
        public static AudioEmitter Emitter = new AudioEmitter();
        public static ContentManager Content { get; set; }

        public static void StopSounds(string name)
        {
            foreach (Sound3D activeSound in ActiveSounds)
            {
                if (activeSound.Name == name)
                {
                    activeSound.EffectInstance.Stop();
                }
            }
        }

        public static void PlaySound(string name, Vector3 location)
        {
            if (Content == null)
            {
                return;
            }
            SoundEffect effect = Content.Load<SoundEffect>(name);
   
            Sound3D sound = new Sound3D();
            sound.Position = location;
            sound.EffectInstance = effect.CreateInstance();
            sound.EffectInstance.IsLooped = false;
            sound.HasStarted = false;
            sound.Name = name;

            ActiveSounds.Add(sound);
        }

        public static void Update(GameTime time, Camera camera)
        {
            List<Sound3D> toRemove = new List<Sound3D>();

            Matrix viewInverse = Matrix.Invert(camera.ViewMatrix);
            Listener.Position = camera.Position;
            Listener.Up = viewInverse.Up;
            Listener.Velocity = camera.Velocity;
            Listener.Forward = viewInverse.Forward;
          


            foreach (Sound3D instance in ActiveSounds)
            {
                if (instance.HasStarted && instance.EffectInstance.State == SoundState.Stopped)
                {
                    instance.EffectInstance.Dispose();
                    toRemove.Add(instance);
                }
                else if (!instance.HasStarted)
                {
                    Emitter.Position = instance.Position;
                    instance.EffectInstance.Apply3D(Listener, Emitter);
                    instance.EffectInstance.Play();
                    instance.HasStarted = true;
                }
            }

            foreach (Sound3D r in toRemove)
            {
                ActiveSounds.Remove(r);
            }
        }
    }
}
