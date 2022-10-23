using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class XYWing : SudokuTechnique
    {
        public override string Name => $"XY-Wing";
        public override int MinComplexity => 40;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            foreach (var cell1 in sudoku.UnsetCells.Where(x => x.PossibleValues.Count == 2))
            {
                foreach (var domain in cell1.Domains)
                {
                    foreach (var cell2 in domain.UnsetCells.Where(x => x != cell1 && x.PossibleValues.Count == 2))
                    {
                        var vAs = cell1.PossibleValues.Intersect(cell2.PossibleValues).ToArray();
                        if (vAs.Length == 1)
                        {
                            // a pair - now find another one
                            var vA = vAs[0];
                            var vB = cell1.PossibleValues.Where(x => x != vA).First();
                            var vC = cell2.PossibleValues.Where(x => x != vA).First();
                            // check the other domains
                            foreach (var domain2 in cell1.Domains.Where(x => x != domain))
                            {
                                var cell3 = domain2.Cells.FirstOrDefault(x =>
                                        x != cell1 &&
                                        !x.Domains.Contains(domain) &&
                                        x.PossibleValues.Count == 2 &&
                                        x.PossibleValues.Contains(vB) &&
                                        x.PossibleValues.Contains(vC));

                                if (cell3 != null)
                                {
                                    var move = new SudokuMove("XY-Wing", MinComplexity);

                                    foreach (var cell4 in cell3.Domains
                                        .SelectMany(x => x.UnsetCells)
                                        .Distinct()
                                        .Where(x =>
                                            x != cell2 &&
                                            x != cell3 &&
                                            x != cell1 &&
                                            x.PossibleValues.Contains(vC) &&
                                            x.Domains.Any(x => x.Cells.Contains(cell2))))
                                    {
                                        move.Operations.Add(new SudokuAction(cell4, SudokuActionType.RemoveOption, vC, "Removed by XY-wing"));
                                    }

                                    if (!move.IsEmpty)
                                    {
                                        move.Hints.Add(new SudokuCellHint(cell1, SudokuHint.Base));
                                        move.Hints.Add(new SudokuCellHint(cell2, SudokuHint.Cover));
                                        move.Hints.Add(new SudokuCellHint(cell3, SudokuHint.Cover));
                                        foreach(var cell in new SudokuCell[] { cell1, cell2, cell3 })
                                        {
                                            foreach(var val in new int[] {vA,vB, vC})
                                                if (cell.PossibleValues.Contains(val))
                                                    move.Hints.Add(new SudokuCellOptionHint(cell,val, SudokuHint.Direct));
                                        }
                                        moves.Add(move);
                                        if (moves.Count >= limit)
                                            return moves;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return moves;
        }

    }
}
