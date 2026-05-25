using System.Collections.ObjectModel;
using System.Text;
using SocketApp.Core.Common;
using SocketApp.Core.FileExplorer;

namespace SocketApp.FileBrowser.Maui;

public sealed class MainPage : ContentPage
{
    private const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss";

    private readonly ObservableCollection<FileBrowserItemViewModel> _fileItems = [];
    private readonly ObservableCollection<string> _driveItems = [];
    private readonly SocketOptions _socketOptions = SocketOptions.ForFileBrowser();

    private readonly Entry _pathEntry;
    private readonly Entry _ipAddressEntry;
    private readonly Entry _portEntry;
    private readonly Picker _drivePicker;
    private readonly ListView _fileListView;
    private readonly Editor _clientLogEditor;
    private readonly Editor _serverLogEditor;
    private readonly Button _connectButton;
    private readonly Button _disconnectButton;
    private readonly Button _serverButton;
    private readonly Button _sendToServerButton;
    private readonly Button _sendToClientButton;

    private FileExplorerClientService? _clientService;
    private FileExplorerServerService? _serverService;
    private CancellationTokenSource? _serverCancellation;
    private CancellationTokenSource? _clientCancellation;
    private Task? _serverTask;
    private string? _selectedPath;

    public MainPage()
    {
        Title = "Программа для обмена данными между компьютерами";
        BackgroundColor = Color.FromArgb("#ECE9D8");

        _pathEntry = CreatePathEntry();
        _ipAddressEntry = CreateIpAddressEntry();
        _portEntry = CreatePortEntry();
        _drivePicker = CreateDrivePicker();
        _fileListView = CreateFileListView();
        _clientLogEditor = CreateLogEditor();
        _serverLogEditor = CreateLogEditor();
        _connectButton = CreateButton("Соединиться");
        _disconnectButton = CreateButton("Отключиться");
        _serverButton = CreateButton("Сервер включить");
        _sendToServerButton = CreateButton("Передать серверу");
        _sendToClientButton = CreateButton("Передать клиенту");

        _connectButton.Clicked += OnConnectClicked;
        _disconnectButton.Clicked += OnDisconnectClicked;
        _serverButton.Clicked += OnServerButtonClicked;
        _sendToServerButton.Clicked += OnSendToServerClicked;
        _sendToClientButton.Clicked += OnSendToClientClicked;

        _disconnectButton.IsEnabled = false;
        _sendToServerButton.IsEnabled = false;
        _sendToClientButton.IsEnabled = false;

        Content = BuildContent();
    }

    private View BuildContent()
    {
        Grid mainGrid = new()
        {
            Padding = new Thickness(8),
            RowSpacing = 6,
            ColumnSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(330)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        Grid pathBox = new()
        {
            ColumnSpacing = 4,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(80))
            }
        };
        pathBox.Add(_pathEntry, 0, 0);
        pathBox.Add(_drivePicker, 1, 0);

        mainGrid.Add(pathBox, 0, 0);
        mainGrid.Add(CreateHeaderLabel("Клиентская сторона"), 1, 0);
        mainGrid.Add(CreateHeaderLabel("Серверная сторона"), 2, 0);

        mainGrid.Add(WrapInFrame(_fileListView), 0, 1);
        mainGrid.Add(WrapInFrame(_clientLogEditor), 1, 1);
        mainGrid.Add(WrapInFrame(_serverLogEditor), 2, 1);

        Grid serverControls = new()
        {
            ColumnSpacing = 6,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(80)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(72)),
                new ColumnDefinition(new GridLength(128))
            }
        };
        serverControls.Add(new Label
        {
            Text = "IP-адрес",
            VerticalTextAlignment = TextAlignment.Center,
            FontSize = 14,
            TextColor = Colors.Black
        }, 0, 0);
        serverControls.Add(_ipAddressEntry, 1, 0);
        serverControls.Add(_portEntry, 2, 0);
        serverControls.Add(_serverButton, 3, 0);
        mainGrid.Add(serverControls, 0, 2);

        Grid clientControls = new()
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };
        clientControls.Add(_connectButton, 0, 0);
        clientControls.Add(_disconnectButton, 1, 0);
        clientControls.Add(CreateExitButton(), 2, 0);
        mainGrid.Add(clientControls, 0, 3);

        Grid transferControls = new()
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };
        transferControls.Add(_sendToServerButton, 0, 0);
        transferControls.Add(_sendToClientButton, 1, 0);
        mainGrid.Add(transferControls, 0, 4);

        return mainGrid;
    }

    private static Entry CreatePathEntry()
    {
        return new Entry
        {
            Text = string.Empty,
            Placeholder = "Путь к каталогу или текстовому файлу",
            FontSize = 14,
            HeightRequest = 32,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            PlaceholderColor = Colors.Gray,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
    }

    private Entry CreateIpAddressEntry()
    {
        return new Entry
        {
            Text = _socketOptions.Host,
            FontSize = 14,
            MinimumWidthRequest = 40,
            HeightRequest = 32,
            Keyboard = Keyboard.Numeric,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center
        };
    }

    private Entry CreatePortEntry()
    {
        return new Entry
        {
            Text = _socketOptions.Port.ToString(),
            FontSize = 14,
            WidthRequest = 20,
            HeightRequest = 32,
            Keyboard = Keyboard.Numeric,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center
        };
    }

    private Picker CreateDrivePicker()
    {
        Picker picker = new()
        {
            Title = "Диски",
            TitleColor = Colors.Black,
            FontSize = 14,
            HeightRequest = 32,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            ItemsSource = _driveItems
        };

        picker.SelectedIndexChanged += (_, _) =>
        {
            if (picker.SelectedItem is string drive)
            {
                _selectedPath = drive;
                _pathEntry.Text = drive;
            }
        };

        return picker;
    }

    private ListView CreateFileListView()
    {
        ListView listView = new()
        {
            ItemsSource = _fileItems,
            BackgroundColor = Colors.White,
            HasUnevenRows = false,
            RowHeight = 24,
            SeparatorVisibility = SeparatorVisibility.None,
            SelectionMode = ListViewSelectionMode.Single,
            ItemTemplate = new DataTemplate(() =>
            {
                TextCell textCell = new();
                textCell.TextColor = Colors.Black;
                textCell.SetBinding(TextCell.TextProperty, nameof(FileBrowserItemViewModel.DisplayName));
                return textCell;
            })
        };

        listView.ItemSelected += (_, eventArgs) =>
        {
            if (eventArgs.SelectedItem is not FileBrowserItemViewModel item)
                return;

            _selectedPath = item.FullPath;
            _pathEntry.Text = item.FullPath;
        };

        return listView;
    }

    private static Editor CreateLogEditor()
    {
        return new Editor
        {
            Text = string.Empty,
            FontSize = 14,
            FontFamily = "Consolas",
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            IsReadOnly = true,
            AutoSize = EditorAutoSizeOption.Disabled
        };
    }

    private static Label CreateHeaderLabel(string text)
    {
        return new Label
        {
            Text = text,
            FontSize = 14,
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
    }

    private static Button CreateButton(string text)
    {
        return new Button
        {
            Text = text,
            FontSize = 13,
            HeightRequest = 34,
            Padding = new Thickness(6, 0),
            TextColor = Colors.Black,
            BackgroundColor = Color.FromArgb("#F0F0F0"),
            BorderColor = Colors.Gray,
            BorderWidth = 1,
            CornerRadius = 0
        };
    }

    private Button CreateExitButton()
    {
        Button button = CreateButton("Выход");
        button.Clicked += async (_, _) =>
        {
            await DisconnectClientAsync();
            StopServer();

            Application.Current?.Quit();
        };

        return button;
    }

    private static Border WrapInFrame(View content)
    {
        return new Border
        {
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeThickness = 1,
            Padding = 0,
            BackgroundColor = Colors.White,
            Content = content
        };
    }

    private async void OnServerButtonClicked(object? sender, EventArgs eventArgs)
    {
        if (_serverService is null) await StartServerAsync();
        else StopServer();
    }

    private async Task StartServerAsync()
    {
        if (!TryApplySocketOptions()) return;

        _serverService = new FileExplorerServerService();
        _serverCancellation = new CancellationTokenSource();
        _serverButton.Text = "Сервер отключить";
        AppendServerLog($"Сервер включён {DateTime.Now.ToString(DateTimeFormat)}");

        _serverTask = Task.Run(async () =>
        {
            try
            {
                await _serverService.StartAsync(
                    _socketOptions,
                    message => AppendServerLog(message.Text),
                    _serverCancellation.Token);
            }
            catch (Exception exception)
            {
                AppendServerLog($"Ошибка сервера: {exception.Message}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _serverService = null;
                    _serverButton.Text = "Сервер включить";
                });
            }
        });

        await Task.Delay(100);
    }

    private void StopServer()
    {
        if (_serverService is null) return;

        _serverCancellation?.Cancel();
        _serverService.Stop();
        _serverService.Dispose();
        _serverService = null;
        _serverButton.Text = "Сервер включить";
        AppendServerLog($"Сервер отключён {DateTime.Now.ToString(DateTimeFormat)}");
    }

    private async void OnConnectClicked(object? sender, EventArgs eventArgs)
    {
        if (!TryApplySocketOptions())
            return;

        _clientCancellation = new CancellationTokenSource();
        _clientService = new FileExplorerClientService();

        try
        {
            FileResponse response = await _clientService.ConnectAsync(_socketOptions, _clientCancellation.Token);
            AppendClientLog($"Клиент получил {DateTime.Now.ToString(DateTimeFormat)}");
            AppendServerLog($"Клиент соединен {DateTime.Now.ToString(DateTimeFormat)}");
            ApplyResponse(response);
            SetClientConnected(true);
        }
        catch (Exception exception)
        {
            AppendClientLog($"Ошибка соединения: {exception.Message}");
            await DisconnectClientAsync();
        }
    }

    private async void OnDisconnectClicked(object? sender, EventArgs eventArgs) 
        => await DisconnectClientAsync();

    private async Task DisconnectClientAsync()
    {
        if (_clientService is null) return;

        try
        {
            await _clientService.DisconnectAsync();
        }
        catch
        {
            //
        }

        await _clientService.DisposeAsync();
        _clientCancellation?.Cancel();
        _clientCancellation?.Dispose();
        _clientCancellation = null;
        _clientService = null;
        SetClientConnected(false);
        AppendClientLog($"Клиент отключён {DateTime.Now.ToString(DateTimeFormat)}");
    }

    private async void OnSendToServerClicked(object? sender, EventArgs eventArgs)
    {
        if (_clientService is null)
        {
            AppendClientLog("Клиент не подключён к серверу.");
            return;
        }

        string path = GetCurrentPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            AppendClientLog("Не выбран каталог или текстовый файл.");
            return;
        }

        try
        {
            AppendServerLog($"Сервер получил {DateTime.Now.ToString(DateTimeFormat)}");
            AppendServerLog(path);

            FileResponse response = await _clientService.RequestPathAsync(path, _clientCancellation?.Token ?? CancellationToken.None);
            AppendClientLog($"Клиент получил {DateTime.Now.ToString(DateTimeFormat)}");
            ApplyResponse(response);
        }
        catch (Exception exception)
        {
            AppendClientLog($"Ошибка передачи серверу: {exception.Message}");
        }
    }

    private async void OnSendToClientClicked(object? sender, EventArgs eventArgs)
    {
        if (_clientService is null)
        {
            AppendServerLog("Нет подключенного клиента.");
            return;
        }

        try
        {
            FileResponse response = await _clientService.RequestDrivesAsync(_clientCancellation?.Token ?? CancellationToken.None);
            AppendServerLog($"Сервер передал клиенту список устройств {DateTime.Now.ToString(DateTimeFormat)}");
            AppendClientLog($"Клиент получил {DateTime.Now.ToString(DateTimeFormat)}");
            ApplyResponse(response);
        }
        catch (Exception exception)
        {
            AppendServerLog($"Ошибка передачи клиенту: {exception.Message}");
        }
    }

    private bool TryApplySocketOptions()
    {
        string host = _ipAddressEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(host))
        {
            AppendClientLog("IP-адрес не указан.");
            return false;
        }

        if (!int.TryParse(_portEntry.Text, out int port) || port <= 0 || port > 65535)
        {
            AppendClientLog("Порт должен быть числом от 1 до 65535.");
            return false;
        }

        _socketOptions.Host = host;
        _socketOptions.Port = port;
        return true;
    }

    private string GetCurrentPath()
    {
        string path = _pathEntry.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(path))
            return path;

        return _selectedPath ?? string.Empty;
    }

    private void ApplyResponse(FileResponse response)
    {
        switch (response.Type)
        {
            case FileResponseType.Drives:
                ApplyDriveResponse(response);
                break;

            case FileResponseType.Directory:
                ApplyDirectoryResponse(response);
                break;

            case FileResponseType.TextFile:
                ApplyTextFileResponse(response);
                break;

            case FileResponseType.Error:
                AppendClientLog($"Ошибка: {response.Message}");
                break;

            case FileResponseType.Disconnected:
                AppendClientLog("Сервер подтвердил отключение.");
                break;
        }
    }

    private void ApplyDriveResponse(FileResponse response)
    {
        _driveItems.Clear();
        _fileItems.Clear();

        foreach (string drive in response.Drives)
        {
            _driveItems.Add(drive);
            _fileItems.Add(new FileBrowserItemViewModel(drive, drive, "Диск"));
        }

        if (_driveItems.Count > 0 && _drivePicker.SelectedIndex < 0)
        {
            _drivePicker.SelectedIndex = 0;
            _selectedPath = _driveItems[0];
            _pathEntry.Text = _driveItems[0];
        }

        AppendClientLog(string.Join(Environment.NewLine, response.Drives));
    }

    private void ApplyDirectoryResponse(FileResponse response)
    {
        _fileItems.Clear();
        _selectedPath = response.Path;
        _pathEntry.Text = response.Path ?? string.Empty;

        foreach (DirectoryTreeItem item in response.Items)
            AddDirectoryItem(item, 0);

        AppendClientLog(response.Path ?? string.Empty);
        AppendClientLog(FormatDirectoryItems(response.Items));
    }

    private void ApplyTextFileResponse(FileResponse response)
    {
        _selectedPath = response.Path;
        _pathEntry.Text = response.Path ?? string.Empty;
        AppendClientLog(response.Path ?? string.Empty);
        AppendClientLog(response.TextContent ?? string.Empty);
    }

    private void AddDirectoryItem(DirectoryTreeItem item, int level)
    {
        string prefix = new(' ', level * 4);
        string suffix = item.Type == DirectoryTreeItemType.Directory ? "\\" : string.Empty;
        _fileItems.Add(new FileBrowserItemViewModel($"{prefix}{item.Name}{suffix}", item.FullPath, item.Type.ToString()));

        foreach (DirectoryTreeItem child in item.Children)
            AddDirectoryItem(child, level + 1);
    }

    private static string FormatDirectoryItems(IEnumerable<DirectoryTreeItem> items)
    {
        StringBuilder builder = new();

        foreach (DirectoryTreeItem item in items)
            AppendDirectoryItem(builder, item, 0);

        return builder.ToString().TrimEnd();
    }

    private static void AppendDirectoryItem(StringBuilder builder, DirectoryTreeItem item, int level)
    {
        string prefix = new(' ', level * 4);
        string suffix = item.Type == DirectoryTreeItemType.Directory ? "\\" : string.Empty;
        builder.AppendLine($"{prefix}{item.Name}{suffix}");

        foreach (DirectoryTreeItem child in item.Children)
            AppendDirectoryItem(builder, child, level + 1);
    }

    private void SetClientConnected(bool isConnected)
    {
        _connectButton.IsEnabled = !isConnected;
        _disconnectButton.IsEnabled = isConnected;
        _sendToServerButton.IsEnabled = isConnected;
        _sendToClientButton.IsEnabled = isConnected;
    }

    private void AppendClientLog(string text) => AppendLog(_clientLogEditor, text);

    private void AppendServerLog(string text) => AppendLog(_serverLogEditor, text);
    

    private static void AppendLog(Editor editor, string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (string.IsNullOrEmpty(editor.Text))
                editor.Text = text;
            else
                editor.Text += Environment.NewLine + text;

            editor.CursorPosition = editor.Text.Length;
        });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await DisconnectClientAsync();
        StopServer();
    }
}

public sealed class FileBrowserItemViewModel(string displayName, string fullPath, string itemType)
{
    public string DisplayName { get; } = displayName;

    public string FullPath { get; } = fullPath;

    public string ItemType { get; } = itemType;
}
