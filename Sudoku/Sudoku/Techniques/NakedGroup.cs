using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class NakedGroup : SudokuTechnique
    {
        public override int MinComplexity => 4;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            for (var n = 2; n < 5; ++n)
            {
               

                foreach (var domain in sudoku.Domains)
                {
                    var groups = domain.Cells.Where(x => x.PossibleValues.Count == n).ToArray();
                    var groupGroups = groups.GroupBy(x => x.PID).Where(x => x.Count() == n).ToArray();
                    foreach (var groupGroup in groupGroups)
                    {
                        var cells = groupGroup.ToHashSet();
                        var vals = cells.First().PossibleValues;
                        var move = new SudokuMove(GetName(n),n*n);
                        foreach (var cell in domain.Cells.Except(cells))
                            if (cell.IsUnset)
                                foreach(var val in cell.PossibleValues.Intersect(vals))
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, val, "Locked into naked group"));

                        if (!move.IsEmpty)
                        {
                            foreach (var groupCell in cells)
                                move.Hints.Add(new SudokuCellHint(groupCell, SudokuHint.Direct));
                            moves.Add(move);
                            if (moves.Count >= limit)
                                return moves;
                        }
                    }
                }
            }
            return moves;
        }

        

        private string GetName(int n)
        {
            var name = n switch
            {
                2 => "Pair",
                3 => "Triplet",
                4 => "Quad",
                5 => "Penta",
                6 => "Hexa",
                7 => "Hepta",
                8 => "Octa",
                9 => "Nona",
                10 => "Deca",
                _ => "Group"
            };
            name = $"Naked {name}";
            return name;
        }
    }
}
