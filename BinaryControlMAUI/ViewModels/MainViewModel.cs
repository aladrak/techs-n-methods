using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BinaryControlLib;
using BinaryControlMAUI.Drawables;

namespace BinaryControlMAUI.ViewModels;

public class MainViewModel
{
    private readonly FileManager _fileManager;
    private List<TreeNodeDisplay> _allNodes;
    private List<ProductInfo> _productsCache;
    private List<SpecInfo> _specsCache;
    public ObservableCollection<ProductInfo> AllProducts { get; } = new();
    public ObservableCollection<TreeNodeDisplay> VisibleNodes { get; }
    
    public ICommand LoadCommand { get; }
    public ICommand AddConnectionCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand EditNodeCommand { get; }
    public ICommand DeleteNodeCommand { get; }
    public ICommand RestoreNodeCommand { get; }
    public ICommand SaveCommand { get; }

    public bool IsDatabaseOpen
    {
        get;
        private set
        {
            field = value;
            UpdateCommandCanExecute();
        }
    }
    
    public MainViewModel(FileManager fileManager)
    {
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        VisibleNodes = new ObservableCollection<TreeNodeDisplay>();
        _allNodes = new List<TreeNodeDisplay>();
    
        LoadCommand = new Command(async () => await LoadDataAsync(), () => IsDatabaseOpen);
    
        AddConnectionCommand = new Command<TreeNodeDisplay>(async (node) => await AddConnectionAsync(node), 
            node => node?.CanHaveChildren == true && !node.IsDeleted);
    
        EditNodeCommand = new Command<TreeNodeDisplay>(async (node) => await EditNodeAsync(node), 
            node => node != null && !node.IsDeleted);
    
        DeleteNodeCommand = new Command<TreeNodeDisplay>(async (node) => await DeleteNodeAsync(node), 
            node => node != null && !node.IsDeleted);
    
        RestoreNodeCommand = new Command<TreeNodeDisplay>(async (node) => await RestoreNodeAsync(node), 
            node => node?.IsDeleted == true);
    
        AddProductCommand = new Command(async () => await AddRootProductAsync(), () => IsDatabaseOpen);
        SaveCommand = new Command(async () => await SaveChangesAsync(), () => IsDatabaseOpen);
    
        CheckDatabaseState();
    }
    
    private void CheckDatabaseState()
    {
        IsDatabaseOpen = _fileManager.IsOpen;
    }
    
    public void OnDatabaseOpened()
    {
        IsDatabaseOpen = true;
        if (LoadCommand is Command c1) c1.ChangeCanExecute();
        if (AddProductCommand is Command c2) c2.ChangeCanExecute();
        if (SaveCommand is Command c3) c3.ChangeCanExecute();
        LoadProductList(); 
    }
    
    public void LoadProductList()
    {
        AllProducts.Clear();
        if (!_fileManager.IsOpen) return;

        foreach (var product in _fileManager.Products)
        {
            AllProducts.Add(product);
        }
    }
    
    public async Task RefreshAllViewsAsync()
    {
        LoadProductList();         
        await LoadDataAsync();     
    }
    
    public async Task DeleteProductFlat(ProductInfo product)
    {
        try
        {
            _fileManager.LogicalDeleteProduct(product.Name);
            await RefreshAllViewsAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }
    
    public async Task RestoreProductFlat(ProductInfo product)
    {
        try
        {
            _fileManager.RestoreProduct(product.Name);
            await RefreshAllViewsAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }
    
    public async Task RenameProductFlat(ProductInfo product, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
    
        try
        {
            product.Name = newName;
            await RefreshAllViewsAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }
    
    private async Task LoadDataAsync()
    {
        if (!_fileManager.IsOpen) return;
        
        try
        {
            _productsCache = _fileManager.Products.Where(p => !p.IsDeleted).ToList();
            _specsCache = _fileManager.Specs.Where(s => !s.IsDeleted).ToList();
            
            BuildFlatList();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Ошибка загрузки", ex.Message, "OK");
        }
    }
    
    private void BuildFlatList()
    {
        VisibleNodes.Clear();
        _allNodes = new List<TreeNodeDisplay>();
    
        var usedAsComponent = _specsCache
            .Where(s => !s.IsDeleted)
            .Select(s => s.ProductFilePtr)
            .ToHashSet();
    
        var hasSpecification = _productsCache
            .Where(p => p.SpecFilePtr != -1 && 
                        _specsCache.Any(s => !s.IsDeleted && s.OwnerOffset == p.FileOffset))
            .Select(p => p.FileOffset)
            .ToHashSet();
    
        var visibleProducts = _productsCache
            .Where(p => !p.IsDeleted && 
                        (usedAsComponent.Contains(p.FileOffset) || hasSpecification.Contains(p.FileOffset)) || p.Type == ComponentType.Product)
            .OrderBy(p => p.Name);
    
        var childPtrs = _specsCache.Where(s => !s.IsDeleted).Select(s => s.ProductFilePtr).ToHashSet();
        var roots = visibleProducts.Where(p => !childPtrs.Contains(p.FileOffset));
    
        foreach (var root in roots)
            AddNodeRecursive(root, null, 0);
    }
    
    private void AddNodeRecursive(ProductInfo product, SpecInfo ownerSpec, int depth)
    {
        var node = new TreeNodeDisplay(product, ownerSpec, depth);
        VisibleNodes.Add(node);
        _allNodes.Add(node);
        
        if (node.CanHaveChildren)
        {
            var children = _specsCache
                .Where(s => s.OwnerOffset == product.FileOffset)
                .OrderBy(s => s.Multiplicity);
            
            foreach (var spec in children)
            {
                var childProduct = _productsCache.FirstOrDefault(p => p.FileOffset == spec.ProductFilePtr);
                if (childProduct != null)
                    AddNodeRecursive(childProduct, spec, depth + 1);
            }
        }
    }
    
    public async Task<ProductInfo> AddProductFlatAsync(string name, ComponentType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не может быть пустым", nameof(name));
    
        if (!_fileManager.IsOpen)
            throw new InvalidOperationException("База данных не открыта");
    
        var offset = _fileManager.AddProduct(name, type);
        var product = _fileManager.Products.FirstOrDefault(p => p.FileOffset == offset);
        if (product != null)
        {
            AllProducts.Add(product);
        
            if (type != ComponentType.Detail)
            {
                await LoadDataAsync();
            }
        }
    
        return product;
    }
    
    private async Task AddConnectionAsync(TreeNodeDisplay node)
    {
        if (node?.CanHaveChildren != true || node.IsDeleted) return;
        
        var choices = new[] { "Создать новый компонент", "Выбрать существующий"};
        var result = await Application.Current.MainPage.DisplayActionSheetAsync("Добавить связь", "Отмена", null, choices);
        short multiplicity = 1;
        int componentOffset;
        
        switch (result)
        {
            case "Создать новый компонент":
                var types = Enum.GetNames<ComponentType>();
                var typeResult = await Application.Current.MainPage.DisplayActionSheetAsync("Тип компонента", "Отмена", null, types);
                if (typeResult == "Отмена" || !Enum.TryParse<ComponentType>(typeResult, out var componentType))
                    return;
                
                var name = await Application.Current.MainPage.DisplayPromptAsync("Новый компонент", "Введите имя:");
                if (string.IsNullOrWhiteSpace(name)) return;
                try
                {
                    componentOffset = _fileManager.AddProduct(name, componentType);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlertAsync("Ошибка создания", ex.Message, "ОК");
                    return;
                }

                if (componentType == ComponentType.Detail)
                {
                    var multStr =
                        await Application.Current.MainPage.DisplayPromptAsync("Кратность", "Количество (1-999):",
                            initialValue: "1");
                    if (!short.TryParse(multStr, out multiplicity) || multiplicity < 1)
                    {
                        await Application.Current.MainPage.DisplayAlertAsync("Ошибка", "Неверное количество", "ОК");
                        return;
                    }
                }
                break;
                
            case "Выбрать существующий":
                var existing = _fileManager.Products.Where(p => !p.IsDeleted).ToList();
                if (!existing.Any())
                {
                    await Application.Current.MainPage.DisplayAlertAsync("Инфо", "Нет доступных компонентов", "ОК");
                    return;
                }
                
                var options = existing.Select(p => p.Name).ToArray();
                var selected = await Application.Current.MainPage.DisplayActionSheetAsync("Выберите компонент", "Отмена", null, options);
                if (selected == "Отмена") return;
                
                var found = _fileManager.FindProductByName(selected);
                if (found == null) return;
                   
                if (found.Type == ComponentType.Detail)
                {
                    var multStr =
                        await Application.Current.MainPage.DisplayPromptAsync("Кратность", "Количество (1-999):",
                            initialValue: "1");
                    if (!short.TryParse(multStr, out multiplicity) || multiplicity < 1)
                    {
                        await Application.Current.MainPage.DisplayAlertAsync("Ошибка", "Неверное количество", "ОК");
                        return;
                    }
                }
                
                componentOffset = found.FileOffset;
                break;
                
            default:
                return;
        }

        try
        {
            _fileManager.AddToSpecification(node.Product.FileOffset, componentOffset, multiplicity);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Ошибка добавления связи", ex.Message, "ОК");
        }
        await LoadDataAsync();
    }

    private async Task EditNodeAsync(TreeNodeDisplay node)
    {
        if (node == null || node.IsDeleted) return;
        
        var newName = await Application.Current.MainPage.DisplayPromptAsync(
            "Редактировать", $"Новое имя для '{node.Name}':", initialValue: node.Name);
        
        if (string.IsNullOrWhiteSpace(newName) || newName == node.Name) return;
        
        node.Name = newName;
    }

    private async Task DeleteNodeAsync(TreeNodeDisplay node)
    {
        if (node == null || node.IsDeleted) return;
    
        var descendants = GetDescendantsByRefs(node).ToList();
        var count = descendants.Count;
    
        var confirmMsg = count > 0 
            ? $"Убрать '{node.Name}' и ещё {count} компонент(а) из спецификации?" 
            : $"Убрать '{node.Name}' из спецификации?";
    
        var confirm = await Application.Current.MainPage.DisplayAlertAsync("Подтверждение", confirmMsg, "Да", "Нет");
        if (!confirm) return;
    
        try
        {
            if (node.OwnerSpec != null && !node.OwnerSpec.IsDeleted)
            {
                _fileManager.RemoveFromSpecification(node.OwnerSpec.OwnerOffset, node.FileOffset);
            }
        
            foreach (var descendant in descendants)
            {
                if (descendant.OwnerSpec != null && !descendant.OwnerSpec.IsDeleted)
                {
                    _fileManager.RemoveFromSpecification(descendant.OwnerSpec.OwnerOffset, descendant.FileOffset);
                }
            }
        
            BuildFlatList();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "ОК");
        }
    }

    private IEnumerable<TreeNodeDisplay> GetDescendantsByRefs(TreeNodeDisplay parentNode)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(parentNode.FileOffset);
        
        while (queue.Count > 0)
        {
            var currentOffset = queue.Dequeue();
            var specs = _specsCache?.Where(s => s.OwnerOffset == currentOffset && !s.IsDeleted) ?? Enumerable.Empty<SpecInfo>();
            foreach (var spec in specs)
            {
                if (visited.Contains(spec.ProductFilePtr)) continue;
                visited.Add(spec.ProductFilePtr);
                
                var childNode = _allNodes?.FirstOrDefault(n => n.FileOffset == spec.ProductFilePtr && !n.IsDeleted);
                if (childNode != null)
                {
                    yield return childNode;
                    queue.Enqueue(spec.ProductFilePtr);
                }
            }
        }
    }

    private async Task RestoreNodeAsync(TreeNodeDisplay node)
    {
        if (node?.IsDeleted != true) return;
        _fileManager.RestoreProduct(node.Name);
        await LoadDataAsync();
    }

    private async Task AddRootProductAsync()
    {
        var name = await Application.Current.MainPage.DisplayPromptAsync("Новый корневой продукт", "Введите имя продукта:");
        if (string.IsNullOrWhiteSpace(name)) return;
        
        var types = new[] { ComponentType.Product };
        var typeResult = await Application.Current.MainPage.DisplayActionSheetAsync("Тип", "Отмена", null, 
            types.Select(t => t.ToString()).ToArray());
        
        if (typeResult == "Отмена" || !Enum.TryParse<ComponentType>(typeResult, out var componentType))
            return;
        try
        {
            _fileManager.AddProduct(name, componentType);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Ошибка создания", ex.Message, "ОК");
            return;
        }
        await LoadDataAsync();
    }

    private async Task SaveChangesAsync()
    {
        if (!_fileManager.IsOpen) return;
        try
        {
            _fileManager.Truncate();
            await Application.Current.MainPage.DisplayAlertAsync("Успех", "Изменения сохранены в файлы", "ОК");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlertAsync("Ошибка сохранения", ex.Message, "ОК");
        }
    }
    
    private void UpdateCommandCanExecute()
    {
        if (LoadCommand is Command c1) c1.ChangeCanExecute();
        if (AddConnectionCommand is Command c2) c2.ChangeCanExecute();
        if (AddProductCommand is Command c3) c3.ChangeCanExecute();
        if (EditNodeCommand is Command c4) c4.ChangeCanExecute();
        if (DeleteNodeCommand is Command c5) c5.ChangeCanExecute();
        if (RestoreNodeCommand is Command c6) c6.ChangeCanExecute();
        if (SaveCommand is Command c7) c7.ChangeCanExecute();
    }
}