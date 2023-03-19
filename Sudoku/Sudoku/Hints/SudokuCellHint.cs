using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuCellHint : SudokuHint
    {
        public SudokuCell Cell { get; set; }
        public bool Fill { get; }

        public SudokuCellHint(SudokuCell cell, Color color,bool fill = true) : base(color)
        {
            Cell = cell;
            Fill = fill;
        }
    }
}
