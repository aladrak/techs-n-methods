using BinaryControlLib;

namespace BinaryControlMAUI.Views;
public class StartupPage : ContentPage
{
    private readonly Entry _nameEntry;
    private FileManager _fileManager;
    private readonly Action _toMain;
    public StartupPage(FileManager fm, Action toMain)
    {
        _fileManager = fm;
        _toMain = toMain;
        
        Title = "Выбор базы данных";
        BackgroundColor = Colors.White;

        _nameEntry = new Entry
        {
            Placeholder = "Название базы данных",
            FontSize = 16,
            TextColor = Colors.Black,
            Margin = new Thickness(20),
            HorizontalOptions = LayoutOptions.Fill
        };

        var createBtn = new Button
        {
            Text = "Создать",
            WidthRequest = 150,
            Margin = new Thickness(10)
        };
        createBtn.Clicked += (_, _) => OnCreateClicked();

        var openBtn = new Button
        {
            Text = "Открыть",
            WidthRequest = 150,
            Margin = new Thickness(10)
        };
        openBtn.Clicked += (_, _) => OnOpenClicked();

        var statusLabel = new Label
        {
            Text = "",
            TextColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(10)
        };

        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label 
                { 
                    Text = "Название базы данных:",
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 30, 0, 10)
                },
                _nameEntry,
                createBtn,
                openBtn,
                statusLabel
            },
            Padding = 20,
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
    }

    private async void OnCreateClicked()
    {
        string name = _nameEntry.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Ошибка", "Введите название", "OK");
            return;
        }

        try
        {
            var fm = new FileManager();
            fm.CreateDatabase(name);
            fm.Dispose();

            _fileManager.CreateDatabase(name);
            _toMain();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnOpenClicked()
    {
        string name = _nameEntry.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Ошибка", "Введите название", "OK");
            return;
        }

        try
        {
            if (!Path.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Binfile\\" + name + ".prd")) 
                throw new Exception("Неправильный путь.");
            _fileManager.OpenDatabase(name);
            _toMain();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
    }
}