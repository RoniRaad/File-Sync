using FileSync.Application.Interfaces;
using System.Windows;

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
            new AddDirectoryPrompt(_fileManagerViewModel, _addDirectoryViewModel).Show();
        }

        private void DeleteDirectory(object sender, RoutedEventArgs e) => _fileManagerViewModel.DeleteDirectory(_fileManagerViewModel.SelectedSyncDirectory);

    }
}
