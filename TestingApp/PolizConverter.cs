namespace TestingApp;

public sealed class PolizConverter
{
    public IReadOnlyList<Token> ConvertToPoliz(IReadOnlyList<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new Exception("Выражение не должно быть пустым.");
        }

        List<Token> output = new();
        Stack<Token> stack = new();
        Token? previous = null;

        foreach (Token token in tokens)
        {
            switch (token.Type)
            {
                case TokenType.Number:
                    ProcessNumber(token, previous, output);
                    break;

                case TokenType.Operator:
                    ProcessOperator(token, previous, output, stack);
                    break;

                case TokenType.LeftParenthesis:
                    ProcessLeftParenthesis(token, previous, stack);
                    break;

                case TokenType.RightParenthesis:
                    ProcessRightParenthesis(previous, output, stack);
                    break;

                default:
                    throw new Exception("Неизвестный тип лексемы.");
            }

            previous = token;
        }

        ValidateEndOfExpression(previous);
        MoveRemainingOperators(output, stack);

        return output;
    }

    public string FormatPoliz(IReadOnlyList<Token> tokens)
    {
        return string.Join(' ', tokens.Select(token => token.Value));
    }

    private static void ProcessNumber(Token token, Token? previous, List<Token> output)
    {
        if (previous is not null && (previous.Type == TokenType.Number || previous.Type == TokenType.RightParenthesis))
        {
            throw new Exception("Между двумя операндами отсутствует операция.");
        }

        output.Add(token);
    }

    private static void ProcessOperator(Token token, Token? previous, List<Token> output, Stack<Token> stack)
    {
        OperatorInfo currentOperator = Operators.Get(token.Value);

        if (currentOperator.OperandsCount == 2 && IsBinaryOperatorInvalid(previous))
        {
            throw new Exception($"Бинарная операция '{token.Value}' записана без левого операнда.");
        }

        while (stack.Count > 0 && stack.Peek().Type == TokenType.Operator)
        {
            OperatorInfo stackOperator = Operators.Get(stack.Peek().Value);
            bool shouldMoveFromStack = currentOperator.IsRightAssociative
                ? currentOperator.Priority < stackOperator.Priority
                : currentOperator.Priority <= stackOperator.Priority;

            if (!shouldMoveFromStack)
            {
                break;
            }

            output.Add(stack.Pop());
        }

        stack.Push(token);
    }

    private static void ProcessLeftParenthesis(Token token, Token? previous, Stack<Token> stack)
    {
        if (previous is not null && (previous.Type == TokenType.Number || previous.Type == TokenType.RightParenthesis))
        {
            throw new Exception("Перед открывающей скобкой отсутствует операция.");
        }

        stack.Push(token);
    }

    private static void ProcessRightParenthesis(Token? previous, List<Token> output, Stack<Token> stack)
    {
        if (previous is null || previous.Type == TokenType.Operator || previous.Type == TokenType.LeftParenthesis)
        {
            throw new Exception("Закрывающая скобка записана в недопустимом месте.");
        }

        bool hasLeftParenthesis = false;

        while (stack.Count > 0)
        {
            Token top = stack.Pop();

            if (top.Type == TokenType.LeftParenthesis)
            {
                hasLeftParenthesis = true;
                break;
            }

            output.Add(top);
        }

        if (!hasLeftParenthesis)
        {
            throw new Exception("Не найдена соответствующая открывающая скобка.");
        }
    }

    private static bool IsBinaryOperatorInvalid(Token? previous)
    {
        return previous is null
            || previous.Type == TokenType.Operator
            || previous.Type == TokenType.LeftParenthesis;
    }

    private static void ValidateEndOfExpression(Token? previous)
    {
        if (previous is null)
        {
            throw new Exception("Выражение не должно быть пустым.");
        }

        if (previous.Type == TokenType.Operator || previous.Type == TokenType.LeftParenthesis)
        {
            throw new Exception("Выражение не может заканчиваться операцией или открывающей скобкой.");
        }
    }

    private static void MoveRemainingOperators(List<Token> output, Stack<Token> stack)
    {
        while (stack.Count > 0)
        {
            Token top = stack.Pop();

            if (top.Type == TokenType.LeftParenthesis || top.Type == TokenType.RightParenthesis)
            {
                throw new Exception("Нарушен баланс скобок.");
            }

            output.Add(top);
        }
    }
}
