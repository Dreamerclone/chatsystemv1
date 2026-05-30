using System.Windows;
using ChatSystem.ViewModels;

namespace ChatSystem.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
