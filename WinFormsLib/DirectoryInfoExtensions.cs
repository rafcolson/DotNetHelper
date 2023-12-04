namespace WinFormsLib
{
    public static class DirectoryInfoExtensions
    {
        public static bool Rename(this DirectoryInfo super, string name)
        {
            if (!super.Exists)
            {
                throw new IOException($"Directory '{super.FullName}' does not exist.");
            }
            if (super.Name == name)
            {
                return false;
            }
            DirectoryInfo? parent = super.Parent;
            string renamedPath = parent == null ? name : Path.Join(parent.FullName, name);
            DirectoryInfo renamed = new(renamedPath);
            if (renamed.Exists)
            {
                if (MessageBox.Show($"Overwrite directory '{name}'?", "", MessageBoxButtons.OKCancel).Equals(DialogResult.Cancel))
                {
                    return false;
                }
                renamed.Delete(true);
            }
            super.MoveTo(renamedPath);
            return true;
        }

        public static long GetLength(this DirectoryInfo super, CancellationToken? cancellationToken = null)
        {
            long l = 0;
            foreach (FileInfo fi in super.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                cancellationToken?.ThrowIfCancellationRequested();
                l += fi.Length;
            }
            return l;
        }
    }
}
