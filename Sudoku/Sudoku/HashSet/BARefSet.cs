using System.Collections;
using System.Runtime.CompilerServices;

namespace BlazorSudoku
{
    /// <summary>
    /// A fixed size set using a bitarray
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BARefSet<T> : BA<T> where T : WithID
    {
        public BARefSet(int capacity) : base(capacity)
        {
        }

        public BARefSet(CBitArray arr) : base(arr) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => Refs[item.Key] = true;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T item) => Refs[item.Key] = false;

        public void IntersectWith(BA<T> other)
        {
            Refs.And(other.Refs);
        }

        public void UnionWith(BA<T> other)
        {
            Refs.Or(other.Refs);
        }
        public void ExceptWith(BA<T> other)
        {
            Refs.Not();
            Refs.Or(other.Refs);
            Refs.Not();
        }

        public int Count => Refs.Count;
    }

}
