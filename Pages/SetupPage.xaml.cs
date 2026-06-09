using HurryUpDavid.Models;
using Microsoft.Maui.Controls;

namespace HurryUpDavid.Pages;

public partial class SetupPage : ContentPage
{
    private int _playerCount = 4;
    private int _turnDuration = 90;
    private Button? _selectedTimeButton;
    private readonly Color _normalColor = Color.FromArgb("#008B8B");
    private readonly Color _selectedColor = Color.FromArgb("#00BCD4");

    public SetupPage()
    {
        InitializeComponent();

        _playerCount = 4;
        _turnDuration = 90;
        _selectedTimeButton = Time90Button;
        Time90Button.BackgroundColor = _selectedColor;
        Time90Button.Scale = 1.15;
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.Quit();
        }
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
        int playerCount = _playerCount;

        int turnDuration = _turnDuration;

        string selectedSoundTheme = SoundThemePicker.SelectedItem?.ToString() ?? "Aucune";

        // Créer les paramètres du jeu
        GameSettings gameSettings = new GameSettings
        {
            PlayerCount = playerCount,
            TurnDuration = turnDuration,
            SoundTheme = selectedSoundTheme,
        };

        // Naviguer vers la page de sélection des couleurs
        Navigation.PushAsync(new ColorSelectionPage(gameSettings));
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

    private async void OnTimeSelectedAsync(object sender, EventArgs e)
    {   

        var button = (Button)sender;

        _turnDuration =
            int.Parse(button.Text.Replace("s", ""));

        if (_selectedTimeButton != null)
        {
            await _selectedTimeButton.ScaleToAsync(1, 80);
            _selectedTimeButton.BackgroundColor = _normalColor;            
        }

        button.BackgroundColor = _selectedColor;

        await button.ScaleToAsync(1.15, 80);

        _selectedTimeButton = button;
    }
}