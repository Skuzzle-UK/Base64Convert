using Microsoft.Extensions.DependencyInjection;

namespace Base64Convert;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        SystemTheme.Apply(this);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Width/Height only apply on desktop; Android always runs full screen.
        return new Window(new AppShell())
        {
            Width = 480,
            Height = 700,
            MinimumWidth = 360,
            MinimumHeight = 480,
        };
    }
}