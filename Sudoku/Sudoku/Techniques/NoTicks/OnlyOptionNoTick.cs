namespace BlazorSudoku
{

    /// <summary>
    /// Marks the final cell in a domain.
    /// </summary>
    public class OnlyOptionNoTick : SudokuTechnique
    {
        public override int MinComplexity => 0;

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue, int complexityLimit = int.MaxValue, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();
            var moves = new List<SudokuMove>();
            foreach (var domain in sudoku.Domains.Where(x => x.Unset.Count == 1))
            {
                var unsetValue = domain.Unset.Single();
                var move = new SudokuMove("Final Cell", 0);
                move.Operations.Add(new SudokuAction(domain.UnsetCells.Single(), SudokuActionType.SetValue, unsetValue, "Final cell in domain"));
                moves.Add(move);
            }
            return moves.OrderBy(x => x.Complexity).Take(limit).ToList();
        }
    }
}
