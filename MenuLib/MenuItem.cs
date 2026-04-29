namespace MenuLib
{
    public enum MenuItemKind
    {
        Action,
        Submenu
    }

    public class MenuItem
    {
        public int Level { get; }
        public MenuItemKind Kind { get; }
        public string Name { get; }
        public string Permission { get; set; } = "0";
        public object? Value { get; }
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
            var level = int.Parse(data[0]);
            var name = data[1];
            var permission = data[2];
            var method = data.ElementAtOrDefault(3);

            return new MenuItem(level, name, permission, method);
        }
    }
}