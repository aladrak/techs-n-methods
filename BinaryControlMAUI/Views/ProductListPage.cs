using BinaryControlLib;
using BinaryControlMAUI.Drawables;
using BinaryControlMAUI.ViewModels;

namespace BinaryControlMAUI.Views;

public class ProductListPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private CollectionView _collectionView;

    public ProductListPage(MainViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BuildUI();
    }

    private void BuildUI()
    {
        Title = "Список изделий";
        BackgroundColor = Colors.White;
        SetupToolbar();

        _collectionView = new CollectionView
        {
            ItemsSource = _viewModel.AllProducts,
            SelectionMode = SelectionMode.Single,
            ItemTemplate = CreateItemTemplate(),
            EmptyView = new Label 
            { 
                Text = "Список пуст", 
                HorizontalOptions = LayoutOptions.Center, 
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray 
            },
            Margin = new Thickness(10)
        };

        _collectionView.SelectionChanged += OnSelectionChanged;

        Content = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            Children = { _collectionView }
        };
    }
    
    private void SetupToolbar()
    {
        ToolbarItems.Add(new ToolbarItem("Назад", null, 
            async () => await Navigation.PopAsync(), ToolbarItemOrder.Primary, 0));
        
        ToolbarItems.Add(new ToolbarItem("Сохранить", null, 
            async () => await SaveChangesAsync(), ToolbarItemOrder.Primary, 1));
        
        ToolbarItems.Add(new ToolbarItem("Загрузить", null, 
            async () => await LoadDataAsync(), ToolbarItemOrder.Primary, 2));
        
        ToolbarItems.Add(new ToolbarItem("Добавить", null, 
            async () => await AddProductAsync(), ToolbarItemOrder.Secondary, 0));
    }
    
    private async Task AddProductAsync()
    {
        var name = await DisplayPromptAsync("Новое изделие", "Введите имя:");
        if (string.IsNullOrWhiteSpace(name)) return;
        
        var types = Enum.GetNames<ComponentType>();
        var typeResult = await DisplayActionSheetAsync("Тип компонента", "Отмена", null, types);
        
        if (typeResult == "Отмена" || !Enum.TryParse<ComponentType>(typeResult, out var componentType))
            return;
        
        try
        {
            await _viewModel.AddProductFlatAsync(name, componentType);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "ОК");
        }
    }
    
    private async Task LoadDataAsync()
    {
        try
        {
            _viewModel.LoadProductList();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка загрузки", ex.Message, "ОК");
        }
    }

    private async Task SaveChangesAsync()
    {
        if (!_viewModel.IsDatabaseOpen)
        {
            await DisplayAlertAsync("Ошибка", "База данных не открыта", "ОК");
            return;
        }
        
        try
        {
            _viewModel.SaveCommand.Execute(null);
            await DisplayAlertAsync("Успех", "Изменения сохранены в файлы", "ОК");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка сохранения", ex.Message, "ОК");
        }
    }

    private DataTemplate CreateItemTemplate()
    {
        return new DataTemplate(() =>
        {
            var label = new Label
            {
                FontSize = 16,
                Padding = new Thickness(5),
                LineBreakMode = LineBreakMode.TailTruncation
            };
            label.SetBinding(Label.TextProperty, new Binding(nameof(ProductInfo.Name)));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(ProductInfo.IsDeleted), 
                converter: new DeletedToColorConverter()));

            return label;
        });
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ProductInfo product)
        {
            _collectionView.SelectedItem = null;

            var actions = new List<string>();
            
            if (!product.IsDeleted)
            {
                actions.Add("Переименовать");
                actions.Add("Удалить");
            }
            else
            {
                actions.Add("Восстановить");
            }
            
            actions.Add("Отмена");

            var result = await DisplayActionSheetAsync(
                $"Действия: {product.Name}",
                "Отмена",
                null,
                actions.ToArray());

            await HandleActionAsync(result, product);
        }
    }

    private async Task HandleActionAsync(string action, ProductInfo product)
    {
        switch (action)
        {
            case "Переименовать":
                var newName = await DisplayPromptAsync("Переименование", "Новое имя:", initialValue: product.Name);
                if (!string.IsNullOrWhiteSpace(newName) && newName != product.Name)
                {
                    await _viewModel.RenameProductFlat(product, newName);
                }
                break;

            case "Удалить":
                var confirm = await DisplayAlertAsync("Подтверждение", $"Удалить '{product.Name}'?", "Да", "Нет");
                if (confirm)
                {
                    await _viewModel.DeleteProductFlat(product);
                }
                break;

            case "Восстановить":
                await _viewModel.RestoreProductFlat(product);
                break;
        }
    }
}