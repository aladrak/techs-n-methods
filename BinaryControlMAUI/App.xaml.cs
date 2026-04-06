using BinaryControlLib;
using BinaryControlMAUI.ViewModels;
using BinaryControlMAUI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BinaryControlMAUI;

public partial class App : Application
{
    private readonly FileManager _fileManager;
    
    public App()
    {
        InitializeComponent();
        _fileManager = new FileManager();
        MainPage = new NavigationPage(new StartupPage(_fileManager, OnDatabaseReady));
    }

    private void OnDatabaseReady()
    {
        var viewModel = new MainViewModel(_fileManager);
        var mainPage = new MainPage(viewModel);
        
        viewModel.OnDatabaseOpened();
        
        MainPage = new NavigationPage(mainPage);
    }
}