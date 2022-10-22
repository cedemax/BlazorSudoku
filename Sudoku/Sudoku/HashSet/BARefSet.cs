using System.Collections;
using System.Runtime.CompilerServices;

namespace BlazorSudoku
{
    /// <summary>
    /// A fixed size set using a bitarray
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BARefSet<T> : BA where T : WithID
    {
        public BitArray Refs { get; }

        public BARefSet(int capacity)
        {
            Refs = new BitArray(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => Refs[item.Key] = true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T item) => Refs[item.Key] = false;


        public int Count => Refs.Count;
    }

}
