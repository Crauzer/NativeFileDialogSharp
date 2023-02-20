using System.Runtime.InteropServices;

namespace NativeFileDialogExtendedSharp;

internal static class Native
{
    private const string NFD_LIB = "nfd";

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_Init();

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe void NFD_Quit();

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe void NFD_FreePathU8(byte* path);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe void NFD_PathSet_Free(IntPtr pathSet);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe byte* NFD_GetError();

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_OpenDialogU8(byte** outPath, NativeFilterItem* filterList, uint filterCount, byte* defaultPath);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_OpenDialogMultipleU8(byte** outPaths, NativeFilterItem* filterList, uint filterCount, byte* defaultPath);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_SaveDialogU8(byte** outPath, NativeFilterItem* filterList, uint filterCount, byte* defaultPath, byte* defaultName);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_PickFolderU8(byte** outPath, byte* defaultPath);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_PathSet_GetCount(IntPtr pathSet, uint* count);

    [DllImport(NFD_LIB, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern unsafe NfdStatus NFD_PathSet_GetPathU8(IntPtr pathSet, uint index, byte** outPath);
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeFilterItem
{
    public byte* Name;
    public byte* Spec;
}


public enum NfdStatus
{
    Error,
    Ok,
    Cancel
}
