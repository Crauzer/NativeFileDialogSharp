namespace NativeFileDialogExtendedSharp.Sandbox;

internal class Program
{
    static void Main(string[] args) 
    {
        ProfilePickFolder();
    }

    static void ProfileOpenDialog()
    {
        NfdFilter[] filters = new NfdFilter[]
        {
            new() { Description = "Test", Specification = "anm" }
        };

        NfdDialogResult result = Nfd.FileOpen(filters);
    }

    static void ProfileOpenMultipleDialog()
    {
        NfdFilter[] filters = new NfdFilter[]
        {
            new() { Description = "Test", Specification = "anm" }
        };

        NfdDialogResult result = Nfd.FileOpenMultiple(filters);
    }

    static void ProfileSaveDialog()
    {
        NfdFilter[] filters = new NfdFilter[]
        {
            new() { Description = "Test", Specification = "anm" }
        };

        NfdDialogResult result = Nfd.FileSave(filters);
    }

    static void ProfilePickFolder()
    {
        NfdDialogResult result = Nfd.PickFolder();
    }
}
