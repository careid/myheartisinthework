using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace HeartGame
{
    public class PrimitiveLibrary
    {
        public static Dictionary<string, BoxPrimitive> BoxPrimitives = new Dictionary<string, BoxPrimitive>();
        public static Dictionary<string, BillboardPrimitive> BillboardPrimitives = new Dictionary<string, BillboardPrimitive>();

        private static bool m_initialized = false;

        public PrimitiveLibrary(GraphicsDevice graphics, ContentManager content)
        {
            Initialize(graphics, content);
        }

        public void Initialize(GraphicsDevice graphics, ContentManager content)
        {
            if (!m_initialized)
            {
                Texture2D spriteSheet = content.Load<Texture2D>("tiles");
                BoxPrimitive.BoxTextureCoords boxCoords = new BoxPrimitive.BoxTextureCoords(spriteSheet.Width, spriteSheet.Height, 32, 32,
                    new Point(5, 9),
                    new Point(8, 9), 
                    new Point(7, 8), 
                    new Point(9, 9), 
                    new Point(7, 9),
                    new Point(6, 9));
                BoxPrimitives["bed"] = new BoxPrimitive(graphics, 0.8f, 0.5f, 1.8f, boxCoords);
                m_initialized = false;
            }
        }

    }
}
