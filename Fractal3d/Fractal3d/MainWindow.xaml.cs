using System.Reflection;
using System;
using System.IO;
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

            var fileName = GetFilename();

            var viewModel = new MainVm(fileName);
            Closing += viewModel.OnWindowClosing!;
            DataContext = viewModel;
        }

        private bool HasCorrectExtension(string filename)
        {
            var ext = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(ext))
                return false;

            return ext == Fractal3dConstants.FileExtension;
        }

        private string GetFilename()
        {
            var args = Environment.GetCommandLineArgs();
            var fileName = args.Length > 1 ? args[1] : "";

            int nextArg = 2;

            // It cuts out spaces and adds the rest of the string on the next line
            while (!HasCorrectExtension(fileName) && args.Length > nextArg)
            {
                fileName += " ";
                fileName += args[nextArg++];
            }

            return fileName;
        }
    }
}
