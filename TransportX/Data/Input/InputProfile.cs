using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TransportX.Diagnostics;

namespace TransportX.Data.Input
{
    public class InputProfile
    {
        public List<Button> Buttons { get; set; } = [];
        public List<Axis> Axes { get; set; } = [];


        public static InputProfile Import(string path, IErrorCollector errorCollector)
        {
            if (!File.Exists(path))
            {
                XmlSerializer<InputProfile>.ToXml(new InputProfile(), path);
            }

            InputProfile data = XmlSerializer<InputProfile>.FromXml(path, errorCollector) ?? new InputProfile();
            return data;
        }
    }
}
