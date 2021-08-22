using FileSync.Application.Interfaces;
using FileSync.Application.ViewModels;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private readonly IAddDirectoryViewModel _addDirectoryViewModel;
        public FileManager(IFileManagerViewModel fileManagerViewModel, IAddDirectoryViewModel addDirectoryViewModel)
        {
            _fileManagerViewModel = fileManagerViewModel;
            _addDirectoryViewModel = addDirectoryViewModel;

            InitializeComponent();
            DataContext = _fileManagerViewModel;
        }

        private void AddDirectory(object sender, RoutedEventArgs e)
        {
            (new AddDirectoryPrompt(_fileManagerViewModel, _addDirectoryViewModel)).Show();
        }

        private void DeleteDirectory(object sender, RoutedEventArgs e) => _fileManagerViewModel.DeleteDirectory(_fileManagerViewModel.SelectedSyncDirectory);

    }
}
