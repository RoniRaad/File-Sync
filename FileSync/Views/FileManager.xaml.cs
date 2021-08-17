using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileSync
{

    public partial class FileManager : Window
    {
        private readonly IFileManagerViewModel _fileManagerViewModel;
        public FileManager(IFileManagerViewModel fileManagerViewModel)
        {
            _fileManagerViewModel = fileManagerViewModel;
            InitializeComponent();
            DataContext = _fileManagerViewModel;
        }

        private void AddDirectory(object sender, RoutedEventArgs e)
        {
            (new AddDirectoryPrompt(_fileManagerViewModel)).Show();
        }

        private void DeleteDirectory(object sender, RoutedEventArgs e)
        {
            _fileManagerViewModel.DeleteDirectory(_fileManagerViewModel.SelectedSyncDirectory);
        }
    }
}
