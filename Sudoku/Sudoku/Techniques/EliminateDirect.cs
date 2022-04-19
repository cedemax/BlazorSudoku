using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class EliminateDirect : SudokuTechnique
    {
        public override int MinComplexity => 1;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            foreach (var domain in sudoku.Domains)
            {
                var move = new SudokuMove("Direct elimination",1);
                var set = domain.Cells.Where(x => x.IsSet).Select(x => x.PossibleValues.First()).ToHashSet();
                foreach (var cell in domain.UnsetCells)
                {
                    var toRemoves = cell.PossibleValues.Intersect(set).ToArray();
                    if (toRemoves.Length > 0)
                    {
                        foreach (var toRemove in toRemoves)
                        {
                            if (done.Contains((cell, toRemove)))
                                continue;
                            move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, toRemove, "already present in domain"));
                            var hintCell = domain.Cells.First(x => x.IsSet && x.PossibleValues.Contains(toRemove));
                            if (!move.Hints.Any(x => (x as SudokuCellHint).Cell == hintCell))
                                move.Hints.Add(new SudokuCellHint(hintCell, SudokuHint.Direct));
                            done.Add((cell,toRemove));
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
