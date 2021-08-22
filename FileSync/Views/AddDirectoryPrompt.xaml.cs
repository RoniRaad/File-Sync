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
using Ookii.Dialogs.Wpf;
using FileSync.Application.ViewModels;
using FileSync.Application.Interfaces;

namespace FileSync
{
    /// <summary>
    /// Interaction logic for AddDirectoryPrompt.xaml
    /// </summary>
    public partial class AddDirectoryPrompt : Window
    {
        private readonly IFileManagerViewModel _parentViewModel;
        private readonly IAddDirectoryViewModel _viewModel;

        public AddDirectoryPrompt(IFileManagerViewModel parentViewModel, IAddDirectoryViewModel viewModel)
        {
            _parentViewModel = parentViewModel;
            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent(); 
        }

        private void AddDirectory(object sender, RoutedEventArgs e)
        {
            _parentViewModel.AddDirectory(_viewModel.SyncDirectory);
            Close();
        }

        private void OpenFolderPicker(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.Description = "Please select a folder.";
            folderBrowserDialog.UseDescriptionForTitle = true;

            if (folderBrowserDialog.ShowDialog() ?? false)
                _viewModel.SyncDirectory.Directory = folderBrowserDialog.SelectedPath;

            _viewModel.SyncDirectoryUpdated();
        }
    }
}
