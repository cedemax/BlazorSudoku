namespace SudokuTests.Sets
{
    public class BABooleans
    {
        [Fact]
        public void Intersect()
        {
            var C = new BARefSet<ID>(10);
            C.Add(3);

            Equals(C, A.Intersect(B));
        }

        [Fact]
        public void Union()
        {
            var C = new BARefSet<ID>(10);
            C.Add(1);
            C.Add(2);
            C.Add(3);
            C.Add(5);
            C.Add(9);

            Equals(C, A.Union(B));
        }

        [Fact]
        public void Except()
        {
            var C = new BARefSet<ID>(10);
            C.Add(1);
            C.Add(9);

            Equals(C, A.Except(B));

            var D = new BARefSet<ID>(10);
            D.Add(2);
            D.Add(5);

            Equals(D, B.Except(A));
        }


        private static BARefSet<ID> A
        {
            get
            {
                var A = new BARefSet<ID>(10);
                A.Add(1);
                A.Add(3);
                A.Add(9);
                return A;
            }
        }
        private static BARefSet<ID> B
        {
            get
            {
                var A = new BARefSet<ID>(10);
                A.Add(5);
                A.Add(3);
                A.Add(2);
                return A;
            }
        }

    }
}
