namespace CkCommons.DrawSystem;

public static class DDSHelpers
{
    public static string RootName => "_root";

    // Prevent .HasFlag overhead
    public static bool HasAny(this FolderFlags flags, FolderFlags check) => (flags & check) != 0;
    public static bool HasAny(this DynamicFlags flags, DynamicFlags check) => (flags & check) != 0;

    // A filesystem name may not contain forward-slashes, as they are used to split paths.
    // The empty string as name signifies the root, so it can also not be used.
    public static string FixName(this string name)
        => name.Replace('/', '\\').Trim();
}
