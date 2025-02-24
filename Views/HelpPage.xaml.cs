using taskConnectProject.ViewModel;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace taskConnectProject.Views;



public partial class HelpPage : ContentPage
{
	public HelpPage()
	{
		InitializeComponent();
        BindingContext = new HelpViewModel();
    }
}