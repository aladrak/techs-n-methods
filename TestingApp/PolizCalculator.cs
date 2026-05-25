namespace TestingApp;

public sealed class PolizCalculator
{
    public long Calculate(IReadOnlyList<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new Exception("ПОЛИЗ не должна быть пустой.");
        }

        Stack<long> stack = new();

        foreach (Token token in tokens)
        {
            if (token.Type == TokenType.Number)
            {
                stack.Push(ParseNumber(token.Value));
                continue;
            }

            if (token.Type != TokenType.Operator)
            {
                throw new Exception("ПОЛИЗ не должна содержать скобки.");
            }

            OperatorInfo operatorInfo = Operators.Get(token.Value);
            ExecuteOperator(operatorInfo, stack);
        }

        if (stack.Count != 1)
        {
            throw new Exception("Структура ПОЛИЗ неверна: после вычисления осталось несколько значений.");
        }

        return stack.Pop();
    }

    public IReadOnlyList<Token> ParsePoliz(string poliz)
    {
        if (string.IsNullOrWhiteSpace(poliz))
        {
            throw new Exception("ПОЛИЗ не должна быть пустой.");
        }

        string[] parts = poliz.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        List<Token> tokens = new(parts.Length);

        foreach (string part in parts)
        {
            if (long.TryParse(part, out _))
            {
                tokens.Add(new Token(TokenType.Number, part));
                continue;
            }

            if (Operators.IsOperator(part))
            {
                tokens.Add(new Token(TokenType.Operator, part));
                continue;
            }

            throw new Exception($"Недопустимая лексема ПОЛИЗ: {part}.");
        }

        return tokens;
    }

    private static void ExecuteOperator(OperatorInfo operatorInfo, Stack<long> stack)
    {
        if (stack.Count < operatorInfo.OperandsCount)
        {
            throw new Exception($"Недостаточно операндов для операции '{operatorInfo.Symbol}'.");
        }

        if (operatorInfo.OperandsCount == 1)
        {
            long value = stack.Pop();
            stack.Push(ExecuteUnaryOperator(operatorInfo.Symbol, value));
            return;
        }

        long right = stack.Pop();
        long left = stack.Pop();
        stack.Push(ExecuteBinaryOperator(operatorInfo.Symbol, left, right));
    }

    private static long ParseNumber(string value)
    {
        if (!long.TryParse(value, out long result))
        {
            throw new Exception($"Слишком большое целое число: {value}.");
        }

        return result;
    }

    private static long ExecuteUnaryOperator(string operation, long value)
    {
        return operation switch
        {
            "u+" => value,
            "u-" => checked(-value),
            _ => throw new Exception($"Неизвестная унарная операция: {operation}.")
        };
    }

    private static long ExecuteBinaryOperator(string operation, long left, long right)
    {
        return operation switch
        {
            "+" => checked(left + right),
            "-" => checked(left - right),
            "*" => checked(left * right),
            "/" => Divide(left, right),
            ":" => Divide(left, right),
            "**" => Power(left, right),
            _ => throw new Exception($"Неизвестная бинарная операция: {operation}.")
        };
    }

    private static long Divide(long left, long right)
    {
        if (right == 0)
        {
            throw new Exception("Деление на ноль запрещено.");
        }

        return left / right;
    }

    private static long Power(long value, long degree)
    {
        if (degree < 0)
        {
            throw new Exception("Степень должна быть неотрицательным целым числом.");
        }

        long result = 1;
        checked
        {
            for (long i = 0; i < degree; i++)
            {
                result *= value;
            }
        }

        return result;
    }
}
