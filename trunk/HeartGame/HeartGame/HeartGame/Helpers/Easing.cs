using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeartGame
{
    public class Easing
    {
       public static float CubeInOut(float time, float start, float dest, float duration) 
       {
                time /= duration;
	            time--;
	            return dest*(time*time*time + 1) + start;
       }
    }
}
