namespace TestingApp;

public static class Program
{
    private static readonly Tokenizer s_tokenizer = new();
    private static readonly PolizConverter s_converter = new();
    private static readonly PolizCalculator s_calculator = new();
    
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        try
        {
            if (args.Length > 0)
            {
                ProcessCommandLine(args);
                return;
            }

            RunInteractiveMode();
        }
        catch (Exception exception)
        {
            PrintError(exception.Message);
        }
    }

    private static void ProcessCommandLine(string[] args)
    {
        string command = args[0].Trim().ToLowerInvariant();
        string value = string.Join(' ', args.Skip(1));

        switch (command)
        {
            case "--expr":
            case "-e":
                ConvertAndCalculateExpression(value);
                break;

            case "--poliz":
            case "-p":
                CalculatePoliz(value);
                break;

            case "--help":
            case "-h":
                PrintHelp();
                break;

            default:
                PrintError("Неизвестный параметр командной строки.");
                PrintHelp();
                break;
        }
    }

    private static void RunInteractiveMode()
    {
        PrintHeader();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1 - Перевести инфиксное выражение в ПОЛИЗ и вычислить его");
            Console.WriteLine("2 - Вычислить выражение, уже записанное в ПОЛИЗ");
            Console.WriteLine("0 - Выход");
            Console.Write("Выберите действие: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Write("Введите инфиксное выражение: ");
                        ConvertAndCalculateExpression(Console.ReadLine() ?? string.Empty);
                        break;

                    case "2":
                        Console.Write("Введите ПОЛИЗ через пробелы: ");
                        CalculatePoliz(Console.ReadLine() ?? string.Empty);
                        break;

                    case "0":
                    case "exit":
                    case "Exit":
                        return;

                    default:
                        PrintError("Выбран неизвестный пункт меню.");
                        break;
                }
            }
            catch (Exception exception)
            {
                PrintError(exception.Message);
            }
        }
    }

    private static void ConvertAndCalculateExpression(string expression)
    {
        IReadOnlyList<Token> tokens = s_tokenizer.Tokenize(expression);
        IReadOnlyList<Token> polizTokens = s_converter.ConvertToPoliz(tokens);
        long result = s_calculator.Calculate(polizTokens);

        Console.WriteLine($"Исходное выражение: {expression}");
        Console.WriteLine($"ПОЛИЗ: {s_converter.FormatPoliz(polizTokens)}");
        Console.WriteLine($"Результат: {result}");
    }

    private static void CalculatePoliz(string poliz)
    {
        IReadOnlyList<Token> tokens = s_calculator.ParsePoliz(poliz);
        long result = s_calculator.Calculate(tokens);

        Console.WriteLine($"ПОЛИЗ: {poliz}");
        Console.WriteLine($"Результат: {result}");
    }

    private static void PrintHeader()
    {
        Console.WriteLine("Лабораторная работа: Тестирование программных продуктов");
        Console.WriteLine("Программа перевода выражения в ПОЛИЗ и вычисления ПОЛИЗ");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Использование:");
        Console.WriteLine("  TestingApp --expr \"3 + 4 * (5 - 1)\"");
        Console.WriteLine("  TestingApp --poliz \"3 4 5 1 - * +\"");
        Console.WriteLine();
        Console.WriteLine("Допустимые операции: +, -, *, /, :, **, унарные + и -.");
        Console.WriteLine("В ПОЛИЗ унарные операции обозначаются как u+ и u-.");
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Ошибка: {message}");
        Console.ResetColor();
    }
}
