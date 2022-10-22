using Sudoku.Sudoku.HashSet;
using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace BlazorSudoku
{
    /// <summary>
    /// A limited size set using a bitarray
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BASet<T> : BA, IEnumerable<T> where T : WithID
    {
        private readonly List<T> data;

        public BitArray Refs { get; }

        public BASet(int capacity)
        {
            data = new List<T>(capacity);
            Refs = new BitArray(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (!Refs[item.Key])
                data.Add(item);
            Refs[item.Key] = true;
        }

        public int Count => data.Count;


        public IEnumerable<T> Where(BA refs)
        {
            foreach(var v in data)
                if (refs.Refs[v.Key])
                    yield return v;
        }

        public bool Contains(T item)
        {
            return Refs[item.Key];
        }

        public IEnumerator<T> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



    }

}
