using HurryUpDavid.Models;
using Microsoft.Maui.Controls;

namespace HurryUpDavid.Pages;

public partial class SetupPage : ContentPage
{
    public SetupPage()
    {
        InitializeComponent();
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
        // Récupérer le nombre de joueurs (limité à 8)
        if (!int.TryParse(PlayersEntry.Text, out int playerCount) || playerCount < 2)
        {
            playerCount = 2; // Valeur par défaut (minimum 2 joueurs)
            PlayersEntry.Text = "2";
        }
        else if (playerCount > 8)
        {
            playerCount = 8; // Limite à 8 joueurs
            PlayersEntry.Text = "8";
        }

        // Récupérer le temps par tour
        if (!int.TryParse(TimeEntry.Text, out int turnDuration) || turnDuration <= 0)
        {
            turnDuration = 90; // Valeur par défaut
            TimeEntry.Text = "90";
        }

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
}