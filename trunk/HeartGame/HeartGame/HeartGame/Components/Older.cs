using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class Older : NPC
    {
        public Older(Vector3 position,
                          ComponentManager componentManager,
                          ContentManager content,
                          GraphicsDevice graphics)
            : base("Older", position, componentManager, content, graphics, "oldwalk")
        {
        }
    }
}

