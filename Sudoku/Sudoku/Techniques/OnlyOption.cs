using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class OnlyOption : SudokuTechnique
    {
        public override string Name => $"Final Cell with specific Mark";
        public override int MinComplexity => 20;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();
            var counts = new int[sudoku.N];
            foreach (var domain in sudoku.UnsetDomains)
            {
                sudoku.GetPossibleValueCounts(domain.UnsetCellRefs, ref counts);
                for(var value = 0; value < counts.Length; ++value)
                {
                    if (counts[value] == 1 && domain.Unset.Contains(value))
                    {
                        // only option.
                        var cell = domain.Cells.First(x => x.PossibleValues.Contains(value));

                        var move = new SudokuMove("Only remaining option", Math.Max(domain.Unset.Count * 5, MinComplexity));
                        move.Operations.Add(new SudokuAction(cell, SudokuActionType.SetOnlyPossible, value, $"Only possible cell in domain {domain} for value {value}"));
                        if(hint)
                            move.Hints.Add(new SudokuDomainHint(domain, SudokuHint.Direct));
                        moves.Add(move);
                    }
                }
            }
            return moves;
        }

       
    }
}
