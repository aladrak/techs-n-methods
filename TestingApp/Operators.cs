namespace TestingApp;

public static class Operators
{
    private static readonly Dictionary<string, OperatorInfo> s_operators = new()
    {
        ["+"] = new OperatorInfo("+", 2, false, 2),
        ["-"] = new OperatorInfo("-", 2, false, 2),
        ["*"] = new OperatorInfo("*", 3, false, 2),
        ["/"] = new OperatorInfo("/", 3, false, 2),
        [":"] = new OperatorInfo(":", 3, false, 2),
        ["**"] = new OperatorInfo("**", 4, true, 2),
        ["u+"] = new OperatorInfo("u+", 5, true, 1),
        ["u-"] = new OperatorInfo("u-", 5, true, 1)
    };

    public static bool IsOperator(string symbol) => s_operators.ContainsKey(symbol);

    public static OperatorInfo Get(string symbol)
    {
        if (!s_operators.TryGetValue(symbol, out OperatorInfo? info))
        {
            throw new Exception($"Недопустимая операция: {symbol}.");
        }

        return info;
    }
}
