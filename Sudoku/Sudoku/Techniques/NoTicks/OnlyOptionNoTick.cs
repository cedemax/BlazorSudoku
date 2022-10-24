namespace BlazorSudoku.Techniques
{

    /// <summary>
    /// Marks the final cell in a domain.
    /// </summary>
    public class OnlyOptionNoTick : SudokuTechnique
    {
        public override string Name => $"Final Empty Cell";
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
                if (domain.UnsetCellRefs.CountTrue() > 0)
                    continue;   // error
                move.Operations.Add(new SudokuAction(domain.UnsetCells.First(), SudokuActionType.SetValue, unsetValue, "Final cell in domain"));
                moves.Add(move);
            }
            return moves.OrderBy(x => x.Complexity).Take(limit).ToList();
        }
    }
}
