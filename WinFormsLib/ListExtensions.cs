namespace WinFormsLib
{
    /// <summary>Provides helpers for copying, replacing, and comparing list entries.</summary>
    public static class ListExtensions
    {
        /// <summary>Creates a shallow copy of the list in the same order.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="super">The list to copy.</param>
        /// <returns>A new list whose reference-type elements are shared with the source.</returns>
        public static List<T> Clone<T>(this List<T> super) => [.. super];

        /// <summary>Clears the list and appends the supplied elements in enumeration order.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="super">The list to update.</param>
        /// <param name="other">The replacement elements.</param>
        /// <remarks>The replacement sequence is captured before the list is cleared, so it can depend on the original list.</remarks>
        public static void Replace<T>(this List<T> super, IEnumerable<T> other)
        {
            T[] replacement = other.ToArray();
            super.Clear();
            super.AddRange(replacement);
        }

        /// <summary>Replaces the first matching element at its current position.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="super">The list to update.</param>
        /// <param name="v">The element to find.</param>
        /// <param name="other">The replacement element.</param>
        public static void Replace<T>(this List<T> super, T v, T other)
        {
            int i = super.IndexOf(v);
            if (i == -1)
            {
                return;
            }
            super.RemoveAt(i);
            super.Insert(i, other);
        }

        /// <summary>Removes every element absent from the comparison sequence, preserving the order of the remaining elements.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="super">The list to update.</param>
        /// <param name="other">The elements to compare against.</param>
        /// <returns><see langword="true"/> if at least one element was removed.</returns>
        public static bool IntersectWith<T>(this List<T> super, IEnumerable<T> other)
        {
            HashSet<T> retained = other.ToHashSet();
            return super.RemoveAll(item => !retained.Contains(item)) > 0;
        }

        /// <summary>Removes every element present in the comparison sequence, preserving the order of the remaining elements.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="super">The list to update.</param>
        /// <param name="other">The elements to compare against.</param>
        /// <returns><see langword="true"/> if at least one element was removed.</returns>
        public static bool ExceptWith<T>(this List<T> super, IEnumerable<T> other)
        {
            HashSet<T> excluded = other.ToHashSet();
            return super.RemoveAll(excluded.Contains) > 0;
        }

        /// <summary>Determines whether the comparison sequence contains every distinct element of the list.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="super">The list to compare.</param>
        /// <param name="other">The sequence to check for matching elements.</param>
        public static bool IsSubsetOf<T>(this List<T> super, IEnumerable<T> other) => super.ToHashSet().IsSubsetOf(other);
    }
}
