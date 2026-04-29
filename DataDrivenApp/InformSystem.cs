using MenuLib;
using MenuItem = MenuLib.MenuItem;

namespace DataDrivenApp;

public partial class InformSystem : Form
{
    public InformSystem(MenuLib.Menu menu)
    {
        InitializeComponent();
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

        this.Controls.Add(menuStrip);
    }

    public void SubMenu(ToolStripMenuItem menuItem, MenuItem parentMenu)
    {
        foreach (var el in parentMenu.SubItems)
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

    public void ChildClick(object sender, EventArgs e)
    {
        MessageBox.Show(string.Concat("You have Clicked '", sender.ToString(), "' Menu"), "Menu Items Event",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void InformSystem_Load_1(object sender, EventArgs e) {}
}