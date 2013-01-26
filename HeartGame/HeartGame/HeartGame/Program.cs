using System;
using System.Threading;

namespace HeartGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }

            SignalShutdown();
        }
        public static ManualResetEvent shutdownEvent = new ManualResetEvent(false);

        static void SignalShutdown()
        {
            shutdownEvent.Set();
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
#endif
}

