using MenuLib;
using Menu = MenuLib.Menu;

namespace DataDrivenApp;

public partial class AuthForm : Form
{
    public AuthForm()
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
        if (string.IsNullOrEmpty(userNameInput.Text) || string.IsNullOrEmpty(passwordInput.Text))
        {
            MessageBox.Show(string.Concat("Есть хотя бы одно пустое поле!"), "Заполните все поля!",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var permissions = new Auth(userNameInput.Text, passwordInput.Text);
        if (string.IsNullOrEmpty(permissions.UserPermissions))
        {
            MessageBox.Show(string.Concat("Юзер не существует"), "Такого юзера нет!",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var menu = new Menu(permissions.UserPermissions);
        new InformSystem(menu).ShowDialog();
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