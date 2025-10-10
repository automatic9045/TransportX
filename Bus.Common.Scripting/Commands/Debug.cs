using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bus.Common.Scripting.Commands
{
    public class Debug
    {
        private readonly ScriptWorld World;

        internal Debug(ScriptWorld world)
        {
            World = world;
        }

        public void ShowDialog(string message)
        {
            MessageBox.Show(message);
        }
    }
}
