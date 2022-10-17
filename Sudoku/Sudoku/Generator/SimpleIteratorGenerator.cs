namespace BlazorSudoku
{
    public abstract class SimpleIteratorGenerator : SudokuGenerator
    {
        private readonly SudokuTechnique[] techniques;

        public SimpleIteratorGenerator(SudokuTechnique[] techniques)
        {
            this.techniques = techniques;
        }

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue, int complexityLimit = int.MaxValue)
        {
            var solved = GetRandomSolvedSudoku(sudoku);
            var move = new SudokuMove("Generate", 0);
            var iters = 0;
            do
            {
                var workingSet = solved.Clone(false);
                int grade = 0;
                int oldGrade = 0;

                var changeable = workingSet.Cells.Where(x => x.IsSet).ToList();
                var unchangeable = new HashSet<SudokuCell>();

                while(changeable.Count > 0)
                {
                    var remove = changeable
                        .GetRandom();
                    changeable.Remove(remove);

                    var val = remove.Value!.Value;
                    remove.Unset(sudoku.N);
                    oldGrade = grade;
                    grade = workingSet.Grade(out _, techniques, out var solution);
                    if (!solution.IsSolved)
                    {
                        workingSet.SetValue(remove, val);
                        grade = oldGrade;   // revert grading
                    }
                }
                // we must go on
                if (grade < MinComplexity)
                    continue;

                foreach (var cell in workingSet.Cells.Where(x => x.IsSet))
                {
                    var origCell = sudoku.Cells.Single(x => x.Key == cell.Key);
                    move.Operations.Add(new SudokuAction(origCell, SudokuActionType.SetValue, cell.Value!.Value, "Set by generator"));
                }

                break;
            } while (iters++ < 100);

            return new() { move };


        }

    }
}
