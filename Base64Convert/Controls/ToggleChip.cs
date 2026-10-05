using Microsoft.Maui.Controls.Shapes;

namespace Base64Convert.Controls;

/// <summary>
/// A pill-shaped on/off option, filled with the system accent when on.
/// </summary>
public class ToggleChip : ContentView
{
    private readonly Border _chip;
    private readonly Label _label;
    private bool _isChecked;

    public ToggleChip()
    {
        _label = new Label
        {
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
        };

        _chip = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = new Thickness(14, 0),
            HeightRequest = 40,
            Content = _label,
        };

        TapGestureRecognizer tap = new();
        tap.Tapped += (_, _) => IsChecked = !IsChecked;
        _chip.GestureRecognizers.Add(tap);

        Content = _chip;
        ApplyStyle();
    }

    public event EventHandler? Toggled;

    public string Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            ApplyStyle();
            Toggled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ApplyStyle()
    {
        _chip.SetDynamicResource(BackgroundColorProperty, _isChecked ? SystemTheme.Accent : SystemTheme.CardBackground);
        _chip.SetDynamicResource(Border.StrokeProperty, _isChecked ? SystemTheme.Accent : SystemTheme.CardStroke);
        _label.SetDynamicResource(Label.TextColorProperty, _isChecked ? SystemTheme.OnAccent : SystemTheme.TextSecondary);
        _label.FontAttributes = _isChecked ? FontAttributes.Bold : FontAttributes.None;
    }
}
