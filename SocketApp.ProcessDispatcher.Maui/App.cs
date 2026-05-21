namespace SocketApp.ProcessDispatcher.Maui;

public sealed class App : Application
{
    public App()
    {
        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);
        window.Title = "Пульт диспетчера процесса";
        return window;
    }
}
