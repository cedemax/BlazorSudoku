using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class SelectOnlies : SudokuTechnique
    {
        public override string Name => $"Cell with only one Mark";
        public override int MinComplexity => 0;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var move = new SudokuMove("Mark Cells", MinComplexity);
            foreach (var cell in sudoku.Cells.Where(x => x.PossibleValues.Count == 1 && !x.Value.HasValue))
                move.Operations.Add(new SudokuAction(cell, SudokuActionType.SetValue, cell.PossibleValues.First(), "Mark cell"));
            if(!move.IsEmpty)
                return new List<SudokuMove> { move};
            return new List<SudokuMove>();
        }


    }
}
