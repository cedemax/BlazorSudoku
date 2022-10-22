using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class EliminateDirect : SudokuTechnique
    {
        public override int MinComplexity => 4;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();
            foreach (var domain in sudoku.UnsetDomains)
            {
                var move = new SudokuMove("Direct elimination", MinComplexity);
                foreach (var cell in domain.UnsetCells.Where(x => !x.PossibleValues.Intersect(domain.Set).IsEmpty))
                {
                    foreach (var toRemove in cell.PossibleValues.Intersect(domain.Set))
                    {
                        move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, toRemove, "already present in domain"));
                        if (hint)
                        {
                            var hintCell = domain.Cells.First(x => x.PossibleValues.Is(toRemove));
                            if (!move.Hints.Any(x => (x as SudokuCellHint).Cell == hintCell))
                                move.Hints.Add(new SudokuCellHint(hintCell, SudokuHint.Direct));
                        }
                    }
                }
                if (!move.IsEmpty)
                {
                    moves.Add(move);
                    if (moves.Count >= limit)
                        return moves;
                }
            }
            return moves;
        }


    }
}
