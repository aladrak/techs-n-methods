namespace MenuLib
{
    public enum MenuItemKind
    {
        Action,
        Submenu
    }

    public class MenuItem
    {
        public int Level { get; init; }
        public MenuItemKind Kind { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Permission { get; set; } = "0";
        public object? Value { get; init; }
        public string? Method => Value as string;
        public List<MenuItem>? SubItems => Value as List<MenuItem>;

        public MenuItem(int level, string name, string permission, string? method = null)
        {
            Level = level;
            Name = name;
            Permission = permission;
            Kind = method != null ? MenuItemKind.Action : MenuItemKind.Submenu;

            if (Kind == MenuItemKind.Action)
                Value = method;
            else
                Value = new List<MenuItem>();
        }

        public static MenuItem Parse(string value)
        {
            var data = value.Split(' ');
            int level = int.Parse(data[0]);
            string name = data[1];
            string permission = data[2];
            string? method = data.ElementAtOrDefault(3);

            return new MenuItem(level, name, permission, method);
        }
    }
}