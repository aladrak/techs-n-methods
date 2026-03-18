using BinaryControl;

namespace BinaryControlGUI;
public partial class MainForm : Form
{
    private FileManager _fileManager = new FileManager();
    private bool _isOpen = false;

    private BindingSource _componentsBinding = new BindingSource();
    private ProductInfo _currentComponent;
    private bool _addingComponent;

    private BindingSource _specsBinding = new BindingSource();
    private int _currentSpecOwnerOffset;
    private ProductInfo _currentSpecProduct;

    public MainForm()
    {
        InitializeComponent();
        UpdateMenuState();
        SetupComponentsGrid();
        SetupSpecGrid();
    }

    private void UpdateMenuState()
    {
        componentsToolStripMenuItem.Enabled = _isOpen;
        specificationToolStripMenuItem.Enabled = _isOpen;
    }

    private void SetupComponentsGrid()
    {
        dgvComponents.AutoGenerateColumns = false;
        dgvComponents.Columns.Clear();
        dgvComponents.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Name",
            HeaderText = "Наименование"
        });
        dgvComponents.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Type",
            HeaderText = "Тип"
        });
        dgvComponents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvComponents.MultiSelect = false;
        dgvComponents.DataSource = _componentsBinding;
    }

    private void SetupSpecGrid()
    {
        dgvSpecs.AutoGenerateColumns = false;
        dgvSpecs.Columns.Clear();
        dgvSpecs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ComponentName",
            HeaderText = "Наименование"
        });
        dgvSpecs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "ComponentType",
            HeaderText = "Тип"
        });
        dgvSpecs.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = "Multiplicity",
            HeaderText = "Кратность"
        });
        dgvSpecs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSpecs.MultiSelect = false;
        dgvSpecs.DataSource = _specsBinding;
    }

    private void LoadComponents()
    {
        var list = _fileManager.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name).ToList();
        _componentsBinding.DataSource = list;
        _componentsBinding.ResetBindings(false);
    }

    private void LoadSpecification(int productOffset)
    {
        _currentSpecOwnerOffset = productOffset;
        _currentSpecProduct = _fileManager.Products.First(p => p.FileOffset == productOffset);
        lblSpecComponent.Text = $"Спецификация: {_currentSpecProduct.Name} ({_currentSpecProduct.Type})";

        var specs = _fileManager.GetSpecsForProduct(productOffset).ToList();
        var display = specs.Select(s =>
        {
            var comp = _fileManager.Products.FirstOrDefault(p => p.FileOffset == s.ProductFilePtr);
            return new
            {
                s.FileOffset,
                ComponentName = comp?.Name ?? "?",
                ComponentType = comp?.Type.ToString() ?? "?",
                s.Multiplicity
            };
        }).ToList();
        _specsBinding.DataSource = display;
        _specsBinding.ResetBindings(false);
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string fileName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя базы данных (без расширения):", "Открыть базу", "");
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        try
        {
            _fileManager.OpenDatabase(fileName.Trim());
            _isOpen = true;
            UpdateMenuState();
            pnlComponents.Visible = false;
            pnlSpecification.Visible = false;
            MessageBox.Show("База открыта", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка открытия: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void componentsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        pnlSpecification.Visible = false;
        pnlComponents.Visible = true;
        LoadComponents();
        ClearComponentFields();
    }

    private void specificationToolStripMenuItem_Click(object sender, EventArgs e)
    {
        string compName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя компонента:", "Просмотр спецификации", "");
        if (string.IsNullOrWhiteSpace(compName))
            return;

        var prod = _fileManager.FindProductByName(compName);
        if (prod == null)
        {
            MessageBox.Show("Компонент не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (prod.Type == ComponentType.Detail)
        {
            MessageBox.Show("Деталь не имеет спецификации", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        pnlComponents.Visible = false;
        pnlSpecification.Visible = true;
        LoadSpecification(prod.FileOffset);
        txtSpecSearch.Clear();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();
    
    private void dgvComponents_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvComponents.SelectedRows.Count > 0)
        {
            _currentComponent = dgvComponents.SelectedRows[0].DataBoundItem as ProductInfo;
            if (_currentComponent != null)
            {
                txtCompName.Text = _currentComponent.Name;
                cmbCompType.SelectedItem = _currentComponent.Type;
                _addingComponent = false;
            }
        }
        UpdateCompButtons();
    }

    private void UpdateCompButtons()
    {
        bool hasSel = dgvComponents.SelectedRows.Count > 0;
        btnCompEdit.Enabled = hasSel && !_addingComponent;
        btnCompDelete.Enabled = hasSel && !_addingComponent;
    }

    private void ClearComponentFields()
    {
        txtCompName.Clear();
        cmbCompType.SelectedIndex = -1;
        _currentComponent = null;
        _addingComponent = false;
        UpdateCompButtons();
    }

    private void btnCompAdd_Click(object sender, EventArgs e)
    {
        ClearComponentFields();
        _addingComponent = true;
        cmbCompType.SelectedIndex = 0;
    }

    private void btnCompSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCompName.Text))
        {
            MessageBox.Show("Введите наименование", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (cmbCompType.SelectedItem == null)
        {
            MessageBox.Show("Выберите тип", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ComponentType type = (ComponentType)cmbCompType.SelectedItem;

        try
        {
            if (_addingComponent)
            {
                _fileManager.AddProduct(txtCompName.Text.Trim(), type);
            }
            else if (_currentComponent != null)
            {
                _currentComponent.Name = txtCompName.Text.Trim();
                _currentComponent.Type = type;
                _fileManager.UpdateProduct(_currentComponent);
            }

            _fileManager.Truncate();
            _fileManager.Reload();
            LoadComponents();
            ClearComponentFields();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCompDelete_Click(object sender, EventArgs e)
    {
        if (_currentComponent == null) return;
        try
        {
            _fileManager.LogicalDeleteProduct(_currentComponent.Name);
            _fileManager.Truncate();
            _fileManager.Reload();
            LoadComponents();
            ClearComponentFields();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnCompCancel_Click(object sender, EventArgs e)
    {
        _fileManager.Reload();
        LoadComponents();
        ClearComponentFields();
    }

    private void btnCompSpec_Click(object sender, EventArgs e)
    {
        if (_currentComponent == null) return;
        if (_currentComponent.Type == ComponentType.Detail)
        {
            MessageBox.Show("Деталь не имеет спецификации", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        pnlComponents.Visible = false;
        pnlSpecification.Visible = true;
        LoadSpecification(_currentComponent.FileOffset);
        txtSpecSearch.Clear();
    }

    private void btnSpecAdd_Click(object sender, EventArgs e)
    {
        string compName = Microsoft.VisualBasic.Interaction.InputBox("Введите имя компонента для добавления:", "Добавление");
        if (string.IsNullOrWhiteSpace(compName)) return;

        var comp = _fileManager.FindProductByName(compName);
        if (comp == null)
        {
            MessageBox.Show("Компонент не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string multStr = Microsoft.VisualBasic.Interaction.InputBox("Введите кратность:", "Кратность", "1");
        if (!short.TryParse(multStr, out short mult) || mult <= 0)
        {
            MessageBox.Show("Некорректная кратность", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _fileManager.AddToSpecification(_currentSpecOwnerOffset, comp.FileOffset, mult);
            _fileManager.Truncate();
            _fileManager.Reload();
            LoadSpecification(_currentSpecOwnerOffset);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnSpecEdit_Click(object sender, EventArgs e)
    {
        if (dgvSpecs.SelectedRows.Count == 0) return;
        dynamic selected = dgvSpecs.SelectedRows[0].DataBoundItem;
        int specOffset = selected.FileOffset;
        var spec = _fileManager.Specs.First(s => s.FileOffset == specOffset);

        string multStr = Microsoft.VisualBasic.Interaction.InputBox("Введите новую кратность:", "Изменение", spec.Multiplicity.ToString());
        if (!short.TryParse(multStr, out short mult) || mult <= 0)
        {
            MessageBox.Show("Некорректная кратность", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        spec.Multiplicity = mult;
        _fileManager.UpdateSpec(spec);
        _fileManager.Truncate();
        _fileManager.Reload();
        LoadSpecification(_currentSpecOwnerOffset);
    }

    private void btnSpecDelete_Click(object sender, EventArgs e)
    {
        if (dgvSpecs.SelectedRows.Count == 0) return;
        dynamic selected = dgvSpecs.SelectedRows[0].DataBoundItem;
        int specOffset = selected.FileOffset;
        var spec = _fileManager.Specs.First(s => s.FileOffset == specOffset);

        spec.IsDeleted = true;
        _fileManager.Truncate();
        _fileManager.Reload();
        LoadSpecification(_currentSpecOwnerOffset);
    }

    private void btnSpecFind_Click(object sender, EventArgs e)
    {
        string search = txtSpecSearch.Text.Trim();
        if (string.IsNullOrEmpty(search)) return;

        var comp = _fileManager.FindProductByName(search);
        if (comp == null)
        {
            MessageBox.Show("Компонент не найден", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show($"Найден: {comp.Name} ({comp.Type})", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}