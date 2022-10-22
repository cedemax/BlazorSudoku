using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuTests.Sets
{
    public class ID : WithID
    {
        public int Key { get; set; }

        public static implicit operator ID(int key) => new ID { Key = key };
    }

    public static class Extensions
    {
        public static void Equal<T>(BA<T> a,BA<T> b) where T:WithID
        {
            Assert.Equal(a.Refs.Count, b.Refs.Count);

            for (var i = 0; i < a.Refs.Count; ++i)
                Assert.Equal(a.Refs[i], b.Refs[i]);
        }
    }
}
