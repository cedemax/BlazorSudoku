namespace BlazorSudoku.Techniques.Chain
{
    public class SudokuLink
    {
        public SudokuCellOption A { get; }
        public SudokuCellOption B { get; }

        public IEnumerable<SudokuCellOption> AB => new [] { A, B };

        public bool Strong { get; }

        public bool Weak => !Strong;

        public SudokuLink(SudokuCellOption A, SudokuCellOption B,bool strong)
        {
            this.A = A;
            this.B = B;
            Strong = strong;
        }

    }
}
