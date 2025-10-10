using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;

using Bus.Models;
using Bus.ViewModels;

namespace Bus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            WorldInfo? worldInfo = WorldSelector.Select();
            if (worldInfo is null) Environment.Exit(0);

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                if (name.Name is null) return null;

                string path = Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location)!, name.Name + ".dll");
                if (!File.Exists(path)) return null;

                AssemblyName foundName;
                try
                {
                    foundName = AssemblyName.GetAssemblyName(path);
                }
                catch
                {
                    return null;
                }

                if (foundName.FullName == name.FullName)
                {
                    Assembly assembly = context.LoadFromAssemblyPath(path);
                    return assembly;
                }
                else
                {
                    return null;
                }
            };

            MainWindow mainWindow = new MainWindow(worldInfo!);
            mainWindow.Show();
        }
    }
}
