namespace TestingApp;

public sealed class OperatorInfo
{
    public OperatorInfo(string symbol, int priority, bool isRightAssociative, int operandsCount)
    {
        Symbol = symbol;
        Priority = priority;
        IsRightAssociative = isRightAssociative;
        OperandsCount = operandsCount;
    }
    
    public string Symbol { get; }
    
    public int Priority { get; }
    
    public bool IsRightAssociative { get; }
    
    public int OperandsCount { get; }
}
