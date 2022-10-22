using BlazorSudoku;
using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class DirectEliminationNoMarks : SudokuTechnique
    {
        public override int MinComplexity => 1;

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();

            foreach (var domain in sudoku.Domains.Where(x => x.Set.Count < (sudoku.N-1)))
            {
                // only check those values that are set in some intersecting domain. 
                foreach (var value in domain.IntersectingDomains.Select(x => x.Set).Union().Intersect(domain.Unset))
                {
                    // check if we can restrict this value in this domain to a single cell
                    var freeCellRefs = domain.UnsetCellRefs.Except(domain.IntersectingDomains.Where(x => x.Set.Contains(value)).Select(x => x.Cells).Union());
                    if (freeCellRefs.CountTrue() == 1)
                    {
                        var freeCell = sudoku.GetCells(freeCellRefs).First();
                        if (!freeCell.PossibleValues.Contains(value))
                            continue;   // eliminated option by marks...

                        // only one option remains.
                        var move = new SudokuMove("Direct elimination", 0);
                        var eliminated = BAPool.Get<SudokuCell>(sudoku.Cells.Length);
                        var elimDomains = BAPool.Get<SudokuDomain>(sudoku.Domains.Length);

                        if (!freeCell.PossibleValues.Contains(value))
                            throw new InvalidOperationException("???");
                        move.Operations.Add(new SudokuAction(freeCell, SudokuActionType.SetValue, value, "Value set by direct elimination"));
                        foreach (var cell in domain.UnsetCells)
                        {
                            if (hint && cell != freeCell)
                                move.Hints.Add(new SudokuCellOptionHint(cell, value, SudokuHint.Elimination));
                            if (cell != freeCell && !eliminated.Contains(cell))
                            {
                                var eliminator = cell.Visible.FirstOrDefault(x => x.PossibleValues.Is(value));
                                // this might be null if the cell has been eliminated using other methods
                                if (eliminator == null)
                                    continue;

                                if(hint)
                                    move.Hints.Add(new SudokuCellHint(eliminator, SudokuHint.Elimination));

                                move.Complexity++;

                                foreach (var elimCell in domain.UnsetCells.Where(x => x.Sees(eliminator) && !eliminated.Contains(x)))
                                {
                                    if (hint)
                                    {
                                        // order here to reduce chances of using multiple domains where one would suffice
                                        var elimDomain = sudoku.GetDomains(elimCell.Domains.Intersect(eliminator.Domains))
                                            .OrderByDescending(x => sudoku.DomainIntersections[(x, domain)].Count).First();
                                        if (!elimDomains.Contains(elimDomain))
                                        {
                                            elimDomains.Add(elimDomain);
                                            move.Hints.Add(new SudokuDomainHint(elimDomain, SudokuHint.Direct));
                                        }
                                    }
                                  
                                    eliminated.Add(elimCell);
                                }
                            }
                        }
                        move.Complexity *= domain.IsBox ? 1 : 2;
                        move.Complexity = Math.Min(4, (int)(move.Complexity * 0.5));
                        moves.Add(move);
                    }
                }
            }

            return moves.OrderBy(x => x.Complexity).Take(limit).ToList();
        }
    }
}
