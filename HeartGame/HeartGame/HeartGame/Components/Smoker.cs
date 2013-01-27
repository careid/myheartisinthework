using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class Smoker : NPC
    {
        public Smoker(Vector3 position,
                          ComponentManager componentManager,
                          ContentManager content,
                          GraphicsDevice graphics) 
            : base("Smoker", position, componentManager, content, graphics, "smokewalk")
        {
        }
    }
}
