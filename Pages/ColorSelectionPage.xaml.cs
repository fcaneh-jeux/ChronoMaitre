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
        // 1. Détermine la direction du déplacement (gauche ou droite)
        int column = Grid.GetColumn(container);
        bool isLeftSide = column == 0;
        double baseMoveDirection = isLeftSide ? -1 : 1; // Direction de base (-1 pour gauche, +1 pour droite)

        // 2. Crée les particules du nuage
        List<BoxView> clouds = new();

        (double x, double y, double size)[] cloudData =
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

        // 3. Crée chaque particule du nuage
        foreach (var cloud in cloudData)
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

        // 4. Apparition RAPIDE du nuage
        foreach (BoxView cloud in clouds)
        {
            cloud.Opacity = 0.8;
        }
        await Task.Delay(100); // Pause pour voir l'apparition

        // 5. Grossissement LENT du nuage
        await Task.WhenAll(clouds.Select(cloud => cloud.ScaleToAsync(1.5, 500, Easing.CubicOut)));
        await Task.Delay(200); // Pause pour voir le grossissement

        // 6. La pastille disparaît
        colorBox.Opacity = 0;
        await Task.Delay(200); // Pause pour voir la disparition de la pastille

        // 7. Déplacement du nuage + grossissement + disparition
        List<Task> tasks = new();

        double driftX = isLeftSide ? -60 : 60;
        Random random = new();

        foreach (var cloud in clouds)
        {
            double localX = random.NextDouble() * 8 - 4;
            double localY = random.NextDouble() * 6 - 3;

            double targetScale = 1.5 + random.NextDouble() * 0.4;

            tasks.Add(Task.WhenAll(cloud.TranslateToAsync(cloud.TranslationX + driftX + localX, cloud.TranslationY + localY, 1200, Easing.CubicOut), cloud.ScaleToAsync(targetScale, 1200, Easing.CubicOut), cloud.FadeToAsync(0, 1200, Easing.CubicIn)));
        }
        
        // 8. Nettoyage
        foreach (BoxView cloud in clouds)
        {
            container.Children.Remove(cloud);
        }
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

