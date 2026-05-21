namespace SocketApp.PlantDispatcher.Maui;

public sealed class App : Application
{
    public App()
    {
        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);
        window.Title = "Пульт состояния установок";
        return window;
    }
}
