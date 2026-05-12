using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Player
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            using Game game = new();
            game.Run();
        }
    }
}
