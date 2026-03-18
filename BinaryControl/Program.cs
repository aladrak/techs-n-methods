namespace BinaryControl;

class Program
{
	private static void Main()
	{
		var fileManager = new FileManager();
		var processor = new CommandProcessor(fileManager);

		Console.WriteLine("Система управления спецификациями");
		Console.WriteLine("Введите Help для списка команд\n");

		while (true)
		{
			Console.Write("PS> ");
			var input = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(input))
				continue;

			var result = processor.Execute(input);

			if (!string.IsNullOrEmpty(result))
				Console.WriteLine(result);

			if (input.Trim().Equals("exit", StringComparison.CurrentCultureIgnoreCase))
				break;
		}

		fileManager.Dispose();
		Console.WriteLine("Программа завершена");
	}
}