namespace WinFormsLib
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> SelectWhereNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> selector)
        {
            return source.Select(selector).Where(x => x != null).Cast<TResult>();
        }

        public static IEnumerable<TResult> SelectWhere<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, Func<TSource, bool> predicate)
        {
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    yield return selector(item);
                }
            }
        }
    }
}
