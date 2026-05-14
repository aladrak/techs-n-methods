using System.Reflection;
using AppInterfaces;

namespace DynamicDataApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        IAuth plugin;
        try
        {
            var asm = Assembly.LoadFrom("./authlib.dll");
            var pluginType = asm.GetExportedTypes()
                .FirstOrDefault(t => t.Name == "Auth");
            if (pluginType == null)
            {
                throw new Exception("Подобный тип не найден.");
            }

            plugin = (IAuth)Activator.CreateInstance(pluginType);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new AuthForm(plugin));
    }
}