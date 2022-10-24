namespace BlazorSudoku
{
    public static class IEnumerableExtensions
    {
        public static int CopyTo<T>(this IEnumerable<T> enumerable, T[] destination)
        {
            var counter = 0;
            foreach (var item in enumerable)
                destination[counter++] = item;
            return counter;
        }
    }
}
