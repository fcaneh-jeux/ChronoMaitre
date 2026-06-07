using System.Diagnostics;
using HurryUpDavid.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Audio;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Animations;

namespace HurryUpDavid.Pages;

public partial class GamePage : ContentPage
{
    private int _currentPlayerIndex = 0;
    private int _remainingSeconds;
    private bool _isRunning = false;
    private bool _isPaused = false;
    private bool _isTransitionAnimating = false;
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
        TimerLabel.TextColor = _playerColors[_currentPlayerIndex];
        PlayerLabel.Text = $"JOUEUR {_currentPlayerIndex + 1}";

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
        if (_isTransitionAnimating) return;
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

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        await Navigation.PopToRootAsync();
    }

    private void OnNextPlayer()
    {
        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        _isPaused = false;

        // Sauvegarde l'ancienne couleur
        Color previousColor = _playerColors[_currentPlayerIndex];

        // Passe au joueur suivant
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _gameSettings.PlayerCount;
        _remainingSeconds = _gameSettings.TurnDuration;
        Color nextColor = _playerColors[_currentPlayerIndex];

        // ✅ CurrentColor reste l'ancienne couleur (pour le contour hors transition)
        // ✅ NextColor = nouvelle couleur (pour l'arc pendant la transition)
        System.Diagnostics.Debug.WriteLine($"ANCIENNE={previousColor} NOUVELLE={nextColor}");
        _circleGameDrawable.CurrentColor = previousColor;
        _circleGameDrawable.NextColor = nextColor;
        _circleGameDrawable.IsTransitioning = true;
        _circleGameDrawable.TransitionProgress = 0f;

        _cancellationTokenSource = new CancellationTokenSource();

        if (_ambientPlayer != null)
        {
            _ambientPlayer.Stop();
            _ambientPlayer.Seek(0);
        }
        _ = AnimateColorTransition();
    }

    private async Task AnimateColorTransition()
    {
        System.Diagnostics.Debug.WriteLine(
    $"Current={_circleGameDrawable.CurrentColor}  Next={_circleGameDrawable.NextColor}");
        _isTransitionAnimating = true;
        try
        {
            float duration = 1.0f; // 1 seconde
            int steps = 30;
            float stepDuration = duration / steps;
            float initialScale = _circleGameDrawable.PulseScale;

            for (int i = 0; i <= steps; i++)
            {
                float progress = i / (float)steps;
                _circleGameDrawable.TransitionProgress = progress;

                // Effet de zoom : le cercle grossit puis rétrécit
                _circleGameDrawable.TransitionScale = 1f + (float)Math.Sin(progress * Math.PI) * 0.12f;

                // Met à jour la couleur du texte avec un alpha progressif
                Color currentColor = GetCurrentTextColor();
                float alpha = 0.6f + (0.4f * progress);
                TimerLabel.TextColor = currentColor.WithAlpha(alpha);
                PlayerLabel.TextColor = currentColor.WithAlpha(alpha);

                GameCanvas.Invalidate();
                await Task.Delay((int)(stepDuration * 1000));
            }

            // Transition terminée
            _circleGameDrawable.CurrentColor = _circleGameDrawable.NextColor;
            _circleGameDrawable.NextColor = _circleGameDrawable.CurrentColor;

            _circleGameDrawable.IsTransitioning = false;
            _circleGameDrawable.TransitionProgress = 0f;
            _circleGameDrawable.TransitionScale = 1f;

            _circleGameDrawable.GlowIntensities.Clear();

            // Met à jour l'affichage final
            TimerLabel.Text = _remainingSeconds.ToString();
            TimerLabel.TextColor = _playerColors[_currentPlayerIndex];
            PlayerLabel.Text = $"JOUEUR {_currentPlayerIndex + 1}";
            PlayerLabel.TextColor = Colors.White;


            _isRunning = true;
            _ = RunTimer(_cancellationTokenSource.Token);
        }
        finally
        {
            _isTransitionAnimating = false;
        }
    }

    private Color GetCurrentTextColor()
    {
        if (_circleGameDrawable.IsTransitioning)
        {
            return CircleGameDrawable.Lerp(_playerColors[(_currentPlayerIndex - 1 + _gameSettings.PlayerCount) % _gameSettings.PlayerCount], _playerColors[_currentPlayerIndex], _circleGameDrawable.TransitionProgress);
        }
        else
        {
            return _circleGameDrawable.CurrentColor;
        }
    }

    private async Task RunTimer(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        long lastSecond = 0;
        List<float> glowIntensities = new List<float>();
        const int maxGlowCircles = 5;
        const float glowFadeInSpeed = 0.05f; // Vitesse plus lente pour un effet progressif

        bool isPulsing = false;
        float pulseDirection = 0.8f;
        float pulseProgress = 0f;
        bool isSoundPlayed = false;

        if (_ambientPlayer != null && _ambientPlayer.IsPlaying && _remainingSeconds <= 10)
        {
            isSoundPlayed = true;
        }

        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            TimerLabel.Text = _remainingSeconds.ToString();
            TimerLabel.TextColor = _playerColors[_currentPlayerIndex];
            PlayerLabel.Text = $"JOUEUR {_currentPlayerIndex + 1}";
            PlayerLabel.TextColor = Colors.White;

            // --- Gestion des anneaux de glow ---
            if (_remainingSeconds <= 20)
            {
                // Nombre d'anneaux à afficher (1 à 5)
                int requiredGlowCircles = Math.Min(maxGlowCircles, (int)((20 - _remainingSeconds) / 2f) + 1);

                // Ajoute les anneaux manquants
                while (glowIntensities.Count < requiredGlowCircles)
                {
                    glowIntensities.Add(0f); // Commence à 0 (invisible)
                }

                // Met à jour l'opacité des anneaux (progressivement)
                for (int i = 0; i < glowIntensities.Count; i++)
                {
                    // Si l'anneau doit être affiché, augmente son opacité progressivement
                    if (i < requiredGlowCircles)
                    {
                        glowIntensities[i] = Math.Min(1f, glowIntensities[i] + glowFadeInSpeed);
                    }
                }
            }
            else
            {
                glowIntensities.Clear();
            }

            // --- Pulsation à 10 secondes ---
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
                    _ambientPlayer.Stop();
                    _ambientPlayer.Seek(0);
                    _ambientPlayer.Play();
                    isSoundPlayed = true;
                }
            }
            else
            {
                isPulsing = false;
                pulseProgress = 0f;
            }

            // Met à jour les propriétés du Drawable
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
                    await AnimateGlowFadeOut();
                    OnNextPlayer();
                    break;
                }
            }

            await Task.Delay(50);
        }
    }

    private async Task AnimateGlowFadeOut()
    {
        // Fade-out progressif : tous les anneaux disparaissent ensemble
        int steps = 60; // Plus d'étapes pour un effet fluide
        float initialOpacity = 1f;

        for (int step = 0; step <= steps; step++)
        {
            float progress = step / (float)steps;
            float opacity = initialOpacity * (1f - progress);

            // Applique à tous les anneaux
            for (int i = 0; i < _circleGameDrawable.GlowIntensities.Count; i++)
            {
                _circleGameDrawable.GlowIntensities[i] = opacity;
            }

            GameCanvas.Invalidate();
            await Task.Delay(16); // 16ms par étape (~1 seconde au total)
        }

        // Réinitialise les anneaux
        _circleGameDrawable.GlowIntensities.Clear();
    }
}