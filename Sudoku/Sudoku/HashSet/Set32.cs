using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BlazorSudoku
{
    /// <summary>
    /// A high performance low memory fixed size set
    /// </summary>
    public struct Set32 : IEnumerable<int>
    {
        private const int MaximumSize = sizeof(int) * 8;

        public Set32() { flags= 0; count = -1; }
        public Set32(uint value)
        {
            flags = value;
            count = -1;
        }

        private uint flags;
        private sbyte count;

        public uint Flag => flags;

        public int Count
        {
            get
            {
                if (count < 0)
                    count = (sbyte)BitOperations.PopCount(flags);
                return count;
            }
        }

        public bool IsEmpty => flags == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int item)
        {
            flags = flags | (1u << item);
            count = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            flags = 0;
            count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int item)
        {
#if DEBUG
            if (item > MaximumSize) throw new ArgumentOutOfRangeException($"Can't add value {item} to fixed size set");
#endif
            return IsSet(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Is(int item)
        {
#if DEBUG
            if (item > MaximumSize) throw new ArgumentOutOfRangeException($"Can't add value {item} to fixed size set");
#endif
            return flags == (1u<<item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int item)
        {
#if DEBUG
            if (item > MaximumSize) throw new ArgumentOutOfRangeException($"Can't add value {item} to fixed size set");
#endif
            flags &= ~(1u << item);
            count = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(Set32 other) => (flags & other.flags) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Set32 Except(Set32 other) => new(flags & (~other.flags));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExceptWith(Set32 other)
        {
            count = -1;
            flags &= (~other.flags);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Set32 Intersect(Set32 other) => new(flags & other.flags);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntersectWith(Set32 other)
        {
            count = -1;
            flags &= other.flags;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Set32 Union(Set32 other) => new(flags | other.flags);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnionWith(Set32 other)
        {
            count = -1;
            flags |= other.flags;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSet(int n) => (flags & (1u << n)) > 0;


        public void RemoveWhere(Func<int, bool> b)
        {
            count = -1;
            for (var i = 0; i < MaximumSize; ++i)
                if (IsSet(i) && b(i))
                    Remove(i);
        }

        public IEnumerator<int> GetEnumerator()
        {
            var min = BitOperations.TrailingZeroCount(flags);
            var max = MaximumSize-BitOperations.LeadingZeroCount(flags);
            for(var i = min; i < max; ++i)
            {
                if((flags & (1u<<i)) > 0)
                    yield return i;
            }
        }

        public IEnumerable<Set32> GetPermutations(int n)
        {
            return n switch
            {
                1 => GetPermutations1(),
                2 => GetPermutations2(),
                3 => GetPermutations3(),
                4 => GetPermutations4(),
                _ => throw new NotImplementedException(),
            };
        }

        private IEnumerable<Set32> GetPermutations1()
        {
            var min = BitOperations.TrailingZeroCount(flags);
            var max = MaximumSize - BitOperations.LeadingZeroCount(flags);
            var ret = Set32.Empty;
            for (var i = min; i < max; ++i)
            {
                if ((flags & (1u << i)) > 0)
                {
                    ret.Add(i);
                    yield return ret;
                    ret.Remove(i);
                }
                ret.Clear();
            }
        }

        private IEnumerable<Set32> GetPermutations2()
        {
            var min = BitOperations.TrailingZeroCount(flags);
            var max = MaximumSize - BitOperations.LeadingZeroCount(flags);
            var ret = Set32.Empty;
            for (var i = min; i < max; ++i)
            {
                if ((flags & (1u << i)) > 0)
                {
                    ret.Add(i);
                    for (var j = i + 1; j < max; ++j)
                    {
                        if ((flags & (1u << j)) > 0)
                        {
                            ret.Add(j);
                            yield return ret;
                            ret.Remove(j);
                        }
                    }
                    ret.Remove(i);
                }
            }
        }


        private IEnumerable<Set32> GetPermutations3()
        {
            var min = BitOperations.TrailingZeroCount(flags);
            var max = MaximumSize - BitOperations.LeadingZeroCount(flags);
            var ret = Set32.Empty;
            for (var i = min; i < max; ++i)
            {
                if ((flags & (1u << i)) > 0)
                {
                    ret.Add(i);
                    for (var j = i + 1; j < max; ++j)
                    {
                        if ((flags & (1u << j)) > 0)
                        {
                            ret.Add(j);
                            for (var k = j + 1; k < max; ++k)
                            {
                                if ((flags & (1u << k)) > 0)
                                {
                                    ret.Add(k);
                                    yield return ret;
                                    ret.Remove(k);
                                }
                            }
                            ret.Remove(j);
                        }
                    }
                    ret.Remove(i);
                }
            }
        }

        private IEnumerable<Set32> GetPermutations4()
        {
            var min = BitOperations.TrailingZeroCount(flags);
            var max = MaximumSize - BitOperations.LeadingZeroCount(flags);
            var ret = Set32.Empty;
            for (var i = min; i < max; ++i)
            {
                if ((flags & (1u << i)) > 0)
                {
                    ret.Add(i);
                    for (var j = i + 1; j < max; ++j)
                    {
                        if ((flags & (1u << j)) > 0)
                        {
                            ret.Add(j);
                            for (var k = j + 1; k < max; ++k)
                            {
                                if ((flags & (1u << k)) > 0)
                                {
                                    ret.Add(k);
                                    for (var l = k + 1; l< max; ++l)
                                    {
                                        if ((flags & (1u << l)) > 0)
                                        {
                                            ret.Add(l);
                                            yield return ret;
                                            ret.Remove(l);
                                        }
                                    }
                                    ret.Remove(k);
                                }
                            }
                            ret.Remove(j);
                        }
                    }
                    ret.Remove(i);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static Set32 Empty => new (0);
        public static Set32 All(int n = MaximumSize) => new((~0u) >>> (MaximumSize - n));

    }
}
