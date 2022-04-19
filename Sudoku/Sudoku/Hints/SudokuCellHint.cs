using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuCellHint : SudokuHint
    {
        public SudokuCell Cell { get; set; }

        public SudokuCellHint(SudokuCell cell, Color color) : base(color)
        {
            Cell = cell;
        }
    }
}
