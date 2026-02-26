namespace BinaryControl;
public class CommandProcessor
{
	private readonly FileManager _fileManager;

	public CommandProcessor(FileManager fileManager)
	{
		_fileManager = fileManager;
	}

	public string Execute(string input)
	{
		var parts = input.Trim().Split([' ', '(', ')', ','], StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0) return "";

		string command = parts[0].ToLower();

		try
		{
			return command switch
			{
				"create" => HandleCreate(parts),
				"open" => HandleOpen(parts),
				"input" => HandleInput(input),
				"delete" => HandleDelete(input),
				"restore" => HandleRestore(parts),
				"truncate" => HandleTruncate(),
				"print" => HandlePrint(parts),
				"help" => HandleHelp(parts),
				"exit" => "exit",
				_ => $"Неизвестная команда: {command}"
			};
		}
		catch (Exception ex)
		{
			return $"Ошибка: {ex.Message}";
		}
	}

	private string HandleCreate(string[] parts)
	{
		if (parts.Length < 2) return "Использование: Create <имя файла> [длина имени] [имя спецификации]";

		string fileName = parts[1];
		int nameLength = Sizes.DefaultNameLength;
		string? specName = null;

		if (parts.Length > 2) int.TryParse(parts[2], out nameLength);
		if (parts.Length > 3) specName = parts[3];

		if (File.Exists(fileName))
		{
			Console.Write("Файл существует. Перезаписать? (y/n): ");
			var response = Console.ReadLine();
			if (response?.ToLower() != "y") return "Отменено";
		}

		_fileManager.CreateDatabase(fileName, specName, (short)nameLength);
		return $"База данных создана: {fileName}";
	}

	private string HandleOpen(string[] parts)
	{
		if (parts.Length < 2) return "Использование: Open <имя файла>";

		_fileManager.OpenDatabase(parts[1]);
		return $"База данных открыта: {parts[1]}";
	}

	private string HandleInput(string input)
	{
		if (!_fileManager.IsOpen) return "Сначала откройте или создайте базу данных";

		// Input (имя, тип) или Input (имя/комплектующее)
		if (input.Contains('/'))
		{
			// Добавление в спецификацию
			var match = System.Text.RegularExpressions.Regex.Match(input, @"\(([^/]+)/([^)]+)\)");
			if (!match.Success) return "Неверный формат. Используйте: Input (имя/комплектующее)";

			string componentName = match.Groups[1].Value.Trim();
			string partName = match.Groups[2].Value.Trim();

			var component = _fileManager.FindProductByName(componentName);
			var part = _fileManager.FindProductByName(partName);

			if (component == null) return $"Компонент '{componentName}' не найден";
			if (part == null) return $"Комплектующее '{partName}' не найдено";

			_fileManager.AddToSpecification(component.FileOffset, part.FileOffset, 1);
			return $"Добавлено '{partName}' в спецификацию '{componentName}'";
		}
		else
		{
			// Добавление компонента
			var match = System.Text.RegularExpressions.Regex.Match(input, @"\(([^,]+),\s*([^)]+)\)");
			if (!match.Success) return "Неверный формат. Используйте: Input (имя, тип)";

			string name = match.Groups[1].Value.Trim();
			string typeStr = match.Groups[2].Value.Trim();

			if (!Enum.TryParse<ComponentType>(typeStr, out var type))
				return $"Неверный тип. Доступны: {string.Join(", ", Enum.GetNames<ComponentType>())}";

			_fileManager.AddProduct(name, type);
			return $"Добавлен компонент '{name}' ({type})";
		}
	}

	private string HandleDelete(string input)
	{
		if (!_fileManager.IsOpen) return "Сначала откройте или создайте базу данных";

		if (input.Contains('/'))
		{
			return "Удаление из спецификации не реализовано в этой версии";
		}
		else
		{
			var match = System.Text.RegularExpressions.Regex.Match(input, @"\(([^)]+)\)");
			if (!match.Success) return "Неверный формат. Используйте: Delete (имя)";

			string name = match.Groups[1].Value.Trim();
			_fileManager.LogicalDeleteProduct(name);
			return $"Компонент '{name}' помечен на удаление";
		}
	}

	private string HandleRestore(string[] parts)
	{
		if (!_fileManager.IsOpen) return "Сначала откройте или создайте базу данных";

		if (parts.Length < 2) return "Использование: Restore (имя) или Restore (*)";

		string name = parts[1].Trim('(', ')');

		if (name == "*")
		{
			_fileManager.RestoreAll();
			return "Все записи восстановлены";
		}
		else
		{
			_fileManager.RestoreProduct(name);
			return $"Компонент '{name}' восстановлен";
		}
	}

	private string HandleTruncate()
	{
		if (!_fileManager.IsOpen) return "Сначала откройте или создайте базу данных";

		_fileManager.Truncate();
		return "Файлы очищены от удаленных записей";
	}

	private string HandlePrint(string[] parts)
	{
		if (!_fileManager.IsOpen) return "Сначала откройте или создайте базу данных";

		if (parts.Length < 2) return "Использование: Print (имя) или Print (*)";

		string name = parts[1].Trim('(', ')');

		if (name == "*")
		{
			_fileManager.PrintAllProducts();
		}
		else
		{
			_fileManager.PrintSpecification(name);
		}

		return "";
	}

	private string HandleHelp(string[] parts)
	{
		const string helpText = """

		                        Доступные команды:
		                          Create <имя> [длина] [спецификация] - Создать базу данных
		                          Open <имя>                          - Открыть базу данных
		                          Input (имя, тип)                    - Добавить компонент (Product/Node/Detail)
		                          Input (имя/комплектующее)           - Добавить в спецификацию
		                          Delete (имя)                        - Логически удалить компонент
		                          Restore (имя) или Restore (*)       - Восстановить удаленные
		                          Truncate                            - Физически удалить помеченные
		                          Print (имя) или Print (*)           - Вывести спецификацию или список
		                          Help                                - Показать эту справку
		                          Exit                                - Выход

		                        """;
		if (parts.Length <= 1) return helpText;
		File.WriteAllText(parts[1], helpText);
		return $"Справка сохранена в {parts[1]}";

	}
}