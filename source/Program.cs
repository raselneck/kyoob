using System;

namespace Kyoob
{
// technically only supports Windows
#if WINDOWS
    static class Program
    {
        /// <summary>
        /// The entry point of Kyoob.
        /// </summary>
        static void Main(string[] args)
        {
            using (KyoobEngine game = new KyoobEngine())
            {
                game.Run();
            }
        }
    }
#endif
}