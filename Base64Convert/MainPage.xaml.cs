namespace Base64Convert;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnSectionChanged(object? sender, EventArgs e)
    {
        TextSection.IsVisible = SectionSwitch.IsFirstSelected;
        ImageSection.IsVisible = !SectionSwitch.IsFirstSelected;
    }
}
