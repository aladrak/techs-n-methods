using SocketApp.Core.Common;
using SocketApp.Core.ProcessParameters;
using SocketApp.ProcessDispatcher.Maui.Controls;

namespace SocketApp.ProcessDispatcher.Maui;

public sealed class MainPage : ContentPage
{
    private const int MaxVisibleSamples = 60;

    private readonly Entry _hostEntry = new()
    {
        Text = AppConstants.Localhost,
        WidthRequest = 150
    };

    private readonly Entry _portEntry = new()
    {
        Text = AppConstants.ProcessPort.ToString(),
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

    private readonly Label _temperatureLabel = new()
    {
        Text = "Температура: — °C",
        FontSize = 18
    };

    private readonly Label _pressureLabel = new()
    {
        Text = "Давление: — атм",
        FontSize = 18
    };

    private readonly Editor _logEditor = new()
    {
        IsReadOnly = true,
        AutoSize = EditorAutoSizeOption.Disabled,
        HeightRequest = 120
    };

    private readonly SimpleLineChart _temperatureChart = new(
        "Температура от времени, °C",
        ProcessLimits.MinTemperature,
        ProcessLimits.MaxTemperature,
        Colors.OrangeRed);

    private readonly SimpleLineChart _pressureChart = new(
        "Давление от времени, атм",
        ProcessLimits.MinPressure,
        ProcessLimits.MaxPressure,
        Colors.SteelBlue);

    private readonly List<ProcessSample> _samples = [];

    private ProcessDispatcherClient? _dispatcher;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainPage()
    {
        Title = "Пульт диспетчера процесса";
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
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        Label titleLabel = new()
        {
            Text = "Задание 2. Пульт диспетчера: температура и давление",
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

        Grid dataPanel = new()
        {
            ColumnSpacing = 12,
            RowSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        Border temperatureCard = CreateCard(new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                _temperatureLabel,
                _temperatureChart
            }
        });

        Border pressureCard = CreateCard(new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                _pressureLabel,
                _pressureChart
            }
        });

        dataPanel.Add(temperatureCard, 0, 0);
        dataPanel.Add(pressureCard, 1, 0);

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
        root.Add(dataPanel, 0, 2);
        root.Add(logPanel, 0, 3);

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

        SocketOptions options = new()
        {
            Host = string.IsNullOrWhiteSpace(_hostEntry.Text) ? AppConstants.Localhost : _hostEntry.Text.Trim(),
            Port = port
        };

        _dispatcher = new ProcessDispatcherClient();
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
                    OnSampleReceived,
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

    private void OnSampleReceived(ProcessSample sample)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _samples.Add(sample);

            while (_samples.Count > MaxVisibleSamples)
                _samples.RemoveAt(0);

            _temperatureLabel.Text = $"Температура: {sample.Temperature:F1} °C";
            _pressureLabel.Text = $"Давление: {sample.Pressure:F2} атм";
            _statusLabel.Text = $"Последнее измерение: {sample.Time:HH:mm:ss}.";

            _temperatureChart.SetValues(_samples.Select(current => current.Temperature).ToArray());
            _pressureChart.SetValues(_samples.Select(current => current.Pressure).ToArray());
        });
    }

    private void AppendLog(UiLogMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            string prefix = message.IsError ? "ОШИБКА" : "INFO";
            _logEditor.Text += $"[{message.CreatedAt:HH:mm:ss}] {prefix}: {message.Text}{Environment.NewLine}";
        });
    }

    private static Border CreateCard(View content)
    {
        return new Border
        {
            Padding = 12,
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Colors.LightGray),
            BackgroundColor = Colors.White,
            Content = content
        };
    }
}
