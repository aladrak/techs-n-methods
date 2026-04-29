namespace MenuLib
{
    public class Menu
    {
        public List<MenuItem> Items { get; private set; } = new();
        public List<MenuItem> PermitedItems { get; private set; } = new();

        public Menu(string permissionsPath, string filePath = "menu.txt")
        {
            try
            {
                using var file = new StreamReader(permissionsPath);
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine()!;
                    MenuItem curItem = MenuItem.Parse(line);
                    PermitedItems.Add(curItem);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
            }
            
            try
            {
                using var file = new StreamReader(filePath);
                while (!file.EndOfStream)
                {
                    string line = file.ReadLine()!;
                    MenuItem curItem = MenuItem.Parse(line);
                    foreach (MenuItem permItem in PermitedItems)
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
                        MenuItem parentItem = Items[Items.Count - 1];
                        while (parentItem.Level+1 != curItem.Level)
                        {
                            parentItem = parentItem.SubItems[parentItem.SubItems.Count - 1];
                        }
                        parentItem.SubItems.Add(curItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
            }
        }
    }
}