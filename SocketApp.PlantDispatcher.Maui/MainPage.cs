using SocketApp.Core.Common;
using SocketApp.Core.PlantStates;

namespace SocketApp.PlantDispatcher.Maui;

public sealed partial class MainPage : ContentPage
{
    private const int ButtonsPerRow = 4;

    private readonly Entry _hostEntry = new()
    {
        Text = AppConstants.Localhost,
        WidthRequest = 150
    };

    private readonly Entry _portEntry = new()
    {
        Text = AppConstants.PlantPort.ToString(),
        Keyboard = Keyboard.Numeric,
        WidthRequest = 90
    };

    private readonly Button _startButton = new()
    {
        Text = "Запустить пульт"
    };

    private readonly Button _stopButton = new()
    {
        Text = "Остановить",
        IsEnabled = false
    };

    private readonly Label _statusLabel = new()
    {
        Text = "Пульт остановлен.",
        FontAttributes = FontAttributes.Bold
    };

    private readonly Label _legendLabel = new()
    {
        Text = "Зелёный — работает, красный — авария, серый — ремонт."
    };

    private readonly Grid _plantGrid = new()
    {
        ColumnSpacing = 8,
        RowSpacing = 8
    };

    private readonly Editor _logEditor = new()
    {
        IsReadOnly = true,
        AutoSize = EditorAutoSizeOption.Disabled,
        HeightRequest = 130
    };

    private readonly Dictionary<int, Button> _plantButtons = [];

    private PlantDispatcherClient? _dispatcher;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainPage()
    {
        Title = "Пульт состояния установок";
        BuildLayout();
        BindEvents();
    }

    private void BuildLayout()
    {
        Grid root = new()
        {
            Padding = 14,
            RowSpacing = 12,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        Label titleLabel = new()
        {
            Text = "Пульт состояния технологических установок",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold
        };

        Grid connectionPanel = new()
        {
            ColumnSpacing = 8,
            RowSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        connectionPanel.Add(new Label { Text = "IP-адрес", VerticalTextAlignment = TextAlignment.Center }, 0, 0);
        connectionPanel.Add(_hostEntry, 1, 0);
        connectionPanel.Add(new Label { Text = "Порт", VerticalTextAlignment = TextAlignment.Center }, 2, 0);
        connectionPanel.Add(_portEntry, 3, 0);
        connectionPanel.Add(_startButton, 4, 0);
        connectionPanel.Add(_stopButton, 5, 0);
        connectionPanel.Add(_statusLabel, 0, 1);
        Grid.SetColumnSpan(_statusLabel, 6);

        Border gridCard = new()
        {
            Padding = 12,
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Colors.LightGray),
            BackgroundColor = Colors.White,
            Content = new ScrollView
            {
                Content = _plantGrid
            }
        };

        Label logTitleLabel = new()
        {
            Text = "Журнал обмена",
            FontAttributes = FontAttributes.Bold
        };

        VerticalStackLayout logPanel = new()
        {
            Spacing = 6,
            Children =
            {
                logTitleLabel,
                _logEditor
            }
        };

        root.Add(titleLabel, 0, 0);
        root.Add(connectionPanel, 0, 1);
        root.Add(_legendLabel, 0, 2);
        root.Add(gridCard, 0, 3);
        root.Add(logPanel, 0, 4);

        Content = root;
    }

    private void BindEvents()
    {
        _startButton.Clicked += async (_, _) => await StartDispatcherAsync();
        _stopButton.Clicked += (_, _) => StopDispatcher();
    }

    private async Task StartDispatcherAsync()
    {
        if (!int.TryParse(_portEntry.Text, out int port))
        {
            await DisplayAlert("Ошибка", "Порт должен быть числом.", "OK");
            return;
        }

        StopDispatcher();
        ClearPlantGrid();

        SocketOptions options = new()
        {
            Host = string.IsNullOrWhiteSpace(_hostEntry.Text) ? AppConstants.Localhost : _hostEntry.Text.Trim(),
            Port = port
        };

        _dispatcher = new PlantDispatcherClient();
        _cancellationTokenSource = new CancellationTokenSource();

        _startButton.IsEnabled = false;
        _stopButton.IsEnabled = true;
        _statusLabel.Text = $"Пульт ожидает контроллер на {options.Host}:{options.Port}.";
        AppendLog(new UiLogMessage(_statusLabel.Text));

        _ = Task.Run(async () =>
        {
            try
            {
                await _dispatcher.StartAsync(
                    options,
                    OnPlantCountReceived,
                    OnStatesReceived,
                    AppendLog,
                    _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                AppendLog(new UiLogMessage($"Ошибка пульта: {exception.Message}", isError: true));
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _startButton.IsEnabled = true;
                    _stopButton.IsEnabled = false;
                    _statusLabel.Text = "Пульт остановлен.";
                });
            }
        });
    }

    private void StopDispatcher()
    {
        _cancellationTokenSource?.Cancel();
        _dispatcher?.Stop();
        _dispatcher?.Dispose();
        _dispatcher = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        _startButton.IsEnabled = true;
        _stopButton.IsEnabled = false;
        _statusLabel.Text = "Пульт остановлен.";
    }

    private void OnPlantCountReceived(int plantCount)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BuildPlantGrid(plantCount);
            _statusLabel.Text = $"Получено количество установок: {plantCount}.";
            AppendLog(new UiLogMessage(_statusLabel.Text));
        });
    }

    private void OnStatesReceived(IReadOnlyList<PlantStateItem> states)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (PlantStateItem state in states)
            {
                if (_plantButtons.TryGetValue(state.Number, out Button? button))
                    ApplyState(button, state.State);
            }

            _statusLabel.Text = $"Последнее обновление: {DateTime.Now:HH:mm:ss}.";
        });
    }

    private void BuildPlantGrid(int plantCount)
    {
        ClearPlantGrid();

        for (int column = 0; column < ButtonsPerRow; column++)
            _plantGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        int rowCount = Math.Max(1, (int)Math.Ceiling((double)plantCount / ButtonsPerRow));
        for (int row = 0; row < rowCount; row++)
            _plantGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int index = 0; index < plantCount; index++)
        {
            int number = index + 1;
            Button button = new()
            {
                Text = number.ToString(),
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 72,
                CornerRadius = 12,
                TextColor = Colors.White
            };

            ApplyState(button, PlantState.Working);

            int row = index / ButtonsPerRow;
            int column = index % ButtonsPerRow;

            _plantGrid.Add(button, column, row);
            _plantButtons[number] = button;
        }
    }

    private void ClearPlantGrid()
    {
        _plantGrid.Children.Clear();
        _plantGrid.RowDefinitions.Clear();
        _plantGrid.ColumnDefinitions.Clear();
        _plantButtons.Clear();
    }

    private static void ApplyState(Button button, PlantState state)
    {
        button.Text = $"{button.Text.Split('\n')[0]}\n{GetStateText(state)}";
        button.BackgroundColor = GetStateColor(state);
    }

    private static Color GetStateColor(PlantState state)
    {
        return state switch
        {
            PlantState.Working => Colors.ForestGreen,
            PlantState.Alarm => Colors.Firebrick,
            PlantState.Repair => Colors.Gray,
            _ => Colors.DarkGray
        };
    }

    private static string GetStateText(PlantState state)
    {
        return state switch
        {
            PlantState.Working => "работает",
            PlantState.Alarm => "авария",
            PlantState.Repair => "ремонт",
            _ => "нет данных"
        };
    }

    private void AppendLog(UiLogMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            string prefix = message.IsError ? "ОШИБКА" : "INFO";
            _logEditor.Text += $"[{message.CreatedAt:HH:mm:ss}] {prefix}: {message.Text}{Environment.NewLine}";
        });
    }
}
