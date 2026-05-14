namespace AppInterfaces;

public enum MenuItemKind
{
    Action,
    Submenu
}

public interface IMenu
{
    public List<IMenuItem> Items { get; }
    public void SetMenu(string a);
}

public interface IMenuItem
{
    public MenuItemKind Kind { get; }
    public string Name { get; }
    public string Permission { get; }
    public object? Value { get; }
}