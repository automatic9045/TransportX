using System;
using System.Configuration;
using System.Data;
using System.Windows;

using TransportX.Models;
using TransportX.ViewModels;

namespace TransportX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(args.Exception.ToString(), "Critical Exception");
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                MessageBox.Show(args.ExceptionObject.ToString(), "Critical Exception");
            };
#endif

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            WorldInfo? worldInfo = WorldSelector.Select();
            if (worldInfo is null)
            {
                Shutdown();
                return;
            }

            bool isLoaded = ((MainWindowViewModel)mainWindow.DataContext).LoadGame(worldInfo);
            if (!isLoaded)
            {
                Shutdown(1);
                return;
            }
        }
    }
}
