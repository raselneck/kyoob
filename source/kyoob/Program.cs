using System;
using System.IO;
using System.Reflection;
using Kyoob.Game;

using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;

namespace Kyoob
{
#if WINDOWS // technically only supports Windows, idk
    static class Program
    {
        /// <summary>
        /// The entry point of Kyoob.
        /// </summary>
        static void Main( string[] args )
        {
            using ( KyoobEngine game = new KyoobEngine() )
            {
                game.Run();
            }
        }
    }
#endif
}