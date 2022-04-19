using BlazorSudoku.Techniques.Chain;
using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuCellOptionHint : SudokuHint
    {
        public SudokuCell Cell { get; set; }

        public int Option { get; set; }

        public SudokuCellOptionHint(SudokuCell cell,int option, Color color) : base(color)
        {
            Cell = cell;
            Option = option;
        }
        public SudokuCellOptionHint(SudokuCellOption cell, Color color) : base(color)
        {
            Cell = cell.Cell;
            Option = cell.Value;
        }
    }
}
