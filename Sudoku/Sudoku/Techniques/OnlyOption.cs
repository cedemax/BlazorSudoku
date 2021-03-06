using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class OnlyOption : SudokuTechnique
    {
        public override int MinComplexity => 8;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit,int complexityLimit)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            foreach (var domain in sudoku.UnsetDomains)
            {
                foreach (var cell in domain.UnsetCells)
                {
                    foreach (var pvalue in cell.PossibleValues)
                    {
                        if (!domain.Cells.Any(x => x != cell && x.PossibleValues.Contains(pvalue)))
                        {
                            if (done.Contains((cell, pvalue)))
                                continue;
                            var move = new SudokuMove("Only remaining option",8);
                            move.Operations.Add(new SudokuAction(cell, SudokuActionType.SetOnlyPossible, pvalue, $"Only possible cell in domain {domain}"));
                            move.Hints.Add(new SudokuDomainHint(domain, SudokuHint.Direct));
                            moves.Add(move);
                        }
                    }
                }
            }
            return moves;
        }

       
    }
}
