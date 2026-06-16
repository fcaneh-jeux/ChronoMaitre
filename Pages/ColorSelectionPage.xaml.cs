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
            return; // Sécurité pour éviter ArgumentOutOfRangeException

        Color selectedColor = _availableColors[colorIndex];

        // Désactive la pastille (ne la supprime pas de la liste)
        colorBox.IsEnabled = true;

        // Ajoute la couleur sélectionnée
        _selectedColors.Add(selectedColor);

        // Animation du nuage de fumée
        await AnimateDustExplosion(colorContainer, colorBox, selectedColor);

        // Passe au joueur suivant
        _currentPlayerIndex++;

        if (_selectedColors.Count == _gameSettings.PlayerCount)
        {
            GoToGame();
        }
        else
        {
            UpdateInstruction();
        }
    }

    private async Task AnimateDustExplosion(Grid container, BoxView colorBox, Color explosionColor)
    {
        colorBox.Opacity = 0;
        colorBox.InputTransparent = true;

        List<BoxView> clouds = new();

        (double x, double y, double size)[] cloudData =
        {
            (-12, -8, 16),
            (12, -8, 16),
            (-18, 6, 18),
            (0, 0, 24),      // centre principal
            (18, 6, 18),
            (-8, 18, 16),
            (8, 18, 16),
            (0, 10, 18)      // remplit le trou du bas
        };

        foreach (var cloud in cloudData)
        {
            BoxView puff = new()
            {
                Color = Color.FromArgb("#DDDDDD"),
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

        foreach (BoxView cloud in clouds)
        {
            cloud.Opacity = 1;
        }

        await Task.WhenAll(clouds.Select(cloud => cloud.ScaleToAsync(1.5, 250, Easing.CubicOut)));

        await Task.Delay(500);

        await Task.WhenAll(clouds.Select(cloud => Task.WhenAll(cloud.TranslateToAsync(12, -4, 500, Easing.CubicOut), cloud.ScaleToAsync(2.1, 500,Easing.CubicOut), cloud.FadeToAsync(0, 500, Easing.CubicIn))));

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

