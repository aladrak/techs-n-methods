using AppInterfaces;

namespace DynamicDataApp;

public partial class InformSystem : Form
{
    private IMenu Menu;
    public InformSystem(IMenu menu)
    {
        InitializeComponent();
        Menu = menu;
        var menuStrip = new MenuStrip();
        foreach (var el in menu.Items)
        {
            switch (el.Permission)
            {
                case "0":
                {
                    if (el.Kind == MenuItemKind.Action)
                    {
                        var menuStripItem = new ToolStripMenuItem(el.Name, null, ChildClick);
                        menuStrip.Items.Add(menuStripItem);
                    }

                    if (el.Kind == MenuItemKind.Submenu)
                    {
                        var menuStripItem = new ToolStripMenuItem(el.Name, null);
                        SubMenu(menuStripItem, el);
                        menuStrip.Items.Add(menuStripItem);
                    }

                    break;
                }
                case "1":
                {
                    var menuStripItem = new ToolStripMenuItem(el.Name, null, ChildClick);
                    menuStrip.Items.Add(menuStripItem);
                    break;
                }
            }
        }

        Controls.Add(menuStrip);
    }

    private void SubMenu(ToolStripMenuItem menuItem, IMenuItem parentMenu)
    {
        if (parentMenu.Value is List<IMenuItem> list)
        {
            foreach (var el in list)
            {
                switch (el.Permission)
                {
                    case "0":
                    {
                        if (el.Kind == MenuItemKind.Action)
                        {
                            var menuStripItem = new ToolStripMenuItem(el.Name, null, ChildClick);
                            menuItem.DropDownItems.Add(menuStripItem);
                        }

                        if (el.Kind == MenuItemKind.Submenu)
                        {
                            var menuStripItem = new ToolStripMenuItem(el.Name, null);
                            SubMenu(menuStripItem, el);
                            menuItem.DropDownItems.Add(menuStripItem);
                        }

                        break;
                    }
                    case "1":
                    {
                        var menuStripItem = new ToolStripMenuItem(el.Name, null);

                        menuItem.DropDownItems.Add(menuStripItem);
                        break;
                    }
                }
            }
        }
    }

    public void ChildClick(object sender, EventArgs e)
    {
        MessageBox.Show(string.Concat("Вы кликнули на '", sender.ToString(), "'"), "Окно событий",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void InformSystem_Load_1(object sender, EventArgs e) {}
}