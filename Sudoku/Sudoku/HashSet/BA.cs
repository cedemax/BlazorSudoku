using System.Collections;
using System.Runtime.CompilerServices;

namespace BlazorSudoku
{
    public abstract class BA<T> where T : WithID
    {
        public CBitArray Refs { get; }

        public BA(int capacity) { Refs = new CBitArray(capacity); }
        public BA(CBitArray arr) { Refs = arr; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAllFalse() => CountTrue() == 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            return Refs[item.Key];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountTrue() => Refs.PopCount();

        public BARefSet<T> Intersect(BA<T> other)
        {
            var set = new BARefSet<T>(Refs.Count);
            set.Refs.Or(Refs);
            set.Refs.And(other.Refs);
            return set;
        }

        public BARefSet<T> Union(BA<T> other)
        {
            var set = new BARefSet<T>(Refs.Count);
            set.Refs.Or(Refs);
            set.Refs.Or(other.Refs);
            return set;
        }
        public BARefSet<T> Except(BA<T> other)
        {
            var set = new BARefSet<T>(Refs.Count);
            set.Refs.Or(other.Refs);
            set.Refs.Not();
            set.Refs.And(Refs);
            return set;
        }

        public BARefSet<T> With(T other)
        {
            var set = new BARefSet<T>(Refs.Count);
            set.Refs.Or(Refs);
            set.Refs.Set(other.Key, true);
            return set;
        }
        public BARefSet<T> Without(T other)
        {
            var set = new BARefSet<T>(Refs.Count);
            set.Refs.Or(Refs);
            set.Refs.Set(other.Key, false);
            return set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(BA<T> other)
        {
            var barr = new CBitArray(Refs);
            return barr.And(other.Refs).PopCount() > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(BA<T> other)
        {
            var barr = new CBitArray(Refs);
            return barr.And(other.Refs).PopCount() == other.Refs.PopCount();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BA<T> other)
        {
            var barr = new CBitArray(Refs);
            return barr.Xor(other.Refs).PopCount() == 0;
        }
    }
}
