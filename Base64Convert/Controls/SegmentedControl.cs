using Microsoft.Maui.Controls.Shapes;

namespace Base64Convert.Controls;

/// <summary>
/// A two-option switch, coloured with the system accent.
/// </summary>
public class SegmentedControl : ContentView
{
    private readonly Border _firstSegment;
    private readonly Border _secondSegment;
    private readonly Label _firstLabel;
    private readonly Label _secondLabel;
    private bool _isFirstSelected = true;

    public SegmentedControl()
    {
        (_firstSegment, _firstLabel) = CreateSegment(() => Select(first: true));
        (_secondSegment, _secondLabel) = CreateSegment(() => Select(first: false));

        Grid segments = new()
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
        };
        segments.Add(_firstSegment, 0);
        segments.Add(_secondSegment, 1);

        Border track = new()
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = 3,
            HeightRequest = 40,
            Content = segments,
        };
        track.SetDynamicResource(BackgroundColorProperty, SystemTheme.CardBackground);
        track.SetDynamicResource(Border.StrokeProperty, SystemTheme.CardStroke);

        Content = track;
        ApplyStyle();
    }

    public event EventHandler? SelectionChanged;

    public string FirstText
    {
        get => _firstLabel.Text;
        set => _firstLabel.Text = value;
    }

    public string SecondText
    {
        get => _secondLabel.Text;
        set => _secondLabel.Text = value;
    }

    public bool IsFirstSelected
    {
        get => _isFirstSelected;
        set => Select(value);
    }

    private void Select(bool first)
    {
        if (_isFirstSelected == first)
        {
            return;
        }

        _isFirstSelected = first;
        ApplyStyle();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyStyle()
    {
        StyleSegment(_firstSegment, _firstLabel, _isFirstSelected);
        StyleSegment(_secondSegment, _secondLabel, !_isFirstSelected);
    }

    private static (Border Segment, Label Label) CreateSegment(Action onTapped)
    {
        Label label = new()
        {
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        Border segment = new()
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Content = label,
        };

        TapGestureRecognizer tap = new();
        tap.Tapped += (_, _) => onTapped();
        segment.GestureRecognizers.Add(tap);

        return (segment, label);
    }

    private static void StyleSegment(Border segment, Label label, bool selected)
    {
        segment.SetDynamicResource(BackgroundColorProperty, selected ? SystemTheme.Accent : SystemTheme.CardBackground);
        label.SetDynamicResource(Label.TextColorProperty, selected ? SystemTheme.OnAccent : SystemTheme.TextSecondary);
        label.FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None;
    }
}
