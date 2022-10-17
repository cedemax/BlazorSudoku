using BlazorSudoku;

namespace BlazorSudoku
{

    /// <summary>
    /// Marks the final cell in a domain.
    /// </summary>
    public class OnlyOptionNoTick : SudokuTechnique
    {
        public override int MinComplexity => 0;

        public override List<SudokuMove> GetMoves(BlazorSudoku.Sudoku sudoku, int limit = int.MaxValue, int complexityLimit = int.MaxValue)
        {
            if (complexityLimit < MinComplexity)
                return new();
            var moves = new List<SudokuMove>();
            foreach (var domain in sudoku.Domains.Where(x => x.UnsetCells.Count == 1))
            {
                var unsetValue = domain.Unset.Except(domain.SetCells.Select(x => x.PossibleValues).Union()).FirstOrDefault(-1);
                // the sudoku is invalid...
                if (unsetValue < 0)
                    return moves;
                var move = new SudokuMove("Final Cell", 0);
                move.Operations.Add(new SudokuAction(domain.UnsetCells.Single(), SudokuActionType.SetValue, unsetValue, "Final cell in domain"));
                moves.Add(move);
            }
            return moves.OrderBy(x => x.Complexity).Take(limit).ToList();
        }
    }
}
