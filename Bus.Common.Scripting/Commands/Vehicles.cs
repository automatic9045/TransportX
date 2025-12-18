using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bus.Common.Diagnostics;

namespace Bus.Common.Scripting.Commands
{
    public class Vehicles
    {
        private readonly ScriptWorld World;

        internal Vehicles(ScriptWorld world)
        {
            World = world;
        }

        public void Load(string path, string? identifier = null)
        {
            string vehiclePath = Path.Combine(World.BaseDirectory, path);
            try
            {
                World.UserVehicle = World.CreateVehicle(vehiclePath, identifier);
            }
            catch (Exception ex)
            {
                ScriptError error = new(ErrorLevel.Error, ex);
                World.ErrorCollector.Report(error);
            }
        }
    }
}
