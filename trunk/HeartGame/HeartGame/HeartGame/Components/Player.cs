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
    public class Player : Person
    {
        public Player(string name, Vector3 position,
                      ComponentManager componentManager,
                      ContentManager content,
                      GraphicsDevice graphics,
                      string spritesheet) :
            base(name, position, componentManager, content, graphics, spritesheet)
        {
        }
    }
}
