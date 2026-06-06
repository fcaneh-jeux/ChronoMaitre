using System.Diagnostics;
using HurryUpDavid.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Audio;
using Microsoft.Maui.Storage;

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
    private IAudioManager _audioManager;
    private IAudioPlayer _ambientPlayer;
    private IAudioPlayer _breathingPlayer;

    public GamePage(GameSettings gameSettings, List<Color> playerColors)
    {
        InitializeComponent();
        
        _gameSettings = gameSettings;
        _playerColors = playerColors;

        _isRunning = false;
        _isPaused = false;
        _remainingSeconds = _gameSettings.TurnDuration;

        TimerLabel.Text = _gameSettings.TurnDuration.ToString();
        PlayerLabel.Text = $"Tour de: Joueur {_currentPlayerIndex + 1}";

        _circleGameDrawable = new CircleGameDrawable
        {
            CurrentColor = _playerColors[0]
        };
        GameCanvas.Drawable = _circleGameDrawable;

        // Initialisation de l'audio
        _audioManager = AudioManager.Current;
        LoadSoundTheme();
    }

    private async void LoadSoundTheme()
    {
        try
        {
            _ambientPlayer?.Dispose();
            _breathingPlayer?.Dispose();

            switch (_gameSettings.SoundTheme)
            {
                case "Clochettes":
                    var bellsStream = await FileSystem.OpenAppPackageFileAsync("Audios/bells.wav");
                    if (bellsStream != null)
                    {
                        _ambientPlayer = _audioManager.CreatePlayer(bellsStream);
                        _ambientPlayer.Loop = false;
                    }
                    break;

                case "Respiration":
                    var breathingStream = await FileSystem.OpenAppPackageFileAsync("Audios/breathing.wav");
                    if (breathingStream != null)
                    {
                        _ambientPlayer = _audioManager.CreatePlayer(breathingStream);
                        _ambientPlayer.Loop = false;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Erreur audio: {ex.Message}");
        }
    }

    private void OnCircleTapped(object sender, EventArgs e)
    {
        if (!_isRunning) return;
        if (_isPaused) return;
        OnNextPlayer();
    }

    private void OnPauseResumeClicked(object sender, EventArgs e)
    {
        if (!_isRunning)
        {
            _isRunning = true;
            PauseResumeButton.Text = "❚❚";
            _cancellationTokenSource = new CancellationTokenSource();

            if (_remainingSeconds <= 10 && _ambientPlayer != null && _gameSettings.SoundTheme != "Aucune")
            {
                _ambientPlayer.Play();
            }

            _ = RunTimer(_cancellationTokenSource.Token);
        }
        else
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                PauseResumeButton.Text = "▶";
                _cancellationTokenSource?.Cancel();
                if (_ambientPlayer != null)
                {
                    _ambientPlayer.Pause();
                }
            }
            else
            {
                PauseResumeButton.Text = "❚❚";
                _cancellationTokenSource = new CancellationTokenSource();
                
                if (_remainingSeconds <= 10 && _ambientPlayer != null && _gameSettings.SoundTheme != "Aucune")
                {
                    _ambientPlayer.Play();
                }
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
        _cancellationTokenSource?.Cancel();

        _isPaused = false;

        _currentPlayerIndex = (_currentPlayerIndex + 1) % _gameSettings.PlayerCount;
        _remainingSeconds = _gameSettings.TurnDuration;
        _circleGameDrawable.GlowIntensities.Clear();
        RenderRefresh();

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();

        if (_ambientPlayer != null)
        {
            _ambientPlayer.Stop();
            _ambientPlayer.Seek(0);
        }
        _ = RunTimer(_cancellationTokenSource.Token);
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
        bool isSoundPlayed = false;

        if (_ambientPlayer != null && _ambientPlayer.IsPlaying && _remainingSeconds <= 10)
        {
            isSoundPlayed = true; // Marque le son comme joué si le timer est déjà dans les 10 secondes et que le son est en train de jouer
        }

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

            // Pulsation et sons dans les 10 dernières secondes
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

                if (_remainingSeconds == 10 && !isSoundPlayed && _ambientPlayer != null && _gameSettings.SoundTheme != "Aucune")
                {
                    _ambientPlayer.Stop(); // Arrête le son au cas où il serait déjà en train de jouer
                    _ambientPlayer.Seek(0); // Rembobine au début
                    _ambientPlayer.Play(); // Lance le son
                    isSoundPlayed = true; // Marque le son comme joué
                    System.Diagnostics.Debug.WriteLine($"🔊 Son joué à 10 secondes pour le joueur {_currentPlayerIndex + 1} !");
                }
            }
            else
            {
                isPulsing = false;
                pulseProgress = 0f;
                // Ne pas réinitialiser isSoundPlayed ici (c'est fait dans OnNextPlayer)
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