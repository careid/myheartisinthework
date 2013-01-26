using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeartGame
{
    class Datastructures
    {
        public static IEnumerable<TKey> RandomKeys<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            Random rand = new Random();
            List<TKey> values = Enumerable.ToList(dict.Keys);

           

            int size = dict.Count;

            if (size > 0)
            {
                while (true)
                {
                    yield return values[rand.Next(size)];
                }
            }
        }

    }
}
