using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Data.Input.GameControllers
{
    public class GameController
    {
        public List<Axis> Axes { get; set; } = [];


        public static GameController Import(string path, IErrorCollector errorCollector)
        {
            if (!File.Exists(path))
            {
                XmlSerializer<GameController>.ToXml(new GameController(), path);
            }

            GameController data = XmlSerializer<GameController>.FromXml(path, errorCollector) ?? new GameController();
            return data;
        }
    }
}
