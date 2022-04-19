namespace BlazorSudoku.Techniques.Chain
{
    public struct SudokuCellOption
    {
        public SudokuCellOption(SudokuCell cell, int value)
        {
            Cell = cell;
            Value = value;
        }

        public SudokuCell Cell { get; }
        public int Value { get; }
    }
}
