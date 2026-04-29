namespace DataDrivenApp;

static class Program
{
    private static string? permissionsAddress { get; set; }
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        permissionsAddress = null;
        ApplicationConfiguration.Initialize();
        Application.Run(new AuthForm(permissionsAddress));
    }
}