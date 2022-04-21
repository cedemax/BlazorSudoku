using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class XYZWing : SudokuTechnique
    {
        public override int MinComplexity => 15;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit,int complexityLimit)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            foreach (var cell1 in sudoku.UnsetCells.Where(x => x.PossibleValues.Count == 3))
            {
                var xyz = cell1.PossibleValues;
                foreach (var domain in cell1.Domains)
                {
                    foreach (var cell2 in domain.UnsetCells.Where(x => x != cell1 && x.PossibleValues.Count == 2))
                    {
                        var yz = xyz.Intersect(cell2.PossibleValues).ToArray();
                        if (yz.Length == 2)
                        {
                            var x = xyz.Except(cell2.PossibleValues).Single();
                            // check the other domains
                            foreach (var domain2 in cell1.Domains.Where(x => x != domain))
                            {
                                var cell3 = domain2.UnsetCells.FirstOrDefault(x =>
                                        x != cell1 &&
                                        !x.Domains.Contains(domain) &&
                                        x.PossibleValues.Count == 2 &&
                                        x.PossibleValues.Intersect(xyz).Count() == 2 &&
                                        x.PossibleValues.Intersect(yz).Count() == 1);

                                if (cell3 != null)
                                {
                                    var move = new SudokuMove("XYZ-Wing", 15);

                                    var z = cell3.PossibleValues.Intersect(cell2.PossibleValues).Single();
                                    var y = yz.Except(new int[] { z }).Single();

                                    foreach (var cell4 in cell3.Domains
                                        .SelectMany(x => x.UnsetCells)
                                        .Where(x =>
                                            x != cell2 &&
                                            x != cell3 &&
                                            x != cell1 &&
                                            x.PossibleValues.Contains(z) &&
                                            x.Domains.Any(x => x.UnsetCells.Contains(cell2)) &&
                                            x.Domains.Any(x => x.UnsetCells.Contains(cell1))))
                                    {
                                        move.Operations.Add(new SudokuAction(cell4, SudokuActionType.RemoveOption, z, "Removed by XYZ-wing"));
                                    }

                                    if (!move.IsEmpty)
                                    {
                                        move.Hints.Add(new SudokuCellHint(cell1, SudokuHint.Base));
                                        move.Hints.Add(new SudokuCellHint(cell2, SudokuHint.Cover));
                                        move.Hints.Add(new SudokuCellHint(cell3, SudokuHint.Cover));
                                        foreach(var cell in new SudokuCell[] { cell1, cell2, cell3 })
                                        {
                                            foreach(var val in new int[] {x,y,z})
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
