using System.ComponentModel;

namespace DataDrivenApp;

partial class AuthForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuthForm));
        this.userNameHint = new System.Windows.Forms.Label();
        this.userNameInput = new System.Windows.Forms.TextBox();
        this.passwordInput = new System.Windows.Forms.TextBox();
        this.passwordHint = new System.Windows.Forms.Label();
        this.loginButton = new System.Windows.Forms.Button();
        this.cancelButton = new System.Windows.Forms.Button();
        this.appNameInfo = new System.Windows.Forms.Label();
        this.versionInfo = new System.Windows.Forms.Label();
        this.userActionHint = new System.Windows.Forms.Label();
        this.keyboardInfo = new System.Windows.Forms.Panel();
        this.capsLockStateInfo = new System.Windows.Forms.Label();
        this.currentLanguageInfo = new System.Windows.Forms.Label();
        this.keysPicture = new System.Windows.Forms.PictureBox();
        this.capsLockStateCheckTimer = new System.Windows.Forms.Timer(this.components);
        this.keyboardInfo.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.keysPicture)).BeginInit();
        this.SuspendLayout();
        // 
        // userNameHint
        // 
        this.userNameHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.userNameHint.AutoSize = true;
        this.userNameHint.Location = new System.Drawing.Point(12, 119);
        this.userNameHint.Name = "userNameHint";
        this.userNameHint.Size = new System.Drawing.Size(109, 15);
        this.userNameHint.TabIndex = 0;
        this.userNameHint.Text = "Имя пользователя";
        // 
        // userNameInput
        // 
        this.userNameInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.userNameInput.Location = new System.Drawing.Point(144, 116);
        this.userNameInput.Name = "userNameInput";
        this.userNameInput.Size = new System.Drawing.Size(333, 23);
        this.userNameInput.TabIndex = 1;
        // 
        // passwordInput
        // 
        this.passwordInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.passwordInput.Location = new System.Drawing.Point(144, 171);
        this.passwordInput.Name = "passwordInput";
        this.passwordInput.PasswordChar = '*';
        this.passwordInput.Size = new System.Drawing.Size(333, 23);
        this.passwordInput.TabIndex = 3;
        // 
        // passwordHint
        // 
        this.passwordHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.passwordHint.AutoSize = true;
        this.passwordHint.Location = new System.Drawing.Point(12, 174);
        this.passwordHint.Name = "passwordHint";
        this.passwordHint.Size = new System.Drawing.Size(49, 15);
        this.passwordHint.TabIndex = 2;
        this.passwordHint.Text = "Пароль";
        // 
        // loginButton
        // 
        this.loginButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.loginButton.Location = new System.Drawing.Point(31, 243);
        this.loginButton.Name = "loginButton";
        this.loginButton.Size = new System.Drawing.Size(75, 23);
        this.loginButton.TabIndex = 4;
        this.loginButton.Text = "Вход";
        this.loginButton.UseVisualStyleBackColor = true;
        this.loginButton.Click += new System.EventHandler(this.loginButton_Click_1);
        // 
        // cancelButton
        // 
        this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.cancelButton.Location = new System.Drawing.Point(371, 243);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.Size = new System.Drawing.Size(75, 23);
        this.cancelButton.TabIndex = 5;
        this.cancelButton.Text = "Отмена";
        this.cancelButton.UseVisualStyleBackColor = true;
        // 
        // appNameInfo
        // 
        this.appNameInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.appNameInfo.BackColor = System.Drawing.Color.LemonChiffon;
        this.appNameInfo.Location = new System.Drawing.Point(13, 10);
        this.appNameInfo.Name = "appNameInfo";
        this.appNameInfo.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.appNameInfo.Size = new System.Drawing.Size(465, 20);
        this.appNameInfo.TabIndex = 6;
        this.appNameInfo.Text = "АИС Отдел кадров";
        this.appNameInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.appNameInfo.Click += new System.EventHandler(this.appNameInfo_Click);
        // 
        // versionInfo
        // 
        this.versionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.versionInfo.BackColor = System.Drawing.Color.Gold;
        this.versionInfo.Location = new System.Drawing.Point(13, 38);
        this.versionInfo.Name = "versionInfo";
        this.versionInfo.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.versionInfo.Size = new System.Drawing.Size(465, 20);
        this.versionInfo.TabIndex = 7;
        this.versionInfo.Text = "Версия 1.0.0";
        this.versionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // userActionHint
        // 
        this.userActionHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.userActionHint.BackColor = System.Drawing.Color.White;
        this.userActionHint.Location = new System.Drawing.Point(13, 66);
        this.userActionHint.Name = "userActionHint";
        this.userActionHint.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.userActionHint.Size = new System.Drawing.Size(465, 20);
        this.userActionHint.TabIndex = 8;
        this.userActionHint.Text = "Введите имя пользователя и пароль";
        this.userActionHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        // 
        // keyboardInfo
        // 
        this.keyboardInfo.Controls.Add(this.capsLockStateInfo);
        this.keyboardInfo.Controls.Add(this.currentLanguageInfo);
        this.keyboardInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.keyboardInfo.Location = new System.Drawing.Point(0, 279);
        this.keyboardInfo.Name = "keyboardInfo";
        this.keyboardInfo.Size = new System.Drawing.Size(489, 24);
        this.keyboardInfo.TabIndex = 9;
        // 
        // capsLockStateInfo
        // 
        this.capsLockStateInfo.Anchor = System.Windows.Forms.AnchorStyles.Right;
        this.capsLockStateInfo.AutoSize = true;
        this.capsLockStateInfo.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
        this.capsLockStateInfo.Location = new System.Drawing.Point(317, 7);
        this.capsLockStateInfo.Name = "capsLockStateInfo";
        this.capsLockStateInfo.Size = new System.Drawing.Size(187, 15);
        this.capsLockStateInfo.TabIndex = 8;
        this.capsLockStateInfo.Text = "Клавиша CapsLock <состояние>";
        // 
        // currentLanguageInfo
        // 
        this.currentLanguageInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.currentLanguageInfo.AutoSize = true;
        this.currentLanguageInfo.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
        this.currentLanguageInfo.Location = new System.Drawing.Point(5, 7);
        this.currentLanguageInfo.Name = "currentLanguageInfo";
        this.currentLanguageInfo.Size = new System.Drawing.Size(138, 15);
        this.currentLanguageInfo.TabIndex = 7;
        this.currentLanguageInfo.Text = "Язык ввода <значение>";
        // 
        // keysPicture
        // 
        this.keysPicture.Image = Image.FromFile("keys.png");;
        this.keysPicture.Location = new System.Drawing.Point(24, 16);
        this.keysPicture.Name = "keysPicture";
        this.keysPicture.Size = new System.Drawing.Size(64, 64);
        this.keysPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        this.keysPicture.TabIndex = 10;
        this.keysPicture.TabStop = false;
        // 
        // capsLockStateCheckTimer
        // 
        this.capsLockStateCheckTimer.Enabled = true;
        // 
        // AuthForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
        this.ClientSize = new System.Drawing.Size(489, 303);
        this.Controls.Add(this.keysPicture);
        this.Controls.Add(this.keyboardInfo);
        this.Controls.Add(this.userActionHint);
        this.Controls.Add(this.versionInfo);
        this.Controls.Add(this.appNameInfo);
        this.Controls.Add(this.cancelButton);
        this.Controls.Add(this.loginButton);
        this.Controls.Add(this.passwordInput);
        this.Controls.Add(this.passwordHint);
        this.Controls.Add(this.userNameInput);
        this.Controls.Add(this.userNameHint);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "AuthForm";
        this.ShowIcon = false;
        this.Text = "Вход";
        this.Load += new System.EventHandler(this.AuthForm_Load_1);
        this.keyboardInfo.ResumeLayout(false);
        this.keyboardInfo.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.keysPicture)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion
    private Label userNameHint;
	private TextBox userNameInput;
	private TextBox passwordInput;
	private Label passwordHint;
	private Button loginButton;
	private Button cancelButton;
	private Label appNameInfo;
	private Label versionInfo;
	private Label userActionHint;
	private Panel keyboardInfo;
	private Label capsLockStateInfo;
	private Label currentLanguageInfo;
	private PictureBox keysPicture;
	private System.Windows.Forms.Timer capsLockStateCheckTimer;
}
