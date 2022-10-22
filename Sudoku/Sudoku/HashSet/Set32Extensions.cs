namespace BlazorSudoku
{
    public static class Set32Extensions
    {
        public static Set32 Union(this IEnumerable<Set32> en)
        {
            var ret = Set32.Empty;
            var any = false;
            foreach (var e in en)
            {
                any = true;
                ret.UnionWith(e);
            }
            return ret;
        }
        public static Set32 Intersect(this IEnumerable<Set32> en)
        {
            var ret = Set32.All();
            var any = false;
            foreach (var e in en)
            {
                any = true;
                ret.IntersectWith(e);
            }
            if (!any) 
                return Set32.Empty;
            return ret;
        }
    }
}
