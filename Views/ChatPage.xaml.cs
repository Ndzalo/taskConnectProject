using taskConnectProject.ViewModel;

namespace taskConnectProject.Views;

public partial class ChatPage : ContentPage
{
	public ChatPage(ChatViewModel chatVM)
	{
		InitializeComponent();
		BindingContext = chatVM;
	}
}