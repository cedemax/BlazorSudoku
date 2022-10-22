namespace BlazorSudoku.Data
{
    /// <summary>
    /// A temporary container for sudoku Cell data
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Value"></param>
    /// <param name="Options"></param>
    public record SudokuDomainData(int[] CellIndices);
}
