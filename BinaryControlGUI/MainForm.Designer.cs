using BinaryControl;
namespace BinaryControlGUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem componentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem specificationToolStripMenuItem;

        private System.Windows.Forms.Panel pnlComponents;
        private System.Windows.Forms.Panel pnlSpecification;

        private System.Windows.Forms.DataGridView dgvComponents;
        private System.Windows.Forms.TextBox txtCompName;
        private System.Windows.Forms.ComboBox cmbCompType;
        private System.Windows.Forms.Button btnCompAdd;
        private System.Windows.Forms.Button btnCompEdit;
        private System.Windows.Forms.Button btnCompDelete;
        private System.Windows.Forms.Button btnCompSave;
        private System.Windows.Forms.Button btnCompCancel;
        private System.Windows.Forms.Button btnCompSpec;

        private System.Windows.Forms.Label lblSpecComponent;
        private System.Windows.Forms.DataGridView dgvSpecs;
        private System.Windows.Forms.Button btnSpecAdd;
        private System.Windows.Forms.Button btnSpecEdit;
        private System.Windows.Forms.Button btnSpecDelete;
        private System.Windows.Forms.Button btnSpecFind;
        private System.Windows.Forms.TextBox txtSpecSearch;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            componentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            specificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pnlComponents = new System.Windows.Forms.Panel();
            dgvComponents = new System.Windows.Forms.DataGridView();
            txtCompName = new System.Windows.Forms.TextBox();
            cmbCompType = new System.Windows.Forms.ComboBox();
            btnCompAdd = new System.Windows.Forms.Button();
            btnCompEdit = new System.Windows.Forms.Button();
            btnCompDelete = new System.Windows.Forms.Button();
            btnCompSave = new System.Windows.Forms.Button();
            btnCompCancel = new System.Windows.Forms.Button();
            btnCompSpec = new System.Windows.Forms.Button();
            pnlSpecification = new System.Windows.Forms.Panel();
            lblSpecComponent = new System.Windows.Forms.Label();
            dgvSpecs = new System.Windows.Forms.DataGridView();
            btnSpecAdd = new System.Windows.Forms.Button();
            btnSpecEdit = new System.Windows.Forms.Button();
            btnSpecDelete = new System.Windows.Forms.Button();
            btnSpecFind = new System.Windows.Forms.Button();
            txtSpecSearch = new System.Windows.Forms.TextBox();
            menuStrip.SuspendLayout();
            pnlComponents.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvComponents).BeginInit();
            pnlSpecification.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSpecs).BeginInit();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, componentsToolStripMenuItem, specificationToolStripMenuItem });
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Padding = new System.Windows.Forms.Padding(10, 4, 0, 4);
            menuStrip.Size = new System.Drawing.Size(1371, 42);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(80, 34);
            fileToolStripMenuItem.Text = "Файл";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(315, 40);
            openToolStripMenuItem.Text = "Открыть";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(315, 40);
            exitToolStripMenuItem.Text = "Выход";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // componentsToolStripMenuItem
            // 
            componentsToolStripMenuItem.Name = "componentsToolStripMenuItem";
            componentsToolStripMenuItem.Size = new System.Drawing.Size(153, 34);
            componentsToolStripMenuItem.Text = "Компоненты";
            componentsToolStripMenuItem.Click += componentsToolStripMenuItem_Click;
            // 
            // specificationToolStripMenuItem
            // 
            specificationToolStripMenuItem.Name = "specificationToolStripMenuItem";
            specificationToolStripMenuItem.Size = new System.Drawing.Size(175, 34);
            specificationToolStripMenuItem.Text = "Спецификация";
            specificationToolStripMenuItem.Click += specificationToolStripMenuItem_Click;
            // 
            // pnlComponents
            // 
            pnlComponents.Controls.Add(dgvComponents);
            pnlComponents.Controls.Add(txtCompName);
            pnlComponents.Controls.Add(cmbCompType);
            pnlComponents.Controls.Add(btnCompAdd);
            pnlComponents.Controls.Add(btnCompEdit);
            pnlComponents.Controls.Add(btnCompDelete);
            pnlComponents.Controls.Add(btnCompSave);
            pnlComponents.Controls.Add(btnCompCancel);
            pnlComponents.Controls.Add(btnCompSpec);
            pnlComponents.Location = new System.Drawing.Point(21, 54);
            pnlComponents.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            pnlComponents.Name = "pnlComponents";
            pnlComponents.Size = new System.Drawing.Size(644, 822);
            pnlComponents.TabIndex = 1;
            pnlComponents.Visible = false;
            // 
            // dgvComponents
            // 
            dgvComponents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvComponents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComponents.Location = new System.Drawing.Point(5, 6);
            dgvComponents.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            dgvComponents.Name = "dgvComponents";
            dgvComponents.RowHeadersWidth = 72;
            dgvComponents.RowTemplate.Height = 25;
            dgvComponents.Size = new System.Drawing.Size(634, 500);
            dgvComponents.TabIndex = 0;
            dgvComponents.SelectionChanged += dgvComponents_SelectionChanged;
            // 
            // txtCompName
            // 
            txtCompName.Location = new System.Drawing.Point(17, 520);
            txtCompName.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            txtCompName.Name = "txtCompName";
            txtCompName.Size = new System.Drawing.Size(340, 35);
            txtCompName.TabIndex = 1;
            // 
            // cmbCompType
            // 
            cmbCompType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCompType.Items.AddRange(new object[] { BinaryControl.ComponentType.Product, BinaryControl.ComponentType.Node, BinaryControl.ComponentType.Detail });
            cmbCompType.Location = new System.Drawing.Point(377, 520);
            cmbCompType.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            cmbCompType.Name = "cmbCompType";
            cmbCompType.Size = new System.Drawing.Size(254, 38);
            cmbCompType.TabIndex = 2;
            // 
            // btnCompAdd
            // 
            btnCompAdd.Location = new System.Drawing.Point(17, 580);
            btnCompAdd.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnCompAdd.Name = "btnCompAdd";
            btnCompAdd.Size = new System.Drawing.Size(129, 46);
            btnCompAdd.TabIndex = 3;
            btnCompAdd.Text = "Добавить";
            btnCompAdd.UseVisualStyleBackColor = true;
            btnCompAdd.Click += btnCompAdd_Click;
            // 
            // btnCompEdit
            // 
            btnCompEdit.Location = new System.Drawing.Point(0, 0);
            btnCompEdit.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnCompEdit.Name = "btnCompEdit";
            btnCompEdit.Size = new System.Drawing.Size(129, 46);
            btnCompEdit.TabIndex = 4;
            // 
            // btnCompDelete
            // 
            btnCompDelete.Location = new System.Drawing.Point(156, 580);
            btnCompDelete.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnCompDelete.Name = "btnCompDelete";
            btnCompDelete.Size = new System.Drawing.Size(129, 46);
            btnCompDelete.TabIndex = 5;
            btnCompDelete.Text = "Удалить";
            btnCompDelete.UseVisualStyleBackColor = true;
            btnCompDelete.Click += btnCompDelete_Click;
            // 
            // btnCompSave
            // 
            btnCompSave.Location = new System.Drawing.Point(295, 580);
            btnCompSave.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnCompSave.Name = "btnCompSave";
            btnCompSave.Size = new System.Drawing.Size(129, 46);
            btnCompSave.TabIndex = 6;
            btnCompSave.Text = "Сохранить";
            btnCompSave.UseVisualStyleBackColor = true;
            btnCompSave.Click += btnCompSave_Click;
            // 
            // btnCompCancel
            // 
            btnCompCancel.Location = new System.Drawing.Point(434, 580);
            btnCompCancel.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnCompCancel.Name = "btnCompCancel";
            btnCompCancel.Size = new System.Drawing.Size(129, 46);
            btnCompCancel.TabIndex = 7;
            btnCompCancel.Text = "Отменить";
            btnCompCancel.UseVisualStyleBackColor = true;
            btnCompCancel.Click += btnCompCancel_Click;
            // 
            // btnCompSpec
            // 
            btnCompSpec.Location = new System.Drawing.Point(703, 580);
            btnCompSpec.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnCompSpec.Name = "btnCompSpec";
            btnCompSpec.Size = new System.Drawing.Size(171, 46);
            btnCompSpec.TabIndex = 8;
            btnCompSpec.Text = "Спецификация";
            btnCompSpec.UseVisualStyleBackColor = true;
            btnCompSpec.Click += btnCompSpec_Click;
            // 
            // pnlSpecification
            // 
            pnlSpecification.Controls.Add(lblSpecComponent);
            pnlSpecification.Controls.Add(dgvSpecs);
            pnlSpecification.Controls.Add(btnSpecAdd);
            pnlSpecification.Controls.Add(btnSpecEdit);
            pnlSpecification.Controls.Add(btnSpecDelete);
            pnlSpecification.Controls.Add(btnSpecFind);
            pnlSpecification.Controls.Add(txtSpecSearch);
            pnlSpecification.Location = new System.Drawing.Point(670, 54);
            pnlSpecification.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            pnlSpecification.Name = "pnlSpecification";
            pnlSpecification.Size = new System.Drawing.Size(687, 822);
            pnlSpecification.TabIndex = 2;
            pnlSpecification.Visible = false;
            // 
            // lblSpecComponent
            // 
            lblSpecComponent.AutoSize = true;
            lblSpecComponent.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblSpecComponent.Location = new System.Drawing.Point(5, 6);
            lblSpecComponent.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            lblSpecComponent.Name = "lblSpecComponent";
            lblSpecComponent.Size = new System.Drawing.Size(277, 32);
            lblSpecComponent.TabIndex = 0;
            lblSpecComponent.Text = "Спецификация: [Имя]";
            // 
            // dgvSpecs
            // 
            dgvSpecs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right));
            dgvSpecs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSpecs.Location = new System.Drawing.Point(5, 50);
            dgvSpecs.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            dgvSpecs.Name = "dgvSpecs";
            dgvSpecs.RowHeadersWidth = 72;
            dgvSpecs.RowTemplate.Height = 25;
            dgvSpecs.Size = new System.Drawing.Size(677, 600);
            dgvSpecs.TabIndex = 1;
            // 
            // btnSpecAdd
            // 
            btnSpecAdd.Location = new System.Drawing.Point(5, 670);
            btnSpecAdd.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnSpecAdd.Name = "btnSpecAdd";
            btnSpecAdd.Size = new System.Drawing.Size(129, 46);
            btnSpecAdd.TabIndex = 2;
            btnSpecAdd.Text = "Добавить";
            btnSpecAdd.UseVisualStyleBackColor = true;
            btnSpecAdd.Click += btnSpecAdd_Click;
            // 
            // btnSpecEdit
            // 
            btnSpecEdit.Location = new System.Drawing.Point(144, 670);
            btnSpecEdit.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnSpecEdit.Name = "btnSpecEdit";
            btnSpecEdit.Size = new System.Drawing.Size(129, 46);
            btnSpecEdit.TabIndex = 3;
            btnSpecEdit.Text = "Изменить";
            btnSpecEdit.UseVisualStyleBackColor = true;
            btnSpecEdit.Click += btnSpecEdit_Click;
            // 
            // btnSpecDelete
            // 
            btnSpecDelete.Location = new System.Drawing.Point(283, 670);
            btnSpecDelete.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnSpecDelete.Name = "btnSpecDelete";
            btnSpecDelete.Size = new System.Drawing.Size(129, 46);
            btnSpecDelete.TabIndex = 4;
            btnSpecDelete.Text = "Удалить";
            btnSpecDelete.UseVisualStyleBackColor = true;
            btnSpecDelete.Click += btnSpecDelete_Click;
            // 
            // btnSpecFind
            // 
            btnSpecFind.Location = new System.Drawing.Point(1193, 668);
            btnSpecFind.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            btnSpecFind.Name = "btnSpecFind";
            btnSpecFind.Size = new System.Drawing.Size(96, 46);
            btnSpecFind.TabIndex = 6;
            btnSpecFind.Text = "Найти";
            btnSpecFind.UseVisualStyleBackColor = true;
            btnSpecFind.Click += btnSpecFind_Click;
            // 
            // txtSpecSearch
            // 
            txtSpecSearch.Location = new System.Drawing.Point(926, 670);
            txtSpecSearch.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            txtSpecSearch.Name = "txtSpecSearch";
            txtSpecSearch.Size = new System.Drawing.Size(254, 35);
            txtSpecSearch.TabIndex = 5;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1371, 900);
            Controls.Add(pnlSpecification);
            Controls.Add(pnlComponents);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            Text = "Многообразные списки";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            pnlComponents.ResumeLayout(false);
            pnlComponents.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvComponents).EndInit();
            pnlSpecification.ResumeLayout(false);
            pnlSpecification.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSpecs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}