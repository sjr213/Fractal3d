using System.Reflection;
using System;
using System.Windows;

namespace Fractal3d
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            Title = "Fractal 3D - Version " + version;

            var viewModel = (MainVm)DataContext;
            Closing += viewModel.OnWindowClosing!;
        }
    }
}
