using System.Collections.Concurrent;

namespace BlazorSudoku
{
    public abstract class SimpleIteratorGenerator : SudokuGenerator
    {
        private readonly SudokuTechnique[] techniques;

        public SimpleIteratorGenerator(SudokuTechnique[] techniques)
        {
            this.techniques = techniques;
        }

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue, int complexityLimit = int.MaxValue, bool hint = true)
        {
            ConcurrentBag<SudokuMove> moves = new();
            Parallel.ForEach(Enumerable.Range(0, Environment.ProcessorCount - 1),new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                var solved = GetRandomSolvedSudoku(sudoku);
                SudokuMove move = null;
                var iters = 0;
                do
                {
                    var workingSet = solved.Clone(false);
                    int grade = 0;
                    int oldGrade = 0;

                    var changeable = workingSet.Cells.Where(x => x.IsSet).ToList();
                    var unchangeable = new HashSet<SudokuCell>();

                    while (changeable.Count > 0 && moves.IsEmpty)
                    {
                        var remove = changeable
                            .GetRandom();
                        changeable.Remove(remove);

                        var val = remove.Value!.Value;
                        remove.Unset(sudoku.N);
                        oldGrade = grade;
                        grade = workingSet.Grade(out _, techniques, out var solution);
                        if (!solution.IsSolved || grade < 0)
                        {
                            workingSet.SetValue(remove, val);
                            grade = oldGrade;   // revert grading
                        }
                    }

                    // we must go on
                    if (grade < MinComplexity || !moves.IsEmpty)
                        continue;
                    move = new SudokuMove("Generate", 0);
                    foreach (var cell in workingSet.Cells.Where(x => x.IsSet))
                    {
                        var origCell = sudoku.Cells.Single(x => x.Key == cell.Key);
                        move.Operations.Add(new SudokuAction(origCell, SudokuActionType.SetValue, cell.Value!.Value, "Set by generator"));
                    }

                    break;
                } while (iters++ < 100 && moves.IsEmpty);
                if(move != null)
                    moves.Add(move);
            });

            return new() { moves.OrderBy(x => x.Complexity).Last() };
        }

    }
}
