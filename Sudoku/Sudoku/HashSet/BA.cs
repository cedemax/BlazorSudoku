using System.Collections;

namespace BlazorSudoku
{
    public abstract class BA<T> where T : WithID
    {
        public CBitArray Refs { get; }

        public BA(int capacity) { Refs = new CBitArray(capacity); }
        public BA(CBitArray arr) { Refs = arr; }

        public bool IsAllFalse() => CountTrue() == 0;
        public bool Contains(T item)
        {
            return Refs[item.Key];
        }
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
    }
}
