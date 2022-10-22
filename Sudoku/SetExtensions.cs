namespace BlazorSudoku
{
    public static class SetExtensions
    {
        private static Random rng = new Random();

        public static IEnumerable<T[]> GetCombinations<T>(this IEnumerable<T> set,int n)
        {
            return RecurseCombinations(set.ToArray(),0,n-1,new T[n]);
        }

        private static IEnumerable<T[]> RecurseCombinations<T>(T[] arr,int i0,int i1,T[] current)
        {
            for(var i = i0; i < arr.Length; i++)
            {
                current[i1] = arr[i];
                if (i1 > 0)
                    foreach(var ret in RecurseCombinations(arr, i + 1, i1-1, current))
                        yield return ret;
                else
                    yield return current; 
            }
        }

        public static HashSet<T> Intersect<T>(this IEnumerable<IEnumerable<T>> arrs)
        {
            var ret = arrs.First().ToHashSet();
            foreach (var arr in arrs.Skip(1))
                ret.IntersectWith(arr);
            return ret;
        }

        public static T GetRandom<T>(this IEnumerable<T> v)
        {
            if (v is IList<T> c)
                return c[rng.Next(0, c.Count)];
            var vArr = v.ToArray();
            return vArr[rng.Next(0, vArr.Length)];
        }


    }
}
