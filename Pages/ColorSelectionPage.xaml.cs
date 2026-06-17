using HurryUpDavid.Models;

namespace HurryUpDavid.Pages;

public partial class ColorSelectionPage : ContentPage
{
    private readonly GameSettings _gameSettings;
    private int _currentPlayerIndex = 0;
    private List<Color> _selectedColors = new();
    private readonly List<Color> _availableColors = new()
    {
        Color.FromArgb("#FF5252"),  // Rouge vif
        Color.FromArgb("#4CAF50"),  // Vert
        Color.FromArgb("#2196F3"),  // Bleu vif
        Color.FromArgb("#FFEB3B"),  // Jaune
        Color.FromArgb("#9C27B0"),  // Violet foncé
        Color.FromArgb("#FF9800"),  // Orange
        Color.FromArgb("#1A1A1A"),  // Gris foncé (remplace Black)
        Color.FromArgb("#EEEEEE")   // Blanc cassé (remplace White)
    };

    private List<Grid> _colorContainers = new List<Grid>();
    private List<BoxView> _colorBoxes = new List<BoxView>();
    private List<Task> _animationTasks = new();
    private readonly Random _random = new();

    public ColorSelectionPage(GameSettings gameSettings)
    {
        InitializeComponent();

        _gameSettings = gameSettings;
        RenderColors();
        UpdateInstruction();
    }

    private void RenderColors()
    {
        ColorsGrid.Children.Clear();
        _colorContainers.Clear();
        _colorBoxes.Clear();

        for (int i = 0; i < _availableColors.Count; i++)
        {
            int row = i / 2;
            int column = i % 2;

            Grid colorContainer = new Grid
            {
                WidthRequest = 50,
                HeightRequest = 50,
                Margin = new Thickness(5),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            BoxView colorBox = new BoxView
            {
                Color = _availableColors[i],
                WidthRequest = 50,
                HeightRequest = 50,
                CornerRadius = 25,
                BackgroundColor = Colors.Transparent
            };

            colorContainer.Children.Add(colorBox);

            TapGestureRecognizer tapGesture = new TapGestureRecognizer();
            int colorIndex = i;
            tapGesture.Tapped += (s, e) => OnColorSelected(colorIndex,colorContainer, colorBox);
            colorBox.GestureRecognizers.Add(tapGesture);

            
            ColorsGrid.Add(colorContainer, column, row);
        }
    }

    private void UpdateInstruction()
    {
        InstructionLabel.Text = $"Joueur {_currentPlayerIndex + 1}, choisissez votre couleur";
    }

    private async void OnColorSelected(int colorIndex, Grid colorContainer, BoxView colorBox)
    {
        if (colorIndex < 0 || colorIndex >= _availableColors.Count)
            return;

        Color selectedColor = _availableColors[colorIndex];
        colorBox.IsEnabled = false;

        _selectedColors.Add(selectedColor);

        // Animation du nuage
        Task animationTAsk = AnimateDustExplosion(colorContainer, colorBox, selectedColor);
        _animationTasks.Add(animationTAsk);

        _currentPlayerIndex++;

        if (_selectedColors.Count == _gameSettings.PlayerCount)
        {
            // Désactive toutes les pastilles restantes
            foreach (var box in _colorBoxes)
            {
                box.IsEnabled = false;
            }
            await Task.WhenAll(animationTAsk);
            GoToGame();
        }
        else
        {
            UpdateInstruction();
        }
    }

    private async Task AnimateDustExplosion(Grid container, BoxView colorBox, Color explosionColor)
    {
        // Détermine la direction du déplacement (gauche ou droite) selon le placement dans la grid
        int column = Grid.GetColumn(container);
        bool isLeftSide = column == 0;
        
        // Crée les positions des particules du nuage à venir
        List<BoxView> clouds = new();

        (double x, double y, double size)[] cloudTemplate =
        {
        (-12, -8, 16),
        (12, -8, 16),
        (-18, 6, 18),
        (0, 0, 32),      // centre principal
        (0, -2, 32),
        (18, 6, 18),
        (-8, 18, 16),
        (8, 18, 16),
        (0, 10, 18)
    };

        // Crée chaque particule du nuage 
        foreach (var cloud in cloudTemplate)
        {
            BoxView puff = new()
            {
                Color = Color.FromArgb("#F0F0F0"),
                WidthRequest = cloud.size,
                HeightRequest = cloud.size,
                CornerRadius = (int)(cloud.size / 2),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TranslationX = cloud.x,
                TranslationY = cloud.y,
                Opacity = 0
            };
            container.Children.Add(puff);
            clouds.Add(puff);
        }

        // Apparition RAPIDE du nuage
        foreach (BoxView cloud in clouds)
        {
            cloud.Opacity = 0.8;
        }
        await Task.Delay(100); // Pause pour voir l'apparition

        // Grossissement LENT du nuage
        await Task.WhenAll(clouds.Select(cloud => cloud.ScaleToAsync(1.5, 500, Easing.CubicOut)));
        await Task.Delay(200); // Pause pour voir le grossissement

        // La pastille disparaît derrière le nuage
        colorBox.Opacity = 0;
        await Task.Delay(200); // Pause pour voir la disparition de la pastille

        // Déplacement du nuage + grossissement + disparition
        List<Task> tasks = new();

        double driftX = isLeftSide ? -120 : 120;
        

        foreach (BoxView cloud in clouds)
        {
            double localX = _random.NextDouble() * 8 - 4;
            double localY = _random.NextDouble() * 6 - 3;

            double targetScale = 1.5 + _random.NextDouble() * 0.4;

            tasks.Add(Task.WhenAll(cloud.TranslateToAsync(cloud.TranslationX + driftX + localX, cloud.TranslationY + localY, 350, Easing.CubicOut), cloud.ScaleToAsync(targetScale, 1200, Easing.CubicOut), cloud.FadeToAsync(0, 1200, Easing.CubicIn)));
        }

        await Task.WhenAll(tasks);

        // Nettoyage
        foreach (BoxView cloud in clouds)
        {
            container.Children.Remove(cloud);
        }
        
        await Task.Delay(200);
    }

    private void GoToGame()
    {
        if (_gameSettings.GameMode == GameMode.TurnTimer)
        {
            Application.Current.Windows[0].Page = new NavigationPage(new GamePage(_gameSettings, _selectedColors));
        }
        else if(_gameSettings.GameMode == GameMode.TimeBank)
        {
            Application.Current.Windows[0].Page = new NavigationPage(new TimeBankGamePage(_gameSettings, _selectedColors));
        }
        else
        {
            throw new NotImplementedException($"Mode {_gameSettings.GameMode} non géré");
        }
    }
}

