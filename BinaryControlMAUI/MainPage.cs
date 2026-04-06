using BinaryControlMAUI.Drawables;
using BinaryControlMAUI.ViewModels;
using Microsoft.UI.Xaml.Controls.Primitives;
using DataTemplate = Microsoft.Maui.Controls.DataTemplate;
using GridLength = Microsoft.Maui.GridLength;
using Thickness = Microsoft.Maui.Thickness;

namespace BinaryControlMAUI;

public class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private CollectionView _collectionView;
    
    public MainPage(MainViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
        BuildUI();
    }
    
    private void BuildUI()
    {
        Title = "Редактор спецификаций";
        BackgroundColor = Colors.White;
        SetupToolbar();
        
        // _statusLabel = new Label
        // {
        //     FontSize = 12,
        //     TextColor = Colors.Gray,
        //     Padding = new Thickness(8, 2),
        //     BackgroundColor = Colors.LightGray
        // };
        // _statusLabel.SetBinding(Label.TextProperty, nameof(MainViewModel.StatusMessage));
        
        _collectionView = new CollectionView
        {
            ItemsSource = _viewModel.VisibleNodes,
            SelectionMode = SelectionMode.Single,
            ItemTemplate = CreateNodeTemplate(),
            EmptyView = CreateEmptyView(),
            Margin = new Thickness(0, 20, 0, 28)
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
        ToolbarItems.Add(new ToolbarItem("📂 Загрузить", null, 
            () => _viewModel.LoadCommand.Execute(null), ToolbarItemOrder.Primary, 0));
        
        ToolbarItems.Add(new ToolbarItem("💾 Сохранить", null, 
            () => _viewModel.SaveCommand.Execute(null), ToolbarItemOrder.Primary, 1));
        
        ToolbarItems.Add(new ToolbarItem("➕ Добавить корень", null, 
            () => _viewModel.AddProductCommand.Execute(null), ToolbarItemOrder.Primary, 2));
    }
    
    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TreeNodeDisplay node) return;
        var actions = new List<string>();
    
        if (node is { IsDeleted: false, CanHaveChildren: true })
            actions.Add("➕ Добавить связь");
    
        if (!node.IsDeleted)
            actions.Add("✏️ Изменить");
    
        actions.Add(node.IsDeleted ? "♻️ Восстановить" : "🗑️ Удалить");
    
        var result = await DisplayActionSheetAsync(
            $"Действия: {node.Name}",
            "Отмена",
            null,
            actions.ToArray());
    
        await HandleActionAsync(result, node);
    
        _collectionView.SelectedItem = null;
    }
    
    private async Task HandleActionAsync(string action, TreeNodeDisplay node)
    {
        switch (action)
        {
            case "➕ Добавить связь" when node.CanHaveChildren && !node.IsDeleted:
                _viewModel.AddConnectionCommand.Execute(node);
                break;
            case "✏️ Изменить" when !node.IsDeleted:
                _viewModel.EditNodeCommand.Execute(node);
                break;
            case "🗑️ Удалить" when !node.IsDeleted:
                _viewModel.DeleteNodeCommand.Execute(node);
                break;
            case "♻️ Восстановить" when node.IsDeleted:
                _viewModel.RestoreNodeCommand.Execute(node);
                break;
        }
    }
    
    private static DataTemplate CreateNodeTemplate()
    {
        return new DataTemplate(() =>
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Star)
                },
                Padding = new Thickness(4, 2)
            };
            var label = new Label
            {
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            label.SetBinding(Label.TextProperty, new Binding(nameof(TreeNodeDisplay.DisplayText)));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(TreeNodeDisplay.IsDeleted), 
                converter: new DeletedToColorConverter()));
        
            var selectedBg = new BoxView { Color = Colors.Blue.WithAlpha(0.1f), IsVisible = false };
            selectedBg.SetBinding(IsVisibleProperty, new Binding(nameof(TreeNodeDisplay.IsSelected)));
        
            grid.Add(label, 0, 0);
            grid.Add(selectedBg, 0, 0);
        
            return grid;
        });
    }
    
    private static VerticalStackLayout CreateEmptyView()
    {
        return new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(30),
            Spacing = 12,
            Children =
            {
                new Label { Text = "📦", FontSize = 56 },
                new Label { Text = "Структура пуста", FontAttributes = FontAttributes.Bold, FontSize = 18 },
                new Label { Text = "• 'Загрузить' — читать файлы\n• 'Добавить корень' — создать продукт\n• Клик по элементу — меню действий", TextColor = Colors.Gray }
            }
        };
    }
}