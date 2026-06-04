using HurryUpDavid.Models;
using Microsoft.Maui.Controls;

namespace HurryUpDavid.Pages;

public partial class GamePage : ContentPage
{
	private readonly GameSettings _gameSettings;

    public int PlayerCount => _gameSettings.PlayerCount;
    public int TurnDuration => _gameSettings.TurnDuration;

    public GamePage(GameSettings gameSettings)
    {
        InitializeComponent();
        _gameSettings = gameSettings;

        BindingContext = this;
    }
}