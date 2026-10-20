namespace LeltarKezelo;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    // .NET 9+ minta. .NET 8 esetén helyette a konstruktorban: MainPage = new AppShell();
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new AppShell()) { Title = "LeltárKezelő" };
}
