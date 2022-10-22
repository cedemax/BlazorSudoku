using System.Reflection;

namespace BlazorSudoku
{
    public abstract class SudokuGenerator : SudokuTechnique
    {
        protected Sudoku GetRandomSolvedSudoku(Sudoku sudoku)
        {
            for (var i = 0; i < 100; ++i)
            {
                var workingSet = sudoku.Clone(true);

                var iter = 0;
                do
                {
                    _ = workingSet.Grade(out _,null, out var solution);
                    solution.IsValid();
                    // try again if we have failed
                    if (solution.Domains.Any(x => x.Error))
                        break;

                    if (solution.IsSolved)
                    {
                        return solution.Clone(false);
                    }
                    else
                    {
                        var randomCell = solution.GetCells(solution.UnsetCells).GetRandom();
                        randomCell.SetValue(randomCell.PossibleValues.GetRandom());

                        workingSet = solution;
                    }

                } while (iter++ < sudoku.Cells.Length);
            }

            throw new Exception("Failed to generate solved sudoku");
        }

        public static SudokuGenerator[] GetAllGenerators()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(x =>
                    !x.IsAbstract &&
                    (x.BaseType == typeof(SudokuGenerator) || x.BaseType!.BaseType == typeof(SudokuGenerator))
                ).Select(x => Activator.CreateInstance(x) as SudokuGenerator).ToArray()!;
        }
    }
}
