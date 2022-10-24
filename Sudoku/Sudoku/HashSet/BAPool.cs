using System.Collections;

namespace BlazorSudoku
{
    /// <summary>
    /// Provides temporary <see cref="BitArray"/> instances of specific size
    /// </summary>
    public static class BAPool
    {
        private static readonly Dictionary<int, CBitArray> pool = new Dictionary<int, CBitArray>();

        /// <summary>
        /// Returns a temporary bitarray
        /// </summary>
        /// <param name="size"></param>
        /// <param name="clear"></param>
        /// <returns></returns>
        public static CBitArray Get(int size, bool clear = true)
        {
            CBitArray ret = !pool.ContainsKey(size) ? (pool[size] = new CBitArray(size)) : pool[size];
            if (clear)
                ret.SetAll(false);
            return ret;
        }

        /// <summary>
        /// Returns a temporary bitarray
        /// </summary>
        /// <param name="size"></param>
        /// <param name="clear"></param>
        /// <returns></returns>
        public static BARefSet<T> Get<T>(int size, bool clear = true) where T : WithID
        {
            return new BARefSet<T>(Get(size, clear));
        }


    }
}
