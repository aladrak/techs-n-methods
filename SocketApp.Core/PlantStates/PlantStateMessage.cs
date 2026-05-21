namespace SocketApp.Core.PlantStates;

public enum PlantMessageType
{
    PlantCount,
    Ack,
    States,
    Disconnect
}

public sealed class PlantStateItem
{
    public int Number { get; set; }

    public PlantState State { get; set; }
}

public sealed class PlantStateMessage
{
    public PlantMessageType Type { get; set; }

    public int PlantCount { get; set; }

    public List<PlantStateItem> States { get; set; } = [];

    public static PlantStateMessage Count(int plantCount)
    {
        return new PlantStateMessage
        {
            Type = PlantMessageType.PlantCount,
            PlantCount = plantCount
        };
    }

    public static PlantStateMessage Ack()
    {
        return new PlantStateMessage
        {
            Type = PlantMessageType.Ack
        };
    }

    public static PlantStateMessage FromStates(IEnumerable<PlantStateItem> states)
    {
        return new PlantStateMessage
        {
            Type = PlantMessageType.States,
            States = states.ToList()
        };
    }
}
