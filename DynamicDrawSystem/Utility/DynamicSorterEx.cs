namespace CkCommons.DrawSystem;
public static class DynamicSorterEx
{
    public static ISortMethod<IDynamicCollection<T>> ByFolderName<T>() where T : class
        => new FolderName<T>();
    public static ISortMethod<IDynamicCollection<T>> ByTotalChildren<T>() where T : class
        => new TotalChildren<T>();

    // Sort Helpers
    public struct TotalChildren<T> : ISortMethod<IDynamicCollection<T>> where T : class
    {
        public string Name => "Total Count";
        public FAI Icon => FAI.SortNumericDown; // Maybe change.
        public string Tooltip => "Sort by number of items in the folder.";
        public Func<IDynamicCollection<T>, IComparable?> KeySelector => c => c.TotalChildren;
    }

    public struct FolderName<T> : ISortMethod<IDynamicCollection<T>> where T : class
    {
        public string Name => "Name";
        public FAI Icon => FAI.SortAlphaDown; // Maybe change.
        public string Tooltip => "Sort by name.";
        public Func<IDynamicCollection<T>, IComparable?> KeySelector => c => c.Name;
    }
}

