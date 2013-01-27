using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeartGame
{
    public class Notification
    {
        public float displayLength;
        public string text;
        public Notification(string text, float timer = 1.0f)
        {
            this.text = text;
            displayLength = timer;
        }
    }
}
