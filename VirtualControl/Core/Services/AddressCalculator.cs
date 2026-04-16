using VirtualControl.Core.Models;

namespace VirtualControl.Core.Services;

public class AddressCalculator
{
    private readonly ArrayDescriptor _descriptor;

    public AddressCalculator(ArrayDescriptor descriptor)
    {
        _descriptor = descriptor;
    }

    public (long pageNumber, int elementOffset) GetPageAndOffset(long elementIndex)
    {
        int elementsPerPage = _descriptor.ElementsPerPage;
        if (elementsPerPage <= 0)
            throw new InvalidOperationException("ElementsPerPage is not configured");

        return (elementIndex / elementsPerPage, (int)(elementIndex % elementsPerPage));
    }

    public long GetElementOffsetInPageData(int elementOffset)
    {
        int elementSize = _descriptor.GetElementSize();
        return Constants.BitmapSizeBytes + (long)elementOffset * elementSize;
    }

    public long GetPageFileOffset(long pageNumber)
    {
        return Constants.HeaderSize + pageNumber * Constants.PageSize;
    }
}