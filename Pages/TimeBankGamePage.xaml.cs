using System.Collections.Generic;
using HurryUpDavid.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace HurryUpDavid.Pages;

public partial class TimeBankGamePage : ContentPage
{
    private List<PlayerInfo> _players = new List<PlayerInfo>();
    private GameSettings _gameSettings;
    private List<Color> _playerColors;
    private TimeSpan _gameTime;
    private bool _isRunning = false;
    private bool _isPaused = false;
    private bool _isTransitionAnimating = false;
    private int _currentPlayerIndex = 0;
    private IDispatcherTimer? _timer;

    public TimeBankGamePage(GameSettings gameSettings, List<Color> playerColors)
    {
        InitializeComponent();
        _gameSettings = gameSettings;
        _playerColors = playerColors;
        _gameTime = TimeSpan.FromMinutes(_gameSettings.TimeBankMinutes);
        _isRunning = false;
        _isPaused = false;


        System.Diagnostics.Debug.WriteLine($"TimeBankMinutes = {_gameSettings.TimeBankMinutes}");
        // Initialise les joueurs avec leur couleur et leur temps
        for (int i = 0; i < _gameSettings.PlayerCount; i++)
        {
            _players.Add(new PlayerInfo
            {
                Color = _playerColors[i],
                RemainingTime = TimeSpan.FromMinutes(_gameSettings.TimeBankMinutes)
            });
        }

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;

        // Affiche les joueurs
        RenderPlayers();
        UpdateCurrentPlayerDisplay();
    }

    private void OnCurrentPlayerTapped(object sender, TappedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("TAP DETECTE");
        NextPlayer();
    }

    private void NextPlayer()
    {
        if(_players.All(player=> player.RemainingTime <= TimeSpan.Zero))
        {
            _isRunning = false;
            _timer?.Stop();
            return;
        }

        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
        while (_players[_currentPlayerIndex].RemainingTime <= TimeSpan.Zero);
        
        RenderPlayers();
        UpdateCurrentPlayerDisplay();
    }

    private void UpdateCurrentPlayerDisplay()
    {
        var player = _players[_currentPlayerIndex];
        CurrentPlayerBorder.BackgroundColor = player.Color.WithAlpha(0.20f);
        CurrentPlayerLabel.Text = $"JOUEUR {_currentPlayerIndex + 1}";
        CurrentPlayerLabel.TextColor = player.Color;
        CurrentPlayerTimeLabel.Text = FormatTimeSpan(player);
        CurrentPlayerTimeLabel.TextColor = player.Color;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_isRunning || _isPaused)
            return;

        PlayerInfo currentPlayer = _players[_currentPlayerIndex];

        if (currentPlayer.RemainingTime > TimeSpan.Zero)
        {
            currentPlayer.RemainingTime = currentPlayer.RemainingTime.Subtract(TimeSpan.FromSeconds(1));

            RenderPlayers();
            UpdateCurrentPlayerDisplay();
        }

        if (currentPlayer.RemainingTime < TimeSpan.Zero)
        {
            NextPlayer();
        }
    }

    private void RenderPlayers()
    {
        PlayersStack.Children.Clear();

        int previousPlayer =
            (_currentPlayerIndex - 1 + _players.Count)
            % _players.Count;

        int visualPosition = 0;
        List<int> displayOrder = new();

        displayOrder.Add(previousPlayer);

        for (int i = 0; i < _players.Count; i++)
        {
            int playerIndex =
                (_currentPlayerIndex + i)
                % _players.Count;

            displayOrder.Add(playerIndex);
        }

        foreach (int index in displayOrder.Distinct())
        {
            if (index == _currentPlayerIndex)
            {
                continue;
            }

            visualPosition++;

            PlayerInfo player = _players[index];

            bool isCurrentPlayer = index == _currentPlayerIndex;

            Border playerBorder = new()
            {
                BackgroundColor = player.Color.WithAlpha(0.45f),
                StrokeThickness = 0,
                Padding = 10,
                WidthRequest = 280,
                HeightRequest = 80,

                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 12
                }
            };

            Label playerLabel = new()
            {
                Text = $"Joueur {index + 1}",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            };

            Label timeLabel = new()
            {
                Text = FormatTimeSpan(player),
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            };

            playerBorder.Content = new VerticalStackLayout
            {
                Spacing = 2,
                Children =
                {
                    playerLabel,
                    timeLabel
                }
            };

            PlayersStack.Children.Add(playerBorder);
            int capturedIndex = index;
            //var tapGesture = new TapGestureRecognizer();
            //tapGesture.Tapped += (sender, e) =>
            //{
            //    OnPlayerTapped(capturedIndex);
            //};

            //playerBorder.GestureRecognizers.Add(tapGesture);
        }
    }

    private string FormatTimeSpan(PlayerInfo player)
    {
        int hours = (int)player.RemainingTime.TotalHours;
        int minutes = player.RemainingTime.Minutes;
        int seconds = player.RemainingTime.Seconds;

        if (player.RemainingTime.TotalMinutes > 5)
        {
            return $"{hours:D2}:{minutes:D2}";
        }
        else
        {
            return $"{minutes:D2}:{seconds:D2}";
        }
    }

    //private void OnPlayerTapped(int playerIndex)
    //{

    //    _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
    //    RenderPlayers();
    //    UpdateCurrentPlayerDisplay();
    //}

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        //_cancellationTokenSource?.Cancel();
        Application.Current.Windows[0].Page = new NavigationPage(new TimeBankSetupPage());
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.Quit();
        }
    }

    private void OnPauseResumeClicked(object sender, EventArgs e)
    {
        if (!_isRunning)
        {
            _isRunning = true;
            PauseResumeButton.Text = "❚❚";
            _timer?.Start();
        }
        else
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                PauseResumeButton.Text = "▶";
                //_cancellationTokenSource?.Cancel();
            }
            else
            {
                PauseResumeButton.Text = "❚❚";
                //_cancellationTokenSource = new CancellationTokenSource();

                //_ = RunTimer(_cancellationTokenSource.Token);
            }
        }
    }
}