namespace BlazorSudoku.Data
{
    /// <summary>
    /// A temporary container for sudoku Domain data
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Value"></param>
    /// <param name="Options"></param>
    public record SudokuCellData(int X, int Y, int? Value, int[]? Options);
}
