namespace WinFormsLib
{
    /// <summary>Provides sequence helpers that combine filtering and projection.</summary>
    public static class IEnumerableExtensions
    {
        /// <summary>Projects the source elements and yields the non-null results in source order.</summary>
        /// <typeparam name="TSource">The source element type.</typeparam>
        /// <typeparam name="TResult">The projected element type.</typeparam>
        /// <param name="source">The sequence to project.</param>
        /// <param name="selector">The function that projects each element.</param>
        /// <returns>A sequence evaluated when enumerated.</returns>
        public static IEnumerable<TResult> SelectWhereNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> selector)
        {
            return source.Select(selector).Where(x => x != null).Cast<TResult>();
        }

        /// <summary>Filters the source elements and projects the matches in source order.</summary>
        /// <typeparam name="TSource">The source element type.</typeparam>
        /// <typeparam name="TResult">The projected element type.</typeparam>
        /// <param name="source">The sequence to filter.</param>
        /// <param name="selector">The function applied to matching elements.</param>
        /// <param name="predicate">The function that determines which elements to include.</param>
        /// <returns>A sequence evaluated when enumerated.</returns>
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
