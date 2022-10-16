namespace BlazorSudoku.Generators
{
    public class Simple : SudokuTechnique
    {
        public override int MinComplexity => 0;

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue, int complexityLimit = int.MaxValue)
        {
            var solved = GetRandomSolvedSudoku(sudoku);
            var move = new SudokuMove("Generate", 0);

            for (var i = 0; i < 100;++i)
            {
                var remove = solved.Cells.Where(x => x.IsSet).GetRandom();
                var val = remove.Value!.Value;
                remove.Unset(sudoku.N);
                var grade = solved.Grade(out _, out var solution);
                if (!solution.IsSolved)
                {
                    solved.SetValue(remove, val);
                }
            }

            foreach(var cell in solved.Cells.Where(x => x.IsSet))
            {
                var origCell = sudoku.Cells.First(x => x.Key == cell.Key);
                move.Operations.Add(new SudokuAction(origCell, SudokuActionType.SetValue, cell.Value!.Value, "Set by generator"));
            }
            return new() { move };
        }

        private Sudoku GetRandomSolvedSudoku(Sudoku sudoku)
        {
            var workingSet = sudoku.Clone(true);
            var iter = 0;
            do
            {
                _ = workingSet.Grade(out _, out var solution);
                var solved = solution.UnsetCells.Count == 0;

                if (solved)
                {
                    return solution.Clone(false);
                }
                else
                {
                    var randomCell = solution.UnsetCells.GetRandom();
                    randomCell.SetValue(randomCell.PossibleValues.GetRandom());
                    workingSet = solution;
                }

            } while (iter++ < sudoku.Cells.Length);
            throw new Exception("Failed to generate solved sudoku");
        }

    }
}
