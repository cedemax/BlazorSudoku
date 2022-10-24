using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class HiddenGroup : SudokuTechnique
    {
        private readonly int maxGroupSize;
        public override string Name => $"Hidden Group Mark Removal (Group size up to {maxGroupSize})";
        public override string Serialize => $"{base.Serialize}{maxGroupSize}";

        public HiddenGroup() { maxGroupSize = 5; }
        public HiddenGroup(int maxGroupSize)
        {
            if (maxGroupSize < 2)
                throw new ArgumentOutOfRangeException(nameof(maxGroupSize));
            this.maxGroupSize = maxGroupSize;
        }
        public override int MinComplexity => 40;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();

            static bool groupTest(SudokuCell cell, Set32 mask) => !cell.PossibleValues.Intersect(mask).IsEmpty;

            for (var n = 2; n < maxGroupSize; ++n)
            {
                if (GetComplexity(n) > complexityLimit)
                    return moves;

                foreach (var domain in sudoku.UnsetDomains)
                {
                    // at least N+1 numbers are needed. N for the group, and 1 for removal
                    if (domain.Unset.Count < (n + 1))
                        continue;

                    foreach (var mask in domain.Unset.GetPermutations(n))
                    {
                        if (domain.UnsetCells.Count(x => groupTest(x, mask)) == n)
                        {
                            // we found a N-group
                            var groupCells = domain.UnsetCells.Where(x => groupTest(x, mask)).ToArray();
                            // make sure it actually removes something
                            var nonGroup = groupCells.Select(x => x.PossibleValues).Union().Except(mask);
                            if (nonGroup.IsEmpty)
                                continue;

                            var move = new SudokuMove(GetName(n), GetComplexity(n));

                            foreach (var groupCell in groupCells)
                            {
                                foreach (var valToRemove in groupCell.PossibleValues.Except(mask))
                                {
                                    if(hint)
                                        move.Hints.Add(new SudokuCellOptionHint(groupCell, valToRemove, SudokuHint.Elimination));

                                    move.Operations.Add(new SudokuAction(groupCell, SudokuActionType.RemoveOption, valToRemove, "Removed by hidden group"));
                                }
                                if (hint)
                                {
                                    move.Hints.Add(new SudokuCellHint(groupCell, SudokuHint.Direct));
                                    foreach (var groupVal in groupCell.PossibleValues.Intersect(mask))
                                        move.Hints.Add(new SudokuCellOptionHint(groupCell,groupVal,SudokuHint.Direct));
                                }
                            }

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
