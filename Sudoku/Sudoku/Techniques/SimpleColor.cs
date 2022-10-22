using BlazorSudoku.Hints;
namespace BlazorSudoku.Techniques
{
    public class SimpleColor : SudokuTechnique
    {

        private readonly int maxChainLength;

        public SimpleColor() { maxChainLength = 128; }
        public SimpleColor(int maxChainLength)
        {
            if (maxChainLength < 2)
                throw new ArgumentOutOfRangeException(nameof(maxChainLength));
            this.maxChainLength = maxChainLength;
        }

        public override int MinComplexity => 40;
        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();
            var starts = sudoku.GetCells(sudoku.UnsetCells).Where(x => x.ConjugatePairs().Any()).GroupBy(x => x.PID).Select(x => x.First()).ToArray();
            foreach (var start in starts)
            {
                foreach (var value in start.PossibleValues)
                {
                    var coloring = new Dictionary<SudokuCell, bool>();
                    ColorBi(start, value, coloring);

                    // check if we should continue
                    if (coloring.Count > maxChainLength)
                        continue;

                    foreach (var c in coloring.Keys)
                        if (!c.PossibleValues.Contains(value))
                            throw new Exception();

                    var move = new SudokuMove($"Single Color on {value + 1}", Math.Max(MinComplexity, coloring.Count * 10));

                    if (move.Complexity > complexityLimit)
                        continue;

                    foreach (var cell in sudoku.GetCells(sudoku.UnsetCells))
                    {
                        if (cell.PossibleValues.Contains(value))
                        {
                            if (coloring.ContainsKey(cell))
                            {
                                var color = coloring[cell];
                                // check for color wrap
                                if (cell.VisibleUnset.Any(x => coloring.ContainsKey(x) && coloring[x] == color))
                                {
                                    // color wrap 
                                    foreach (var falseCell in coloring.Where(x => x.Value == color && x.Key.IsUnset).Select(x => x.Key))
                                    {
                                        if (done.Contains((falseCell, value)))
                                            continue;
                                        move.Operations.Add(new SudokuAction(falseCell, SudokuActionType.RemoveOption, value, "Removed due to color wrap"));
                                        done.Add((falseCell, value));
                                    }
                                }
                            }
                            else
                            {
                                if (cell.VisibleUnset
                                    .Where(x => coloring.ContainsKey(x))
                                    .GroupBy(x => coloring[x]).Count() > 1)
                                {
                                    if (done.Contains((cell, value)))
                                        continue;
                                    // color trap
                                    move.Operations.Add(new SudokuAction(cell, SudokuActionType.RemoveOption, value, "Removed due to color trap"));
                                }
                            }
                        }
                    }
                    if (!move.IsEmpty)
                    {
                        move.Hints.Add(new SudokuCellOptionHint(start, value, SudokuHint.Direct));
                        foreach (var (k, v) in coloring)
                            move.Hints.Add(new SudokuCellHint(k, SudokuHint.Coloring(v)));

                        moves.Add(move);
                        if (moves.Count >= limit)
                            return moves;
                    }
                }
            }
            return moves;
        }



        private void ColorBi(SudokuCell cell, int value, Dictionary<SudokuCell, bool> dict, bool color = true)
        {
            if (!dict.ContainsKey(cell))
            {
                if (!cell.PossibleValues.Contains(value))
                    throw new Exception();
                dict[cell] = color;
                foreach (var cp in cell.ConjugatePairs(value))
                {
                    ColorBi(cp, value, dict, !color);
                }
            }
        }
    }
}
