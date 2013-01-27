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
        public Player Owner { get; set; }
        public LocatableComponent Component { get; set; }

        public Hospital(Player owner,  Vector3 position, Vector3 size, ComponentManager componentManager,
                                                  ContentManager content,
                                                  GraphicsDevice graphics,
                                                  string name)
        {
            Owner = owner;
            Component = EntityFactory.GenerateBuilding(position, size, componentManager, content, graphics, name);
        }
    }
}
