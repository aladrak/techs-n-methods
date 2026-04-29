using MenuLib;
using Menu = MenuLib.Menu;

namespace DataDrivenApp;

public partial class AuthForm : Form
{
    public AuthForm(string? pAddress)
    {
        InitializeComponent();

        InputLanguageChanged += (s, e) => currentLanguageInfo.Text =
            "Язык ввода " + e.Culture.DisplayName;
        capsLockStateCheckTimer.Tick += (s, e) => capsLockStateInfo.Text =
            "Клавиша CapsLock " + (IsKeyLocked(Keys.CapsLock) ? "нажата" : "не нажата");
        currentLanguageInfo.Text = "Язык ввода " + InputLanguage.CurrentInputLanguage.Culture.DisplayName;
        capsLockStateInfo.Text = "Клавиша CapsLock " + (IsKeyLocked(Keys.CapsLock) ? "нажата" : "не нажата");
        versionInfo.Text = "Версия " + Application.ProductVersion;
    }

    private void loginButton_Click_1(object sender, EventArgs e)
    {
        Auth permissions = new Auth(userNameInput.Text, passwordInput.Text);

        if (permissions == null)
        {
            Console.WriteLine("No such user");
        }
        else 
        {
            Menu menu = new Menu(permissions.userPermissions);
            new InformSystem(menu).ShowDialog();
        }
    }

    private void userNameInput_TextChanged(object sender, EventArgs e)
    {

    }

    private void passwordInput_TextChanged(object sender, EventArgs e)
    {

    }

    private void AuthForm_Load(object sender, EventArgs e)
    {

    }

    private void AuthForm_Load_1(object sender, EventArgs e)
    {

    }

    private void appNameInfo_Click(object sender, EventArgs e)
    {

    }
}