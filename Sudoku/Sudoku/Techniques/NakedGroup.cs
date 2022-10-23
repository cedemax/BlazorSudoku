using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class NakedGroup : SudokuTechnique
    {
        public override string Name => $"Naked Group Mark Removal (Group size up to {maxGroupSize})";
        public override string Serialize => $"{base.Serialize}{maxGroupSize}";

        private readonly int maxGroupSize;

        public NakedGroup() { maxGroupSize = 5; }
        public NakedGroup(int maxGroupSize)
        {
            if (maxGroupSize < 2)
                throw new ArgumentOutOfRangeException(nameof(maxGroupSize));
            this.maxGroupSize = maxGroupSize;
        }


        public override int MinComplexity => 20;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            for (var n = 2; n < maxGroupSize; ++n)
            {
                if (GetComplexity(n) < MinComplexity)
                    return moves;

                foreach (var domain in sudoku.UnsetDomains)
                {
                    if (domain.Unset.Count < n)
                        continue;

                    var groups = domain.UnsetCells.Where(x => x.PossibleValues.Count == n).ToArray();
                    if (groups.Length == 0)
                        continue;

                    var groupGroups = groups.GroupBy(x => x.PID).Select(x => x.ToArray()).ToArray().Where(x => x.Length == n).ToArray();
                    foreach (var groupGroup in groupGroups)
                    {
                        var vals = groupGroup[0].PossibleValues;
                        var move = new SudokuMove(GetName(n), GetComplexity(n));
                        foreach (var cell in domain.UnsetCells.Except(groupGroup))
                            if (cell.IsUnset)
                                foreach(var val in cell.PossibleValues.Intersect(vals))
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, val, "Locked into naked group"));

                        if (!move.IsEmpty)
                        {
                            foreach (var groupCell in groupGroup)
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
            name = $"Naked {name}";
            return name;
        }
    }
}
