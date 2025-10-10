using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Bus.ViewModels;
using Bus.Common.Worlds;

namespace Bus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IWorldInfo worldInfo)
        {
            DataContext = new MainWindowViewModel(worldInfo);
            InitializeComponent();
        }
    }
}
