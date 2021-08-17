using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for AddDirectoryPrompt.xaml
    /// </summary>
    public partial class AddDirectoryPrompt : Window
    {
        private readonly IFileManagerViewModel _parentViewModel;
        private readonly SyncDirectory _newSyncDirectory;

        public AddDirectoryPrompt(IFileManagerViewModel parentViewModel)
        {
            _newSyncDirectory = new SyncDirectory();
            _parentViewModel = parentViewModel;
            DataContext = _newSyncDirectory;
            InitializeComponent();
        }

        private void AddDirectory(object sender, RoutedEventArgs e)
        {
            _parentViewModel.AddDirectory(_newSyncDirectory);
            Close();
        }
    }
}
