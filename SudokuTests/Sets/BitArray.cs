using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuTests.Sets
{
    public class BitArrayTests
    {
        [Fact]
        public void PopCountSmall()
        {
            var C = new CBitArray(10);
            C.Set(1, true);
            C.Set(2, true);
            C.Set(3, true);

            Assert.Equal(3,C.PopCount());

            C.Set(4, true);

            Assert.Equal(4,C.PopCount());

            C.Set(4, false);

            Assert.Equal(3,C.PopCount());
        }



        [Fact]
        public void PopCountLarge()
        {
            var C = new CBitArray(100);
            C.SetAll(true);
            Assert.Equal(100, C.PopCount());

            C.Set(4, false);
            C.Set(5, false);

            Assert.Equal(98, C.PopCount());
        }

        [Fact]
        public void PopCountEven()
        {
            var C = new CBitArray(256);
            C.SetAll(true);
            Assert.Equal(256, C.PopCount());

            C.Set(4, false);
            C.Set(5, false);
            C.Set(150, false);

            Assert.Equal(256-3, C.PopCount());
        }

        [Fact]
        public void PopCountHuge()
        {
            var C = new CBitArray(1025);
            C.SetAll(true);
            Assert.Equal(1025, C.PopCount());

            C.Set(4, false);
            C.Set(5, false);
            C.Set(150, false);

            Assert.Equal(1025 - 3, C.PopCount());
        }

        [Fact]
        public void Nand()
        {
            var C = new CBitArray(1025);
            C.SetAll(true);
            Assert.Equal(1025, C.PopCount());

            C.Set(4, false);
            C.Set(5, false);
            C.Set(150, false);

            var D = new CBitArray(1025);
            D.SetAll(true);
            D.Set(1, false);
            D.Set(2, false);
            D.Set(3, false);



            Assert.Equal(3, D.NAnd(C).PopCount());
        }
    }
}
