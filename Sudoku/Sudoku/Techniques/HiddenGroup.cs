using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class HiddenGroup : SudokuTechnique
    {
        private readonly int maxGroupSize;

        public HiddenGroup() { maxGroupSize = 5; }
        public HiddenGroup(int maxGroupSize)
        {
            if (maxGroupSize < 2)
                throw new ArgumentOutOfRangeException(nameof(maxGroupSize));
            this.maxGroupSize = maxGroupSize;
        }
        public override int MinComplexity => 40;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit,int complexityLimit)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            for (var n = 2; n < maxGroupSize; ++n)
            {
                if (GetComplexity(n) < MinComplexity)
                    return moves;

                foreach (var domain in sudoku.UnsetDomains)
                {
                    if (domain.Unset.Count <= n)  // no gains
                        continue;

                    foreach (var group in domain.Unset.GetCombinations(n))
                    {
                        var move = new SudokuMove(GetName(n), GetComplexity(n));
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

        private int GetComplexity(int n) => (n * n)* MinComplexity / 4;

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
