using System.Collections;

namespace Sudoku.Sudoku.HashSet
{
    /// <summary>
    /// Provides temporary <see cref="BitArray"/> instances of specific size
    /// </summary>
    public static class BAPool
    {
        private static readonly Dictionary<int, BitArray> pool = new Dictionary<int, BitArray>();

        /// <summary>
        /// Returns a temporary bitarray
        /// </summary>
        /// <param name="size"></param>
        /// <param name="clear"></param>
        /// <returns></returns>
        public static BitArray Get(int size,bool clear = true)
        {
            BitArray ret = !pool.ContainsKey(size) ? (pool[size] = new BitArray(size)) : pool[size];
            if (clear)
                ret.SetAll(false);
            return ret;
        }


    }
}
