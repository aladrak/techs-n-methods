namespace VirtualControl.Core.Models;

public class ArrayDescriptor
{
    public long ElementCount { get; set; }
    public ArrayType ElementType { get; set; }
    public int StringLength { get; set; }  // 0 для Int, фикс.длина для Char, макс.для Varchar

    public int ElementsPerPage
    {
        get
        {
            if (ElementType == ArrayType.Varchar)
                return Constants.ElementsPerPageFixed;
            
            int elementSize = GetElementSize();
            return elementSize > 0 ? (Constants.PageSize - Constants.BitmapSizeBytes) / elementSize : 0;
        }
    }

    public int GetElementSize()
    {
        return ElementType switch
        {
            ArrayType.Int => sizeof(int),
            ArrayType.Char => StringLength > 0 ? StringLength : 1,
            ArrayType.Varchar => Constants.AddressSize,
            _ => throw new NotSupportedException($"Unknown array type: {ElementType}")
        };
    }

    public long GetTotalPages()
    {
        int elementsPerPage = ElementsPerPage;
        if (elementsPerPage <= 0) return 0;
        return (ElementCount + elementsPerPage - 1) / elementsPerPage;
    }
}