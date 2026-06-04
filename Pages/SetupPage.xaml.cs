using HurryUpDavid.Models;
using Microsoft.Maui.Controls;

namespace HurryUpDavid.Pages;

public partial class SetupPage : ContentPage
{
	public SetupPage()
	{
		InitializeComponent();
	}

	private async void OnStartClicked(object sender, EventArgs e)
	{
		var setttings = new GameSettings
		{
			PlayerCount = int.Parse(PlayersEntry.Text),
			TurnDuration= int.Parse(TimeEntry.Text)
		};

		await Navigation.PushAsync(new GamePage(setttings));
    }
}