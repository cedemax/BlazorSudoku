using BlazorSudoku;

namespace Sudoku.Sudoku.Events
{
    public class SudokuCellEventArgs : EventArgs
    {
        /// <summary>
        /// The cell that was affected
        /// </summary>
        public SudokuCell Cell { get; set; }


        public SudokuCellEventArgs(SudokuCell cell)
        {
            Cell = cell;
        }
    }
}
