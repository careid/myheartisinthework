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
    public class Hospital
    {
        public LocatableComponent Component { get; set; }
        public float Score { get; set; }
        public Color Color { get; set; }

        public Hospital(Vector3 position, Vector3 size, ComponentManager componentManager,
                                                  ContentManager content,
                                                  GraphicsDevice graphics,
                                                  string name, Color color)
        {
            Color = color;
            Score = 0.0f;
            Component = EntityFactory.GenerateBuilding(position, size, componentManager, content, graphics, name);
        }
    }
}
