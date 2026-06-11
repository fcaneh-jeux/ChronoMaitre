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
    private bool _isPulseRunning = false;
    private int _currentPlayerIndex = 0;
    private IDispatcherTimer? _timer;
    private bool _criticalPulseRunning;

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

    private void StopCriticalPulse()
    {
        _criticalPulseRunning = false;
    }

    private async Task RunCriticalPulse()
    {
        if (_criticalPulseRunning)
            return;

        _criticalPulseRunning = true;

        while (_criticalPulseRunning)
        {
            await CurrentPlayerTimeLabel.ScaleToAsync(
                1.15,
                500,
                Easing.CubicOut);

            await CurrentPlayerTimeLabel.ScaleToAsync(
                1.00,
                500,
                Easing.CubicIn);
        }
    }

    private void OnCurrentPlayerTapped(object sender, TappedEventArgs e)
    {
        if (!_isRunning) return;
        if (_isPaused) return;
        NextPlayer();
    }

    private void NextPlayer()
    {
        StopCriticalPulse();

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

        double remainingSeconds = player.RemainingTime.TotalSeconds;

        CurrentPlayerBorder.BackgroundColor = player.Color.WithAlpha(0.35f);
        CurrentPlayerLabel.Text = $"JOUEUR {_currentPlayerIndex + 1}";
        CurrentPlayerLabel.TextColor = player.Color;
        CurrentPlayerTimeLabel.Text = FormatTimeSpan(player);
        CurrentPlayerTimeLabel.TextColor = player.Color;

        if (remainingSeconds <= 20)
        {
            CurrentPlayerBorder.BackgroundColor = player.Color.WithAlpha(0.50f);
        }

        if (remainingSeconds <= 10)
        {
            CurrentPlayerBorder.BackgroundColor = player.Color.WithAlpha(0.70f);
            _ = RunCriticalPulse();
        }
        else
        {
            StopCriticalPulse();
        }
    }

    private async Task PulseTimer(double remaingseconds)
    {
        if (_isPulseRunning) return;

        _isPulseRunning = true;

        if(remaingseconds > 3)
        {
            await CurrentPlayerTimeLabel.ScaleToAsync(1.15, 500, Easing.CubicOut);
            await CurrentPlayerTimeLabel.ScaleToAsync(1.00, 500, Easing.CubicIn);
        }                                                   
        else                                                
        {                                                   
            await CurrentPlayerTimeLabel.ScaleToAsync(1.22, 500, Easing.CubicOut);
            await CurrentPlayerTimeLabel.ScaleToAsync(1.00, 500, Easing.CubicIn);
        }

        _isPulseRunning = false;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_isRunning || _isPaused)
            return;

        PlayerInfo currentPlayer = _players[_currentPlayerIndex];

        if (currentPlayer.RemainingTime > TimeSpan.Zero)
        {
            currentPlayer.RemainingTime -= TimeSpan.FromSeconds(1);

            if (currentPlayer.RemainingTime <= TimeSpan.Zero)
            {
                currentPlayer.RemainingTime = TimeSpan.Zero;
                NextPlayer();
            }
            
            RenderPlayers();
            UpdateCurrentPlayerDisplay();
        }

    }

    private void RenderPlayers()
    {
        PlayersAboveStack.Children.Clear();
        PlayersBelowStack.Children.Clear();

        List<int> displayOrder = new();

        int beforeCount = (_players.Count - 1) / 2;
        int afterCount = _players.Count - beforeCount - 1;

        for (int offset = -beforeCount; offset <= afterCount; offset++)
        {
            int playerIndex = (_currentPlayerIndex + offset + _players.Count) % _players.Count;
            displayOrder.Add(playerIndex);
        }

        //int middleIndex = displayOrder.Count / 2;
        int currentPlayerPosition = displayOrder.IndexOf(_currentPlayerIndex);

        for (int position = 0; position < displayOrder.Count; position++) 
        {
            int index = displayOrder[position];

            // Le joueur courant est affiché par
            // CurrentPlayerLabel / CurrentPlayerTimeLabel
            if (position == currentPlayerPosition)
            {
                continue;
            }

            PlayerInfo player = _players[index];

            int distanceFromCenter = Math.Abs(position - currentPlayerPosition);

            double opacity = distanceFromCenter switch
            {
                1 => 0.80,
                2 => 0.60,
                3 => 0.40,
                _ => 0.25
            };

            double scale = distanceFromCenter switch
            {
                1 => 1.00,
                2 => 0.92,
                3 => 0.84,
                _ => 0.78
            };

            Border playerBorder = new()
            {
                BackgroundColor = player.Color.WithAlpha((float)opacity),
                StrokeThickness = 0,
                Padding = 10,
                WidthRequest = 280,
                HeightRequest = 80,
                Scale = scale,

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

            if (position < currentPlayerPosition)
            {
                PlayersAboveStack.Children.Add(playerBorder);
            }
            else
            {
                PlayersBelowStack.Children.Add(playerBorder);
            }
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
            // pour la démo
            //return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            return $"{minutes:D2}:{seconds:D2}";
        }
    }

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