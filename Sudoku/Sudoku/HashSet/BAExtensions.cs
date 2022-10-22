namespace BlazorSudoku
{
    public static class BAExtensions
    {
        public static BARefSet<T> Intersect<T>(this IEnumerable<BA<T>> sets) where T : WithID
        {
            BARefSet<T>? ret = null;
            var any = false;
            foreach (var set in sets)
            {
                if (ret == null)
                {
                    ret = new BARefSet<T>(set.Refs.Count);
                    ret.Refs.Or(set.Refs);
                }
                else
                {
                    any = true;
                    ret.Refs.And(set.Refs);
                }
            }
            if (ret == null || !any)
                throw new ArgumentException("At least two sets are required");
            return ret;
        }

        public static BARefSet<T> Union<T>(this IEnumerable<BA<T>> sets) where T : WithID
        {
            BARefSet<T>? ret = null;
            foreach (var set in sets)
            {
                ret ??= new BARefSet<T>(set.Refs.Count);
                ret.Refs.Or(set.Refs);
            }
            if (ret == null)
                throw new ArgumentException("At least one set is required");
            return ret;
        }
    }
}
