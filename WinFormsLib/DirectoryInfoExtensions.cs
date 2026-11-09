namespace WinFormsLib
{
    /// <summary>Provides operations for renaming directories and measuring their contents.</summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>Renames a directory within its parent directory.</summary>
        /// <param name="super">The directory to rename.</param>
        /// <param name="name">The new directory name.</param>
        /// <returns><see langword="true"/> if the directory was moved, or <see langword="false"/> if the name is unchanged or the user cancels.</returns>
        /// <remarks>If the destination exists, the user is prompted before it is recursively deleted and the source is moved.</remarks>
        /// <exception cref="IOException">The source directory does not exist or a file system operation fails.</exception>
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

        /// <summary>Sums the lengths of the files in the directory and its subdirectories.</summary>
        /// <param name="super">The directory to measure.</param>
        /// <param name="cancellationToken">An optional token checked for each file found.</param>
        /// <returns>The combined file length in bytes.</returns>
        /// <exception cref="OperationCanceledException">Cancellation is requested when a file is processed.</exception>
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
