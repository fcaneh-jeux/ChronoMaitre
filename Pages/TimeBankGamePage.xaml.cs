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
    private readonly Dictionary<int, Border> _playerBorders = new();

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

        for (int i = 0; i < _players.Count; i++)
        {
            _playerBorders[i] = CreatePlayerBorder(i);
        }

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;

        // Affiche les joueurs
        RenderPlayers();
        UpdateCurrentPlayerDisplay();
    }

    private Border CreatePlayerBorder(int playerIndex)
    {
        PlayerInfo player = _players[playerIndex];

        Border playerBorder = new()
        {
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
            Text = $"Joueur {playerIndex + 1}",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center
        };

        Label timeLabel = new()
        {
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

        return playerBorder;
    }

    private async Task RotatePlayersAnimation()
    {

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

        Border currentBorder = _playerBorders[_currentPlayerIndex];
        while (_criticalPulseRunning)
        {
            await currentBorder.ScaleToAsync(1.15, 500, Easing.CubicOut);

            await currentBorder.ScaleToAsync(1.00, 500, Easing.CubicIn);
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
        if (_isTransitionAnimating) return;

        StopCriticalPulse();

        if (_players.All(player => player.RemainingTime <= TimeSpan.Zero))
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
        RenderPlayers();
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
        GameArea.Children.Clear();
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

            bool isCurrentPlayer = position == currentPlayerPosition;
            PlayerInfo player = _players[index];
            Border playerBorder = _playerBorders[index];

            int distanceFromCenter = Math.Abs(position - currentPlayerPosition);

            double opacity;
            if (isCurrentPlayer)
            {
                opacity = 1.0;
            }
            else
            {
                opacity = distanceFromCenter switch
                {
                    1 => 0.80,
                    2 => 0.60,
                    3 => 0.40,
                    _ => 0.25
                };
            }

            double scale;
            if (isCurrentPlayer)
            {
                scale = 1.0;
            }
            else
            {
                scale = distanceFromCenter switch
                {
                    1 => 1.00,
                    2 => 0.92,
                    3 => 0.84,
                    _ => 0.78
                };
            }

            playerBorder.BackgroundColor = player.Color.WithAlpha((float)opacity);
            playerBorder.Scale = scale;

            VerticalStackLayout content = (VerticalStackLayout)playerBorder.Content;
            Label playerLabel = (Label)content.Children[0];
            Label timerLabel = (Label)content.Children[1];
            playerLabel.Text = $"Joueur {index + 1}";
            timerLabel.Text = FormatTimeSpan(player);

            double centerY = 320;
            double spacing = 90;
            double y = centerY + ((position - currentPlayerPosition) * spacing);
            AbsoluteLayout.SetLayoutBounds(playerBorder, new Rect(20, y, 200, 80));
            GameArea.Children.Add(playerBorder);
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