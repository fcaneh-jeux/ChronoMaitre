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
    private bool _isRunning = false;
    private bool _isPaused = false;
    private int _currentPlayerIndex = 0;
    private IDispatcherTimer? _timer;
    private readonly Dictionary<int, Border> _playerBorders = new();
    private Border? _ghostBorder;
    private const double Spacing = 57;
    private bool _initialLayoutDone;

    public TimeBankGamePage(GameSettings gameSettings, List<Color> playerColors)
    {
        InitializeComponent();
        _gameSettings = gameSettings;
        _playerColors = playerColors;
        _isRunning = false;
        _isPaused = false;

        // Initialise les joueurs avec leur couleur et leur temps
        for (int i = 0; i < _gameSettings.PlayerCount; i++)
        {
            _players.Add(new PlayerInfo
            {
                Color = _playerColors[i],
                RemainingTime = TimeSpan.FromMinutes(_gameSettings.TimeBankMinutes)
            });
        }

        // créaton des borders de chaque joueur
        for (int i = 0; i < _players.Count; i++)
        {
            Border border = CreatePlayerBorder(i);
            border.Opacity = 0;
            _playerBorders[i] = border;
            GameArea.Children.Add(border);
        }

        // border fantôme pour animation de rotation
        _ghostBorder = CreatePlayerBorder(0);
        _ghostBorder.IsVisible = false;
        GameArea.Children.Add(_ghostBorder);

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;

        // Affiche les joueurs
        GameArea.SizeChanged += OnGameAreaSizeChanged;
    }

    /// <summary>
    /// Calcule l'écart vertical entre une carte et le joueur actif.
    /// Les cartes éloignées sont volontairement compressées afin
    /// de conserver un rouleau compact même avec beaucoup de joueurs.
    /// </summary>
    private double GetOffset(int distance)
    {
        return distance switch
        {
            0 => 0,
            1 => Spacing,
            2 => Spacing * 1.9,
            3 => Spacing * 2.8,
            _ => Spacing * 3.8
        };
    }

    /// <summary>
    /// attend de connaitre la taille de GameArena avant d'uutiliser RenderPlayers pour éviter in positionnement trop à gacuhe au lancement sur certains telephones
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnGameAreaSizeChanged(object? sender, EventArgs e)
    {
        if (_initialLayoutDone)
        {
            return;
        }

        if (GameArea.Width <= 0)
        {
            return;
        }

        _initialLayoutDone = true;
        RenderPlayers();
        foreach (Border border in _playerBorders.Values)
        {
            border.Opacity = 1;
        }
    }
    /// <summary>
    /// après création d'un boirder fantome pour l'animation du carrousel, remplissage du border avec les éléments de exitingBorder
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    private void CopyBorderVisual(Border source, Border target)
    {
        target.BackgroundColor = source.BackgroundColor;
        target.Scale = source.Scale;
        target.Opacity = source.Opacity;

        Label sourceLabel = (Label)source.Content;
        Label targetLabel = (Label)target.Content;

        targetLabel.Text = sourceLabel.Text;
    }

    /// <summary>
    /// création de border associé à un joueur
    /// couleur et temps sont maj dans renderplayers
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    private Border CreatePlayerBorder(int playerIndex)
    {
        PlayerInfo player = _players[playerIndex];

        Border playerBorder = new()
        {
            StrokeThickness = 0,
            Padding = 6,
            WidthRequest = 280,
            HeightRequest = 55,

            StrokeShape = new RoundRectangle
            {
                CornerRadius = 12
            }
        };

        Label infoLabel = new()
        {
            Text = $"Joueur {playerIndex + 1} : 01:00",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        playerBorder.Content = infoLabel;

        return playerBorder;
    }

    /// <summary>
    /// animation de tap sur le border du joueur en cours
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCurrentPlayerTapped(object sender, TappedEventArgs e)
    {
        if (!_isRunning) return;
        if (_isPaused) return;
        NextPlayer();
    }

    /// <summary>
    /// passage et initialisation du joueur suivant
    /// </summary>
    /// <returns></returns>
    private async Task NextPlayer()
    {
        // vérification que tous les joueurs n'ont pas fini d'écouler leur temps
        if (_players.All(player => player.RemainingTime <= TimeSpan.Zero))
        {
            _isRunning = false;
            _timer?.Stop();
            return;
        }

        await AnimateRotation();

        do
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }
        while (_players[_currentPlayerIndex].RemainingTime <= TimeSpan.Zero);

        RenderPlayers();
    }

    /// <summary>
    /// ordre des joueurs à partir du joueur en cours, pour l'affichage et animations
    /// </summary>
    /// <returns></returns>
    private List<int> GetDisplayOrder()
    {
        return GetDisplayOrderForPlayer(_currentPlayerIndex);
    }
    /// <summary>
    /// Construit l'ordre circulaire des joueurs autour du joueur actif.
    /// Exemple pour 5 joueurs et J1 actif :
    /// J4 J5 J1 J2 J3
    /// </summary>
    private List<int> GetDisplayOrderForPlayer(int currentPlayerIndex)
    {
        List<int> displayOrder = new();

        int beforeCount = (_players.Count - 1) / 2;
        int afterCount = _players.Count - beforeCount - 1;

        for (int offset = -beforeCount; offset <= afterCount; offset++)
        {
            int playerIndex = (currentPlayerIndex + offset + _players.Count) % _players.Count;
            displayOrder.Add(playerIndex);
        }

        return displayOrder;
    }

    /// <summary>
    /// anime le mouvement de rouleau des borders : 
    /// le mreier border monte et disparait
    /// les borders suivants montent d'un cran, en adaptant de scale et opacity
    /// le ghost réagit comme s'il arrivait d'un tour du venant du haut et réapparait progressivement
    /// </summary>
    /// <returns></returns>
    private async Task AnimateRotation()
    {
        // espace référence entre border 
        double spacing = Spacing;
        int exitingPlayer = GetDisplayOrder().First();
        Border exitingBorder = _playerBorders[exitingPlayer];

        // Prépare le futur état
        int nextPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

        Dictionary<int, double> currentPositions = GetPlayerYPositions(_currentPlayerIndex);
        Dictionary<int, double> futurePositions = GetPlayerYPositions(nextPlayerIndex);

        List<int> futureOrder = GetDisplayOrderForPlayer(nextPlayerIndex);
        int futureCurrentPosition = futureOrder.IndexOf(nextPlayerIndex);

        // Ghost = copie du joueur qui sort
        CopyBorderVisual(exitingBorder, _ghostBorder!);


        // Position du dernier joueur visible
        int lastVisiblePlayer = GetDisplayOrder().Last();
        double lastVisibleY = AbsoluteLayout.GetLayoutBounds(_playerBorders[lastVisiblePlayer]).Y;

        // Détermine l'état visuel final du ghost
        int ghostFuturePosition = futureOrder.IndexOf(exitingPlayer);
        var ghostVisual = GetVisualState(ghostFuturePosition, futureCurrentPosition);

        // Ghost juste sous le dernier
        double borderWidth = 280;
        double x = (GameArea.Width - borderWidth) / 2;

        AbsoluteLayout.SetLayoutBounds(_ghostBorder, new Rect(x, lastVisibleY + spacing, borderWidth, 55));
        _ghostBorder.IsVisible = true;
        _ghostBorder.Opacity = 0;
        _ghostBorder.Scale = ghostVisual.Scale;

        List<Task> animations = new();

        // Le joueur qui sort s'efface
        animations.Add(Task.WhenAll(exitingBorder.TranslateToAsync(0, -30, 800, Easing.CubicInOut), exitingBorder.FadeToAsync(0, 800, Easing.CubicInOut)));

        // Tous les vrais joueurs
        foreach (int player in _playerBorders.Keys)
        {
            Border border = _playerBorders[player];

            if (player == exitingPlayer)
            {
                continue;
            }

            double currentY = currentPositions[player];
            double futureY = futurePositions[player];
            animations.Add(border.TranslateToAsync(0, futureY - currentY, 800, Easing.CubicInOut));
            int futurePosition = futureOrder.IndexOf(player);

            if (futurePosition >= 0)
            {
                var visual = GetVisualState(futurePosition, futureCurrentPosition);

                animations.Add(border.ScaleToAsync(visual.Scale, 800, Easing.CubicInOut));
                animations.Add(border.FadeToAsync(visual.Opacity, 800, Easing.CubicInOut));
            }
        }

        // Ghost
        animations.Add(Task.WhenAll(_ghostBorder.TranslateToAsync(0, -spacing, 800, Easing.CubicInOut), _ghostBorder.FadeToAsync(ghostVisual.Opacity, 800, Easing.CubicInOut)));

        await Task.WhenAll(animations);

        // Nettoyage
        foreach (Border border in _playerBorders.Values)
        {
            border.TranslationY = 0;
        }

        exitingBorder.Opacity = 1;

        _ghostBorder.TranslationY = 0;
        _ghostBorder.IsVisible = false;
    }

    /// <summary>
    /// modification des borders selon la position p/ à la border centrale du joueur en cours
    /// </summary>
    /// <param name="position"></param>
    /// <param name="currentPlayerPosition"></param>
    /// <returns></returns>
    private (double Scale, double Opacity) GetVisualState(int position, int currentPlayerPosition)
    {
        bool isCurrentPlayer = position == currentPlayerPosition;
        double opacity;
        if (isCurrentPlayer)
        {
            opacity = 1.0;
        }
        else
        {
            int distance =
                Math.Abs(position - currentPlayerPosition);

            opacity = distance switch
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
            scale = 1.08;
        }
        else
        {
            int distance = Math.Abs(position - currentPlayerPosition);
            scale = distance switch
            {
                1 => 0.98,
                2 => 0.90,
                3 => 0.82,
                _ => 0.75
            };
        }

        return (scale, opacity);
    }

    /// <summary>
    /// gestion du timer, modification du temps à chaque tic
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
        }

    }

    /// <summary>
    /// Calcule la position verticale théorique de chaque joueur pour un joueur actif donné.
    /// Utilisé par :
    /// - RenderPlayers()
    /// - AnimateRotation()
    ///
    /// Permet d'avoir exactement les mêmes positions pour l'affichage statique et les animations.
    /// </summary>
    private Dictionary<int, double> GetPlayerYPositions(int currentPlayerIndex)
    {
        Dictionary<int, double> positions = new();

        List<int> displayOrder = new();

        int beforeCount = (_players.Count - 1) / 2;
        int afterCount = _players.Count - beforeCount - 1;

        for (int offset = -beforeCount; offset <= afterCount; offset++)
        {
            int playerIndex = (currentPlayerIndex + offset + _players.Count) % _players.Count;
            displayOrder.Add(playerIndex);
        }

        int currentPlayerPosition = displayOrder.IndexOf(currentPlayerIndex);

        double centerY = 180;

        for (int position = 0; position < displayOrder.Count; position++)
        {
            int playerIndex = displayOrder[position];

            int distance = Math.Abs(position - currentPlayerPosition);
            double offset = GetOffset(distance);

            double y = position < currentPlayerPosition ? centerY - offset : centerY + offset;
            positions[playerIndex] = y;
        }

        return positions;
    }

    /// <summary>
    /// affiche les joueurs et borders après les animations
    /// </summary>
    private void RenderPlayers()
    {
        //ordre d'affichage des players autour du current player
        List<int> displayOrder = GetDisplayOrder();
        int currentPlayerPosition = displayOrder.IndexOf(_currentPlayerIndex);

        for (int position = 0; position < displayOrder.Count; position++)
        {
            int index = displayOrder[position];

            PlayerInfo player = _players[index];
            Border playerBorder = _playerBorders[index];

            playerBorder.GestureRecognizers.Clear();

            // installation du tap sur le current player
            if (index == _currentPlayerIndex)
            {
                TapGestureRecognizer tap = new();

                tap.Tapped += OnCurrentPlayerTapped;

                playerBorder.GestureRecognizers.Add(tap);
            }

            // récuperation des positions, scale et opacity correspondantes
            int distanceFromCenter = Math.Abs(position - currentPlayerPosition);

            var visual = GetVisualState(position, currentPlayerPosition);
            double opacity = visual.Opacity;
            double scale = visual.Scale;

            playerBorder.BackgroundColor = player.Color;
            if (Math.Abs(playerBorder.Scale - scale) > 0.001)
            {
                playerBorder.Scale = scale;
            }

            if (Math.Abs(playerBorder.Opacity - opacity) > 0.001)
            {
                playerBorder.Opacity = opacity;
            }

            // mise à jour des infos du lable : num de joueur et remainingtime
            Label infoLabel = (Label)playerBorder.Content;
            infoLabel.Text = $"Joueur {index + 1} : {FormatTimeSpan(player)}";

            // placement de GameArena
            double centerY = 180;
            int distance = Math.Abs(position - currentPlayerPosition);
            double offset = GetOffset(distance);
            double y = position < currentPlayerPosition ? centerY - offset : centerY + offset;
            double borderWidth = 280;
            double x = (GameArea.Width - borderWidth) / 2;
            AbsoluteLayout.SetLayoutBounds(playerBorder, new Rect(x, y, borderWidth, 55));
        }
    }

    /// <summary>
    /// affichage du temps restant de chaque joueur
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
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

    /// <summary>
    /// retour à la page des setups
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnHomeClicked(object sender, EventArgs e)
    {
        Application.Current.Windows[0].Page = new NavigationPage(new TimeBankSetupPage());
    }

    /// <summary>
    /// sortie de l'appli manuelle
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnExitClicked(object sender, EventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.Quit();
        }
    }

    /// <summary>
    /// lancement lors du clic sur le bouton démarrer, pause du timer
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
            }
            else
            {
                PauseResumeButton.Text = "❚❚";
            }
        }
    }
}