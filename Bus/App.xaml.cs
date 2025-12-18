using System;
using System.Configuration;
using System.Data;
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
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            WorldInfo? worldInfo = WorldSelector.Select();
            if (worldInfo is null) Environment.Exit(0);

            bool isLoaded = ((MainWindowViewModel)mainWindow.DataContext).LoadGame(worldInfo);
            if (!isLoaded) Environment.Exit(1);
        }
    }
}
