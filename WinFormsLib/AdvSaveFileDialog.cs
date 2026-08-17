using System.Runtime.InteropServices;

namespace WinFormsLib;

public sealed class AdvSaveFileDialog
{
    public sealed record FileType(string Description, string Pattern, string DefaultFileName);

    public string DefaultExtension { get; init; } = string.Empty;
    public IReadOnlyList<FileType> FileTypes { get; init; } = [];
    public string FileName { get; private set; } = string.Empty;
    public int FilterIndex { get; private set; }

    public DialogResult ShowDialog(IWin32Window owner)
    {
        IFileDialog dialog = (IFileDialog)new FileSaveDialog();
        DialogEvents events = new(FileTypes);
        uint cookie = 0;
        try
        {
            FilterSpec[] filters = [.. FileTypes.Select(type => new FilterSpec(type.Description, type.Pattern))];
            dialog.SetFileTypes((uint)filters.Length, filters);
            dialog.SetFileTypeIndex(1);
            dialog.SetFileName(FileTypes[0].DefaultFileName);
            dialog.SetDefaultExtension(DefaultExtension);
            dialog.GetOptions(out FileOpenOptions options);
            dialog.SetOptions(options | FileOpenOptions.ForceFileSystem | FileOpenOptions.OverwritePrompt);
            dialog.Advise(events, out cookie);

            int result = dialog.Show(owner?.Handle ?? IntPtr.Zero);
            if (result == HResultCancelled)
            {
                return DialogResult.Cancel;
            }
            Marshal.ThrowExceptionForHR(result);

            dialog.GetFileTypeIndex(out uint filterIndex);
            dialog.GetResult(out IShellItem item);
            item.GetDisplayName(ShellItemDisplayName.FileSystemPath, out IntPtr pathPointer);
            try
            {
                FileName = Marshal.PtrToStringUni(pathPointer) ?? string.Empty;
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPointer);
                _ = Marshal.ReleaseComObject(item);
            }
            FilterIndex = (int)filterIndex;
            return DialogResult.OK;
        }
        finally
        {
            if (cookie != 0)
            {
                dialog.Unadvise(cookie);
            }
            _ = Marshal.ReleaseComObject(dialog);
        }
    }

    private const int HResultCancelled = unchecked((int)0x800704C7);

    [ComImport, Guid("C0B4E2F3-BA21-4773-8DBA-335EC946EB8B")]
    private class FileSaveDialog;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private readonly struct FilterSpec(string name, string specification)
    {
        [MarshalAs(UnmanagedType.LPWStr)] internal readonly string Name = name;
        [MarshalAs(UnmanagedType.LPWStr)] internal readonly string Specification = specification;
    }

    [Flags]
    private enum FileOpenOptions : uint
    {
        OverwritePrompt = 0x2,
        ForceFileSystem = 0x40
    }

    private enum ShellItemDisplayName : uint
    {
        FileSystemPath = 0x80058000
    }

    [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        void BindToHandler(IntPtr bindContext, ref Guid handler, ref Guid interfaceId, out IntPtr result);
        void GetParent(out IShellItem parent);
        void GetDisplayName(ShellItemDisplayName displayName, out IntPtr name);
        void GetAttributes(uint mask, out uint attributes);
        void Compare(IShellItem item, uint hint, out int order);
    }

    [ComImport, Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileDialog
    {
        [PreserveSig] int Show(IntPtr owner);
        void SetFileTypes(uint count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] FilterSpec[] filters);
        void SetFileTypeIndex(uint index);
        void GetFileTypeIndex(out uint index);
        void Advise(IFileDialogEvents events, out uint cookie);
        void Unadvise(uint cookie);
        void SetOptions(FileOpenOptions options);
        void GetOptions(out FileOpenOptions options);
        void SetDefaultFolder(IShellItem folder);
        void SetFolder(IShellItem folder);
        void GetFolder(out IShellItem folder);
        void GetCurrentSelection(out IShellItem item);
        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string label);
        void GetResult(out IShellItem item);
        void AddPlace(IShellItem item, uint placement);
        void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);
        void Close(int result);
        void SetClientGuid(ref Guid guid);
        void ClearClientData();
        void SetFilter(IntPtr filter);
    }

    [ComImport, Guid("973510DB-7D7F-452B-8975-74A85828D354"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileDialogEvents
    {
        [PreserveSig] int OnFileOk(IFileDialog dialog);
        [PreserveSig] int OnFolderChanging(IFileDialog dialog, IShellItem folder);
        [PreserveSig] int OnFolderChange(IFileDialog dialog);
        [PreserveSig] int OnSelectionChange(IFileDialog dialog);
        [PreserveSig] int OnShareViolation(IFileDialog dialog, IShellItem item, out uint response);
        [PreserveSig] int OnTypeChange(IFileDialog dialog);
        [PreserveSig] int OnOverwrite(IFileDialog dialog, IShellItem item, out uint response);
    }

    private sealed class DialogEvents(IReadOnlyList<FileType> fileTypes) : IFileDialogEvents
    {
        public int OnTypeChange(IFileDialog dialog)
        {
            dialog.GetFileTypeIndex(out uint index);
            if (index > 0 && index <= fileTypes.Count)
            {
                dialog.SetFileName(fileTypes[(int)index - 1].DefaultFileName);
            }
            return 0;
        }

        public int OnFileOk(IFileDialog dialog) => 0;
        public int OnFolderChanging(IFileDialog dialog, IShellItem folder) => 0;
        public int OnFolderChange(IFileDialog dialog) => 0;
        public int OnSelectionChange(IFileDialog dialog) => 0;
        public int OnShareViolation(IFileDialog dialog, IShellItem item, out uint response) { response = 0; return 0; }
        public int OnOverwrite(IFileDialog dialog, IShellItem item, out uint response) { response = 0; return 0; }
    }
}
