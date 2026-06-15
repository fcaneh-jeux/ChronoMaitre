namespace HurryUpDavid.Pages;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}
    private async void OnTurnTimerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SetupPage(), false);
    }

    private async void OnTimeBankClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TimeBankSetupPage(), false);
    }
}