using BlazorSudoku;
using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class DirectEliminationNoMarks : SudokuTechnique
    {
        public override int MinComplexity => 1;

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();

            foreach (var domain in sudoku.UnsetDomains)
            {
                if (domain.UnsetCells.Count() == 1)
                    continue;

                foreach (var value in domain.Unset)
                {
                    // check if we can restrict this value in this domain to a single cell

                    var freeCells = domain.UnsetCells
                        .Where(x => x.PossibleValues.Contains(value) && !x.Visible.Any(x => x.PossibleValues.Is(value)))
                        .ToList();

                    if (freeCells.Count == 1)
                    {
                        // only one option remains.
                        var move = new SudokuMove("Direct elimination",0);
                        var eliminated = new HashSet<SudokuCell>();
                        var elimDomains = new HashSet<SudokuDomain>();
                        if (!freeCells[0].PossibleValues.Contains(value))
                            throw new InvalidOperationException("???");
                        move.Operations.Add(new SudokuAction(freeCells[0], SudokuActionType.SetValue, value, "Value set by direct elimination"));
                        foreach (var cell in domain.UnsetCells)
                        {
                            if(cell != freeCells[0])
                                move.Hints.Add(new SudokuCellOptionHint(cell,value, SudokuHint.Elimination));
                            if (cell != freeCells[0] && !eliminated.Contains(cell))
                            {
                                var eliminator = cell.Visible.FirstOrDefault(x => x.PossibleValues.Is(value));
                                // this might be null if the cell has been eliminated using other methods
                                if (eliminator == null)
                                    continue;

                                move.Hints.Add(new SudokuCellHint(eliminator, SudokuHint.Elimination));

                                move.Complexity++;

                                foreach (var elimCell in domain.UnsetCells.Where(x => x.Sees(eliminator) && !eliminated.Contains(x)))
                                {
                                    // order here to reduce chances of using multiple domains where one would suffice
                                    var elimDomain = sudoku.GetDomains(elimCell.Domains.Intersect(eliminator.Domains))
                                        .OrderByDescending(x => sudoku.DomainIntersections[(x,domain)].Count).First();
                                    if (!elimDomains.Contains(elimDomain))
                                    {
                                        elimDomains.Add(elimDomain);
                                        move.Hints.Add(new SudokuDomainHint(elimDomain, SudokuHint.Direct));
                                    }
                                    eliminated.Add(elimCell);
                                }
                            }
                        }
                        move.Complexity *= domain.IsBox ? 1 : 2;
                        move.Complexity = Math.Min(4,(int)(move.Complexity*0.5));
                        moves.Add(move);
                    }
                }
            }

            return moves.OrderBy(x => x.Complexity).Take(limit).ToList();
        }
    }
}
