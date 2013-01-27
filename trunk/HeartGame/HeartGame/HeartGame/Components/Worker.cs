using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HeartGame
{
    public class Worker : NPC
    {
        public Worker(Vector3 position,
                          ComponentManager componentManager,
                          ContentManager content,
                          GraphicsDevice graphics)
            : base("Worker", position, componentManager, content, graphics, "oldwalk")
        {
        }
    }
}

