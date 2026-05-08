using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Vortice.Direct3D11;

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
