using AppInterfaces;

namespace MenuLib;

public class Menu : IMenu
{
    public List<IMenuItem> Items { get; } = [];
    private List<MenuItem> _permittedItems = [];

    public void SetMenu(string permPath)
    {
        try
        {
            using var file = new StreamReader(permPath);
            while (!file.EndOfStream)
            {
                var line = file.ReadLine()!;
                var curItem = MenuItem.Parse(line);
                _permittedItems.Add(curItem);
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}"); }
        
        try
        {
            using var file = new StreamReader("menu.txt");
            while (!file.EndOfStream)
            {
                var line = file.ReadLine()!;
                var curItem = MenuItem.Parse(line);
                foreach (var permItem in _permittedItems)
                {
                    if (permItem.Name == curItem.Name)
                    {
                        curItem.Permission = permItem.Permission;
                    }
                }

                if (curItem.Level == 0)
                    Items.Add(curItem);
                else
                {
                    var parentItem = (MenuItem)Items[Items.Count - 1];
                    while (parentItem.Level + 1 != curItem.Level)
                    {
                        parentItem = parentItem.SubItems?[^1];
                    }
                    parentItem.SubItems?.Add(curItem);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
        }
    }
}