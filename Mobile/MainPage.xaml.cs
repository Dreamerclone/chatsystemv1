using ChatSystem.ViewModels;

namespace ChatSystem.Mobile;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
