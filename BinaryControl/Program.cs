using SpecificationApp;

namespace SpecificationApp;
class Program
{
	static void Main(string[] args)
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

			if (input.Trim().ToLower() == "exit")
				break;
		}

		fileManager.Dispose();
		Console.WriteLine("Программа завершена");
	}
}