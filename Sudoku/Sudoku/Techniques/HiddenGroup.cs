using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class HiddenGroup : SudokuTechnique
    {
        public override int MinComplexity => 8;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            for (var n = 2; n < 5; ++n)
            {
                foreach (var domain in sudoku.Domains)
                {
                    var unset = domain.Unset;
                    if (unset.Count <= n)  // no gains
                        continue;

                    foreach (var group in unset.GetCombinations(n))
                    {
#if DEBUG
                        if (group.ToHashSet().Count != n)
                            throw new Exception();
#endif  
                        var move = new SudokuMove(GetName(n), 2 * n*n);
                        var hasAny = domain.Cells.Where(x => x.PossibleValues.Intersect(group).Any()).ToArray();
                        if (hasAny.Length == n)  // found a group
                            foreach (var cell in hasAny)
                                foreach (var cv in cell.PossibleValues.Except(group))
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, cv, $"Hidden group [{string.Join(",", group)}] excludes this value"));
                        if (!move.IsEmpty)
                        {
                            foreach (var groupCell in hasAny)
                                foreach(var cv in groupCell.PossibleValues.Intersect(group))
                                    move.Hints.Add(new SudokuCellOptionHint(groupCell,cv, SudokuHint.Direct));

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
            name = $"Hidden {name}";
            return name;
        }
    }
}
