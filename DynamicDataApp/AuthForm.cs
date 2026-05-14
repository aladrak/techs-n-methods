using System.Reflection;
using AppInterfaces;

namespace DynamicDataApp;

public partial class AuthForm : Form
{
    private IAuth Perm { get; set; }
    public AuthForm(IAuth perm)
    {
        InitializeComponent();
        Perm = perm;
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
        Perm.TryAuth(userNameInput.Text, passwordInput.Text);
        if (string.IsNullOrEmpty(Perm.UserPermissions))
        {
            MessageBox.Show(string.Concat("Юзер не существует"), "Такого юзера нет!",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        IMenu? plugin = null;
        try
        {
            Assembly asm = Assembly.LoadFrom("./menulib.dll");
            Type? pluginType = asm.GetExportedTypes()
                .FirstOrDefault(t => t.Name == "Menu");
            if (pluginType == null)
            {
                throw new Exception("Подобный тип не найден.");
            }

            plugin = (IMenu)Activator.CreateInstance(pluginType);
        }
        catch (Exception _)
        {
            Console.Error.WriteLine(e);
            return;
        }
        plugin.SetMenu(Perm.UserPermissions);
        new InformSystem(plugin).ShowDialog();
    }

    private void AuthForm_Load_1(object sender, EventArgs e)
    {

    }

    private void appNameInfo_Click(object sender, EventArgs e)
    {

    }
}