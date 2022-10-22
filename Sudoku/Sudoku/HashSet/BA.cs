using System.Collections;

namespace BlazorSudoku
{
    public abstract class BA<T> where T : WithID
    {
        public BitArray Refs { get; }

        public BA(int capacity) { Refs = new BitArray(capacity); }
        public BA(BitArray arr) { Refs = arr; }

        public bool IsAllFalse()
        {
            for (var i = 0; i < Refs.Count; ++i)
                if (Refs[i])
                    return false;
            return true;
        }
        public bool Contains(T item)
        {
            return Refs[item.Key];
        }

        public int CountTrue()
        {
            int counter = 0;
            for (var i = 0; i < Refs.Count; ++i)
                if (Refs[i])
                    counter++;
            return counter;
        }

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
