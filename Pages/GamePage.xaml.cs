using HurryUpDavid.Models;
using Microsoft.Maui.Controls;

namespace HurryUpDavid.Pages;

public partial class GamePage : ContentPage
{
    private int _currentPlayerIndex = 0;
    private int _remainingSeconds;
    private bool _isRunning = false;
    private List<Color> _playerColors;
    private CancellationTokenSource _cancellationTokenSource;

    private readonly GameSettings _gameSettings;

    public GamePage(GameSettings gameSettings, List<Color> playerColors)
    {
        InitializeComponent();
        _gameSettings = gameSettings;
        _playerColors = playerColors;

        BindingContext = _gameSettings;
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        _isRunning = true;
        _remainingSeconds = _gameSettings.TurnDuration;

        _cancellationTokenSource = new CancellationTokenSource();

        RenderPlayerColors();

        await RunTimer(_cancellationTokenSource.Token);
    }

    private async Task RunTimer(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            TimerLabel.Text = _remainingSeconds.ToString();
            PlayerLabel.Text = $"Player {_currentPlayerIndex + 1}'s turn";

            await Task.Delay(1000);

            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
             OnNextClicked(null, null);
            }
        }
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _gameSettings.PlayerCount;
        _remainingSeconds = _gameSettings.TurnDuration;

        RenderPlayerColors();
    }

    private void RenderPlayerColors()
    {
        PlayersLayout.Children.Clear();
        for (int i = 0; i < _playerColors.Count; i++)
        {
            var color = _playerColors[i];

            var box = new BoxView
            {
                Color = color,
                WidthRequest = 40,
                HeightRequest = 40,
                CornerRadius = 20,
                Margin = new Thickness(5),
                Opacity = (i == _currentPlayerIndex) ? 1 : 0.4,
                HorizontalOptions = LayoutOptions.Center
            };

            PlayersLayout.Children.Add(box);
        }
    }
}