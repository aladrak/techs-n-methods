using MenuLib;
using MenuItem = MenuLib.MenuItem;

namespace DataDrivenApp;

public partial class InformSystem : Form
{
    public InformSystem(MenuLib.Menu menu)
    {
        InitializeComponent();
        MenuStrip menuStrip = new MenuStrip();
        foreach (MenuItem el in menu.Items)
        {
            //ToolStripMenuItem MnuStripItem = new ToolStripMenuItem(el.Name, null, new EventHandler(Type.GetMethod(el.Method)));
            if (el.Permission == "0")
            {
                if (el.Kind == MenuItemKind.Action)
                {
                    ToolStripMenuItem menuStripItem = new ToolStripMenuItem(el.Name, null, ChildClick);
                    menuStrip.Items.Add(menuStripItem);
                }

                if (el.Kind == MenuItemKind.Submenu)
                {
                    ToolStripMenuItem menuStripItem = new ToolStripMenuItem(el.Name, null);
                    SubMenu(menuStripItem, el);
                    menuStrip.Items.Add(menuStripItem);
                }
            }
            else
            {
                if (el.Permission == "1")
                {
                    ToolStripMenuItem menuStripItem = new ToolStripMenuItem(el.Name, null, ChildClick);
                    menuStrip.Items.Add(menuStripItem);
                }
            }
        }
        this.Controls.Add(menuStrip);
    }

    public void SubMenu(ToolStripMenuItem menuItem, MenuItem parentMenu)
    {
        foreach (MenuItem el in parentMenu.SubItems)
        {
            //ToolStripMenuItem MnuStripItem = new ToolStripMenuItem(el.Name, null, new EventHandler(Type.GetMethod(el.Method)));
            if (el.Permission == "0")
            {
                if (el.Kind == MenuItemKind.Action)
                {
                    ToolStripMenuItem menuStripItem = new ToolStripMenuItem(el.Name, null, ChildClick);
                    menuItem.DropDownItems.Add(menuStripItem);
                }

                if (el.Kind == MenuItemKind.Submenu)
                {
                    ToolStripMenuItem menuStripItem = new ToolStripMenuItem(el.Name, null);
                    SubMenu(menuStripItem, el);
                    menuItem.DropDownItems.Add(menuStripItem);
                }
            }
            else
            {
                if (el.Permission == "1")
                {
                    ToolStripMenuItem menuStripItem = new ToolStripMenuItem(el.Name, null);

                    menuItem.DropDownItems.Add(menuStripItem);
                }
            }
        }
    }

    public void ChildClick(object sender, EventArgs e)
    {
        MessageBox.Show(string.Concat("You have Clicked '", sender.ToString(), "' Menu"), "Menu Items Event",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void FileMenuItemClick(object sender, EventArgs e)
    {
        MessageBox.Show("File menu item clicked");
    }

    private void InformSystem_Load(object sender, EventArgs e)
    {

    }

    private void InformSystem_Load_1(object sender, EventArgs e)
    {

    }
}