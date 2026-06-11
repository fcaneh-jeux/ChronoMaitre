using HurryUpDavid.Models;

namespace HurryUpDavid.Pages;

public partial class TimeBankSetupPage : ContentPage
{
    private int _playerCount = 4;
    private int _timeBankMinutes = 60;
    public TimeBankSetupPage()
	{
		InitializeComponent();
        _playerCount = 4;
        _timeBankMinutes = 60;
	}

    private void OnExitClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.Quit();
        }
    }

    private void OnIncreasePlayers(object sender, EventArgs e)
    {
        if (_playerCount < 8)
        {
            _playerCount++;
            PlayersLabel.Text = _playerCount.ToString();
        }
    }

    private void OnDecreasePlayers(object sender, EventArgs e)
    {
        if (_playerCount > 2)
        {
            _playerCount--;
            PlayersLabel.Text = _playerCount.ToString();
        }
    }

    private void OnIncreaseTimeBank(object sender, EventArgs e)
    {
        //if (_timeBankMinutes < 180)
        //{
        //    _timeBankMinutes += 5;
        //    TimeBankLabel.Text = $"{_timeBankMinutes} min";
        //}

        if (_timeBankMinutes < 180)
        {
            if (_timeBankMinutes < 5)
            {
                _timeBankMinutes += 1;
            }
            else
            {
                _timeBankMinutes += 5;
            }

            TimeBankLabel.Text = $"{_timeBankMinutes} min";
        }
    }

    private void OnDecreaseTimeBank(object sender, EventArgs e)
    {
        //if (_timeBankMinutes > 15)
        //{
        //    _timeBankMinutes -= 5;
        //    TimeBankLabel.Text = $"{_timeBankMinutes} min";
        //}

        if (_timeBankMinutes > 1)
        {
            if (_timeBankMinutes <= 5)
            {
                _timeBankMinutes -= 1;
            }
            else
            {
                _timeBankMinutes -= 5;
            }

            TimeBankLabel.Text = $"{_timeBankMinutes} min";
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        // Créer les paramètres du jeu
        GameSettings gameSettings = new GameSettings
        {
            PlayerCount = _playerCount,
            GameMode = GameMode.TimeBank,
            TimeBankMinutes = _timeBankMinutes
        };

        // Naviguer vers la page de sélection des couleurs
        await Navigation.PushAsync(new ColorSelectionPage(gameSettings));
    }
}