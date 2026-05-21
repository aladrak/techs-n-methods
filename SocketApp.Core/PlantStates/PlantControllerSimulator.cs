using SocketApp.Core.Common;
using SocketApp.Core.Networking;

namespace SocketApp.Core.PlantStates;

public sealed class PlantControllerSimulator
{
    private const double AlarmProbability = 0.2;
    private const double RepairRecoveryProbability = 0.5;

    private readonly Random _random = new();
    private readonly IMessageSerializer _serializer;
    private PlantState[] _currentStates = [];

    public PlantControllerSimulator(IMessageSerializer? serializer = null)
    {
        _serializer = serializer ?? new JsonMessageSerializer();
    }

    public void Initialize(int plantCount)
    {
        if (plantCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(plantCount), "Количество установок должно быть положительным.");

        _currentStates = Enumerable.Repeat(PlantState.Working, plantCount).ToArray();
    }

    public IReadOnlyList<PlantStateItem> GenerateNextStates()
    {
        if (_currentStates.Length == 0)
            throw new InvalidOperationException("Симулятор установок не инициализирован.");

        List<PlantStateItem> generatedStates = [];

        for (int index = 0; index < _currentStates.Length; index++)
        {
            PlantState stateForMessage = GenerateStateForMessage(index);

            generatedStates.Add(new PlantStateItem
            {
                Number = index + 1,
                State = stateForMessage
            });
        }

        return generatedStates;
    }

    public async Task RunAsync(
        PlantControllerConfig config,
        Action<UiLogMessage>? log = null,
        CancellationToken cancellationToken = default)
    {
        Initialize(config.PlantCount);

        await using TcpMessageClient client = new(_serializer);
        await client.ConnectAsync(config.Host, config.Port, cancellationToken);
        log?.Invoke(new UiLogMessage($"Контроллер установок подключён к пульту: {config.Host}:{config.Port}."));

        await client.SendAsync(PlantStateMessage.Count(config.PlantCount), cancellationToken);
        PlantStateMessage response = await client.ReceiveAsync<PlantStateMessage>(cancellationToken);

        if (response.Type != PlantMessageType.Ack)
            throw new HttpRequestException("Пульт диспетчера не подтвердил получение количества установок.");

        while (!cancellationToken.IsCancellationRequested)
        {
            IReadOnlyList<PlantStateItem> states = GenerateNextStates();
            await client.SendAsync(PlantStateMessage.FromStates(states), cancellationToken);
            log?.Invoke(new UiLogMessage("Переданы состояния установок."));
            await Task.Delay(config.IntervalMs, cancellationToken);
        }
    }

    private PlantState GenerateStateForMessage(int index)
    {
        PlantState currentState = _currentStates[index];

        if (currentState == PlantState.Repair)
        {
            bool restored = _random.NextDouble() < RepairRecoveryProbability;
            _currentStates[index] = restored ? PlantState.Working : PlantState.Repair;
            return _currentStates[index];
        }

        bool alarm = _random.NextDouble() < AlarmProbability;
        if (alarm)
        {
            _currentStates[index] = PlantState.Repair;
            return PlantState.Alarm;
        }

        _currentStates[index] = PlantState.Working;
        return PlantState.Working;
    }
}
