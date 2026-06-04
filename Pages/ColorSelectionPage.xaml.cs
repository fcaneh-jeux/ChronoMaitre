using HurryUpDavid.Models;

namespace HurryUpDavid.Pages;

public partial class ColorSelectionPage : ContentPage
{
    private readonly GameSettings _gameSettings;
    private int _currentPlayerIndex = 0;
    private List<Color> _selectedColors = new();
    private readonly List<Color> _availableColors = new()
    {
        Colors.Red,
        Colors.Green,
        Colors.Blue,
        Colors.Yellow,
        Colors.Orange,
        Colors.Purple,
        Colors.Black,
        Colors.White
    };

    public ColorSelectionPage(GameSettings gameSettings)
    {
        InitializeComponent();

        _gameSettings = gameSettings;
        RenderColors();
        UpdateInstruction();
    }

    private void RenderColors()
    {
        ColorsLayout.Children.Clear();
        foreach (var color in _availableColors)
        {
            var colorBox = new BoxView
            {
                Color = color,
                WidthRequest = 50,
                HeightRequest = 50,
                CornerRadius = 25,
                Margin = new Thickness(5),
                HorizontalOptions = LayoutOptions.Center
            };
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnColorSelected(color);
            colorBox.GestureRecognizers.Add(tapGesture);
            ColorsLayout.Children.Add(colorBox);
        }
    }

    private void UpdateInstruction()
    {
        InstructionLabel.Text = $"Player {_currentPlayerIndex + 1}, select your color";
    }

    private void OnColorSelected(Color color)
    {
        _selectedColors.Add(color);
        _availableColors.Remove(color);

        _currentPlayerIndex++;

        if (_selectedColors.Count == _gameSettings.PlayerCount)
        {
            GoToGame();
        }
        else
        {
            UpdateInstruction();
            RenderColors();
        }
    }

    private async void GoToGame()
    {
        await Navigation.PushAsync(new GamePage(_gameSettings, _selectedColors));
    }
}

