using System.Collections;

namespace BlazorSudoku
{
    /// <summary>
    /// A high performance low memory fixed size hashset
    /// </summary>
    public class ReadOnlyFixedSizeHashSet : IReadOnlySet<int>
    {
        public ReadOnlyFixedSizeHashSet(int n,int value)
        {
            N = n;
            flags = value;
        }

        public bool IsReadOnly => true;

        private readonly int flags = 0;
        public int N { get; }
        public int Count { get; private set; }

        public bool Add(int item)
        {
            if (item > N) throw new ArgumentOutOfRangeException($"Can't add value {item} to fixed size set 0-{N}");
            if (IsSet(item))
            {
                return false;
            }
            else
            {
                flags |= (1 << item);
                return true;
            }
        }


        public void Clear()
        {
            flags = 0;
        }

        public bool Contains(int item)
        {
            if (item > N) throw new ArgumentOutOfRangeException($"Can't add value {item} to fixed size set 0-{N}");
            return IsSet(item);
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            for (var i = 0; i < N; ++i)
                if (IsSet(i))
                    array[arrayIndex++] = i;
        }

        public void ExceptWith(IEnumerable<int> other)
        {
            if (other is FixedSizeHashSet fshs)
            {
                flags &= (~fshs.flags);
            }
            else
            {
                foreach (var item in other)
                    Remove(item);
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return Enumerable.Range(0, N).Where(x => IsSet(x)).GetEnumerator();
        }

        public void IntersectWith(IEnumerable<int> other)
        {
            if (other is FixedSizeHashSet fshs)
            {
                flags &= fshs.flags;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool IsProperSubsetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<int> other)
        {
            if (other is FixedSizeHashSet fshs)
            {
                return (flags & fshs.flags) > 0;
            }
            throw new NotImplementedException();
        }

        public bool Remove(int item)
        {
            if (item > N) throw new ArgumentOutOfRangeException($"Can't add value {item} to fixed size set 0-{N}");
            if (IsSet(item))
            {
                flags &= ~(1 << item);
                return true;
            }
            return false;
        }

        public bool SetEquals(IEnumerable<int> other)
        {
            if (other is FixedSizeHashSet fshs)
            {
                return flags == fshs.flags;
            }
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<int> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<int> other)
        {
            if (other is FixedSizeHashSet fshs)
            {
                flags |= fshs.flags;
            }
            throw new NotImplementedException();
        }

        void ICollection<int>.Add(int item)
        {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool IsSet(int n)
        {
            return (flags & (1 << n)) > 0;
        }


        public void RemoveWhere(Func<int,bool> b)
        {
            for (var i = 0; i < N; ++i)
                if (IsSet(i) && b(i))
                    Remove(i);
        }

    }
}
