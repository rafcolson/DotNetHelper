namespace WinFormsLib
{
    public static class ListExtensions
    {
        public static List<T> Clone<T>(this List<T> super) => new(super);

        public static void Replace<T>(this List<T> super, IEnumerable<T> other)
        {
            super.Clear();
            super.AddRange(other);
        }

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

        public static bool IntersectWith<T>(this List<T> super, IEnumerable<T> other)
        {
            IEnumerable<T> ie = super.Except(other);
            if (ie.Any())
            {
                foreach (T v in ie.ToArray())
                {
                    super.Remove(v);
                }
                return true;
            }
            return false;
        }

        public static bool ExceptWith<T>(this List<T> super, IEnumerable<T> other)
        {
            IEnumerable<T> ie = super.Intersect(other);
            if (ie.Any())
            {
                foreach (T v in ie.ToArray())
                {
                    super.Remove(v);
                }
                return true;
            }
            return false;
        }

        public static bool IsSubsetOf<T>(this List<T> super, IEnumerable<T> other) => super.ToHashSet().IsSubsetOf(other);
    }
}
