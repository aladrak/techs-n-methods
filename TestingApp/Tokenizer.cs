using System.Text;

namespace TestingApp;

public sealed class Tokenizer
{
    public IReadOnlyList<Token> Tokenize(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new Exception("Выражение не должно быть пустым.");
        }

        List<Token> tokens = new();
        int position = 0;

        while (position < expression.Length)
        {
            char current = expression[position];

            if (char.IsWhiteSpace(current))
            {
                position++;
                continue;
            }

            if (char.IsDigit(current))
            {
                tokens.Add(ReadNumber(expression, ref position));
                continue;
            }

            if (current == '(')
            {
                tokens.Add(new Token(TokenType.LeftParenthesis, current.ToString()));
                position++;
                continue;
            }

            if (current == ')')
            {
                tokens.Add(new Token(TokenType.RightParenthesis, current.ToString()));
                position++;
                continue;
            }

            if (IsSingleCharacterOperator(current))
            {
                tokens.Add(ReadOperator(expression, ref position));
                continue;
            }

            throw new Exception($"Недопустимый символ '{current}' в позиции {position + 1}.");
        }

        return MarkUnaryOperators(tokens);
    }

    private static Token ReadNumber(string expression, ref int position)
    {
        StringBuilder number = new();

        while (position < expression.Length && char.IsDigit(expression[position]))
        {
            number.Append(expression[position]);
            position++;
        }

        if (!long.TryParse(number.ToString(), out _))
        {
            throw new Exception($"Слишком большое целое число: {number}.");
        }

        return new Token(TokenType.Number, number.ToString());
    }

    private static Token ReadOperator(string expression, ref int position)
    {
        char current = expression[position];

        if (current == '*' && position + 1 < expression.Length && expression[position + 1] == '*')
        {
            position += 2;
            return new Token(TokenType.Operator, "**");
        }

        position++;
        return new Token(TokenType.Operator, current.ToString());
    }

    private static bool IsSingleCharacterOperator(char value)
    {
        return value is '+' or '-' or '*' or '/' or ':';
    }

    private static IReadOnlyList<Token> MarkUnaryOperators(IReadOnlyList<Token> tokens)
    {
        List<Token> result = new(tokens.Count);
        Token? previous = null;

        foreach (Token token in tokens)
        {
            if (token.Type == TokenType.Operator && (token.Value == "+" || token.Value == "-") && IsUnaryPosition(previous))
            {
                result.Add(new Token(TokenType.Operator, "u" + token.Value));
            }
            else
            {
                result.Add(token);
            }

            previous = result[^1];
        }

        return result;
    }

    private static bool IsUnaryPosition(Token? previous)
    {
        return previous is null
            || previous.Type == TokenType.Operator
            || previous.Type == TokenType.LeftParenthesis;
    }
}
