using System.Runtime.InteropServices;
using System.Text;

namespace NativeFileDialogExtendedSharp;

public struct NfdFilter
{
    public string Description;
    public string Specification;
}

public struct NfdDialogResult
{
    public NfdStatus Status { get; }
    public string Path { get; }
    public IReadOnlyList<string> Paths { get; }
    public string Error { get; }

    internal NfdDialogResult(NfdStatus status, string path, IReadOnlyList<string> paths, string error)
    {
        this.Status = status;
        this.Path = path;
        this.Paths = paths;
        this.Error = error;
    }

    internal static NfdDialogResult FromOk(string path, IReadOnlyList<string> paths)
    {
        return new(NfdStatus.Ok, path, paths, null);
    }

    internal static NfdDialogResult FromError(string error)
    {
        return new(NfdStatus.Error, null, null, error);
    }

    internal static NfdDialogResult FromCancel()
    {
        return new(NfdStatus.Cancel, null, null, null);
    }
}

public class Nfd
{
    public static unsafe NfdDialogResult FileOpen(IEnumerable<NfdFilter> filters, string defaultPath = null)
    {
        InitNfd();

        NfdDialogResult dialogResult = default;
        NativeFilterItem[] nativeFilters = CreateNativeFilters(filters);
        byte* outPathPtr = null;
        byte[] defaultPathCstring = string.IsNullOrEmpty(defaultPath) switch
        {
            true => null,
            false => CreateCStringUtf8(defaultPath)
        };
        fixed (byte* defaultPathPtr = defaultPathCstring is null ? null : defaultPathCstring)
        fixed (NativeFilterItem* nativeFiltersPtr = nativeFilters)
        {
            NfdStatus result = Native.NFD_OpenDialogU8(&outPathPtr, nativeFiltersPtr, (uint)nativeFilters.Length, defaultPathPtr);

            dialogResult = result switch
            {
                NfdStatus.Error => NfdDialogResult.FromError(GetError()),
                NfdStatus.Ok => NfdDialogResult.FromOk(GetOutPath(outPathPtr), null),
                NfdStatus.Cancel => NfdDialogResult.FromCancel(),
            };
        }

        Native.NFD_Quit();
        return dialogResult;
    }

    public static unsafe NfdDialogResult FileOpenMultiple(IEnumerable<NfdFilter> filters, string defaultPath = null)
    {
        InitNfd();

        NfdDialogResult dialogResult = default;
        NativeFilterItem[] nativeFilters = CreateNativeFilters(filters);

        byte* outPathsPtr = null;
        byte[] defaultPathCstring = string.IsNullOrEmpty(defaultPath) switch
        {
            true => null,
            false => CreateCStringUtf8(defaultPath)
        };
        fixed (byte* defaultPathPtr = defaultPathCstring is null ? null : defaultPathCstring)
        fixed (NativeFilterItem* nativeFiltersPtr = nativeFilters)
        {
            NfdStatus result = Native.NFD_OpenDialogMultipleU8(&outPathsPtr, nativeFiltersPtr, (uint)nativeFilters.Length, defaultPathPtr);

            dialogResult = result switch
            {
                NfdStatus.Error => NfdDialogResult.FromError(GetError()),
                NfdStatus.Ok => NfdDialogResult.FromOk(null, GetOutPaths(outPathsPtr)),
                NfdStatus.Cancel => NfdDialogResult.FromCancel(),
            };
        }

        Native.NFD_Quit();
        return dialogResult;
    }

    public static unsafe NfdDialogResult FileSave(IEnumerable<NfdFilter> filters, string defaultName = null, string defaultPath = null)
    {
        InitNfd();

        NfdDialogResult dialogResult = default;
        NativeFilterItem[] nativeFilters = CreateNativeFilters(filters);
        byte[] defaultNameCstring = CreateCStringUtf8(defaultName);
        byte[] defaultPathCstring = CreateCStringUtf8(defaultPath);

        fixed (byte* defaultNamePtr = defaultNameCstring is null ? null : defaultNameCstring)
        fixed (byte* defaultPathPtr = defaultPathCstring is null ? null : defaultPathCstring)
        fixed (NativeFilterItem* nativeFiltersPtr = nativeFilters)
        {
            byte* outPathPtr = null;
            NfdStatus result = Native.NFD_SaveDialogU8(&outPathPtr, nativeFiltersPtr, (uint)nativeFilters.Length, defaultPathPtr, defaultNamePtr);

            dialogResult = result switch
            {
                NfdStatus.Error => NfdDialogResult.FromError(GetError()),
                NfdStatus.Ok => NfdDialogResult.FromOk(GetOutPath(outPathPtr), null),
                NfdStatus.Cancel => NfdDialogResult.FromCancel(),
            };
        }

        Native.NFD_Quit();
        return dialogResult;
    }

    public static unsafe NfdDialogResult PickFolder(string defaultPath = null)
    {
        InitNfd();

        NfdDialogResult dialogResult = default;
        byte[] defaultPathCstring = CreateCStringUtf8(defaultPath);

        fixed (byte* defaultPathPtr = defaultPathCstring is null ? null : defaultPathCstring)
        {
            byte* outPathPtr = null;
            NfdStatus result = Native.NFD_PickFolderU8(&outPathPtr, defaultPathPtr);

            dialogResult = result switch
            {
                NfdStatus.Error => NfdDialogResult.FromError(GetError()),
                NfdStatus.Ok => NfdDialogResult.FromOk(GetOutPath(outPathPtr), null),
                NfdStatus.Cancel => NfdDialogResult.FromCancel(),
            };
        }

        Native.NFD_Quit();
        return dialogResult;
    }

    private static void InitNfd()
    {
        NfdStatus initResult = Native.NFD_Init();
        if (initResult is not NfdStatus.Ok)
            throw new InvalidOperationException("Failed to initialize NFD");
    }

    private static unsafe string GetOutPath(byte* outPathPtr)
    {
        string outPath = Marshal.PtrToStringUTF8((IntPtr)outPathPtr);
        Native.NFD_FreePathU8(outPathPtr);
        return outPath;
    }

    private static unsafe string[] GetOutPaths(byte* outPathsPtr)
    {
        uint pathCount = 0;
        Native.NFD_PathSet_GetCount((IntPtr)outPathsPtr, &pathCount);

        string[] outPaths = new string[pathCount];
        for(int i = 0; i < pathCount; i++)
        {
            byte* pathPtr = null;
            Native.NFD_PathSet_GetPathU8((IntPtr)outPathsPtr, (uint)i, &pathPtr);

            outPaths[i] = GetOutPath(pathPtr);
        }

        Native.NFD_PathSet_Free((IntPtr)outPathsPtr);
        return outPaths;
    }

    private static unsafe string GetError()
    {
        byte* errorCstring = Native.NFD_GetError();
        return errorCstring is not null ? Marshal.PtrToStringUTF8((IntPtr)errorCstring) : null;
    }

    private static unsafe NativeFilterItem[] CreateNativeFilters(IEnumerable<NfdFilter> filters)
    {
        (byte[] Name, byte[] Spec)[] filterStrings = filters.Select(x =>
            (CreateCStringUtf8(x.Description), CreateCStringUtf8(x.Specification))
        ).ToArray();

        return filterStrings.Select(x =>
        {
            fixed (byte* name = x.Name)
            fixed (byte* spec = x.Spec)
            {
                return new NativeFilterItem()
                {
                    Name = name,
                    Spec = spec
                };
            }
        }).ToArray();
    }

    private static unsafe byte[] CreateCStringUtf8(string input)
    {
        if(input is null)
            return null;

        int utf8Size = Encoding.UTF8.GetByteCount(input);
        byte[] cstring = new byte[utf8Size];

        fixed (char* inputPtr = input)
        fixed (byte* cstringPtr = cstring)
            Encoding.UTF8.GetBytes(inputPtr, input.Length, cstringPtr, utf8Size);

        return cstring;
    }
}