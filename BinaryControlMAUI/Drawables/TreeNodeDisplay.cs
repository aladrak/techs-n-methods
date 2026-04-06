using BinaryControlLib;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinaryControlMAUI.Drawables;

public class TreeNodeDisplay : INotifyPropertyChanged
{
    public ProductInfo Product { get; }
    public SpecInfo OwnerSpec { get; }
    
    public int FileOffset => Product.FileOffset;
    public string Name 
    { 
        get => Product.Name;
        set { Product.Name = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayText)); }
    }
    public ComponentType Type => Product.Type;
    
    public int Depth { get; set; }

    public bool IsSelected
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool CanHaveChildren => Type == ComponentType.Product || Type == ComponentType.Node;
    public bool IsDeleted => Product.IsDeleted;

    public string Indent => new(' ', Depth * 4);
    
    public string DisplayText
    {
        get
        {
            var deleted = IsDeleted ? "⚰️ " : "";
            var mult = OwnerSpec?.Multiplicity > 1 ? $" ×{OwnerSpec.Multiplicity}" : "";
            return $"{Indent}{deleted}{Name} ({Type}){mult}";
        }
    }
    
    public TreeNodeDisplay(ProductInfo product, SpecInfo ownerSpec = null, int depth = 0)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        OwnerSpec = ownerSpec;
        Depth = depth;
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}