using System.Diagnostics;
using HurryUpDavid.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace HurryUpDavid.Pages;

public partial class GamePage : ContentPage
{
    private int _currentPlayerIndex = 0;
    private int _remainingSeconds;
    private bool _isRunning = false;
    private bool _isPaused = false;
    private List<Color> _playerColors;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly GameSettings _gameSettings;
    private CircleGameDrawable _circleGameDrawable;

    public GamePage(GameSettings gameSettings, List<Color> playerColors)
    {
        InitializeComponent();
        _gameSettings = gameSettings;
        _playerColors = playerColors;

        _isRunning = false; // Le timer ne démarre pas tout seul
        _isPaused = false;
        _remainingSeconds = _gameSettings.TurnDuration;

        TimerLabel.Text = _gameSettings.TurnDuration.ToString();
        PlayerLabel.Text = $"Tour de: Joueur {_currentPlayerIndex + 1}";

        _circleGameDrawable = new CircleGameDrawable
        {
            CurrentColor = _playerColors[0]
        };
        GameCanvas.Drawable = _circleGameDrawable;
    }

    // Méthode appelée quand on clique sur le cercle
    private void OnCircleTapped(object sender, EventArgs e)
    {
        if (!_isRunning) return; // Ne rien faire si le timer n'est pas démarré

        OnNextPlayer();
    }

    // Méthode appelée quand on clique sur Pause/Reprise
    private void OnPauseResumeClicked(object sender, EventArgs e)
    {
        if (!_isRunning)
        {
            // Démarrer le timer
            _isRunning = true;
            PauseResumeButton.Text = "❚❚";  // Icône Pause (deux barres)
            _cancellationTokenSource = new CancellationTokenSource();
            _ = RunTimer(_cancellationTokenSource.Token);
        }
        else
        {
            // Mettre en pause ou reprendre
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                PauseResumeButton.Text = "▶";  // Icône Play
                _cancellationTokenSource?.Cancel();
            }
            else
            {
                PauseResumeButton.Text = "❚❚";  // Icône Pause
                _cancellationTokenSource = new CancellationTokenSource();
                _ = RunTimer(_cancellationTokenSource.Token);
            }
        }
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.Quit();
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        _isRunning = true;
        _remainingSeconds = _gameSettings.TurnDuration;

        _cancellationTokenSource = new CancellationTokenSource();
        await RunTimer(_cancellationTokenSource.Token);
    }

    private void OnNextPlayer()
    {
        // Arrêter le timer actuel
        _cancellationTokenSource?.Cancel();

        // Passer au joueur suivant
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _gameSettings.PlayerCount;
        _remainingSeconds = _gameSettings.TurnDuration;

        _circleGameDrawable.GlowIntensities.Clear();
        RenderRefresh();

        // Relancer le timer pour le nouveau joueur
        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _ = RunTimer(_cancellationTokenSource.Token); // Fire-and-forget
    }

    private async Task RunTimer(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        long lastSecond = 0;
        List<float> glowIntensities = new List<float>();
        const int maxGlowCircles = 5;
        const float glowFadeInSpeed = 0.1f;

        bool isPulsing = false;
        float pulseDirection = 0.8f;
        float pulseProgress = 0f;

        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            TimerLabel.Text = _remainingSeconds.ToString();
            PlayerLabel.Text = $"Tour de: Joueur {_currentPlayerIndex + 1}";

            // Gestion des cercles de glow
            if (_remainingSeconds <= 20)
            {
                int requiredGlowCircles = Math.Min(maxGlowCircles, (int)((20 - _remainingSeconds) / 2f) + 1);
                while (glowIntensities.Count < requiredGlowCircles)
                {
                    glowIntensities.Add(0f);
                }
                for (int i = 0; i < glowIntensities.Count; i++)
                {
                    glowIntensities[i] = Math.Min(1f, glowIntensities[i] + glowFadeInSpeed);
                }
            }
            else
            {
                glowIntensities.Clear();
            }

            // Pulsation dans les 10 dernières secondes
            if (_remainingSeconds <= 10)
            {
                isPulsing = true;
                pulseProgress += pulseDirection * 0.05f;
                if (pulseProgress >= 1f)
                {
                    pulseProgress = 1f;
                    pulseDirection = -2f;
                }
                else if (pulseProgress <= 0f)
                {
                    pulseProgress = 0f;
                    pulseDirection = 0.8f;
                }
            }
            else
            {
                isPulsing = false;
                pulseProgress = 0f;
            }

            _circleGameDrawable.GlowIntensities = glowIntensities;
            _circleGameDrawable.PulseScale = isPulsing ? (1f + 0.05f * pulseProgress) : 1f;
            _circleGameDrawable.IsPulsing = isPulsing;
            _circleGameDrawable.PulseProgress = pulseProgress;
            GameCanvas.Invalidate();

            long elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000;
            if (elapsedSeconds > lastSecond)
            {
                lastSecond = elapsedSeconds;
                _remainingSeconds--;

                if (_remainingSeconds <= 0)
                {
                    _remainingSeconds = 0;
                    TimerLabel.Text = "0";
                    OnNextPlayer();
                    break;
                }
            }

            await Task.Delay(50);
        }
    }

    private void RenderRefresh()
    {
        _circleGameDrawable.CurrentColor = _playerColors[_currentPlayerIndex];
        GameCanvas.Invalidate();
    }
}