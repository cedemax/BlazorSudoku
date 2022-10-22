using BlazorSudoku.Techniques.Chain;

namespace BlazorSudoku.Techniques
{
    public class XYChain : ChainTechnique
    {
        public override int MinComplexity => 100;

        protected override IEnumerable<SudokuChainNode> Starts(Sudoku sudoku)
        {
            foreach(var start in sudoku.UnsetCells.Where(x => x.PossibleValues.Count == 2))
            {
                var options = start.PossibleValues.Select(x => new SudokuCellOption(start, x)).ToArray();
                foreach (var value in start.PossibleValues)
                {
                    var startOption = options.First(x => x.Value == value);
                    var nextOption = options.First(x => x.Value != value);

                    var node = new SudokuChainNode(null, new SudokuLink(startOption, nextOption, true));

                    yield return node;
                }
            }
        }

        protected override SudokuMove? EvaluateChain(SudokuChainNode node, HashSet<(SudokuCell cell, int n)> done, Sudoku sudoku,out bool terminateChain)
        {
            var len = node.Length;
            terminateChain = false;
            // a chain
            if (len % 2 != 1 || len < 3)
                return null;   // end is weak or  chain too short
            var chainStart = node.First;
            var chainEnd = node.Last;
            if (chainStart.Value != chainEnd.Value)
                return null;   // requirement for XY-chain
            var eliminate = sudoku.UnsetCells
                .Where(x =>
                x.PossibleValues.Contains(chainStart.Value) &&
                x.Sees(chainStart.Cell) &&
                x.Sees(chainEnd.Cell) && !node.NodesReversed().Any(y => y.A.Cell == x || y.B.Cell == x))
                .ToArray();
            var move = new SudokuMove($"XY-chain on {chainStart.Value + 1}", GetComplexity(len));

            foreach (var elimCell in eliminate)
            {
                // todo done and op
                if (done.Contains((elimCell, chainStart.Value)))
                    continue;
                move.Operations.Add(new SudokuAction(elimCell, SudokuActionType.RemoveOption, chainStart.Value, "Removed by XY-chain"));
                done.Add((elimCell, chainStart.Value));
            }
            return move;
        }

        protected override IEnumerable<SudokuChainNode> Propagate(SudokuChainNode node)
        {
            if (node.Link.Strong)
            {
                // find weak link
                foreach (var cell in node.Link.B.Cell.VisibleUnset
                    .Where(x => x.PossibleValues.Count == 2 && x.PossibleValues.Contains(node.Link.B.Value) && !x.PossibleValues.Contains(node.Link.A.Value)))
                {
                    if (node.NodesReversed().Any(x => x.B.Cell == cell || x.A.Cell == cell))
                        continue;
                    var option = new SudokuCellOption(cell, node.Link.B.Value);
                    // found a link
                    yield return new SudokuChainNode(node, new SudokuLink(node.Link.B, option, false));
                }
            }
            else
            {
                // find strong link
                if (node.Link.B.Cell.PossibleValues.Count != 2)
                    throw new NotImplementedException();

                var otherValue = node.Link.B.Cell.PossibleValues.First(x => x != node.Link.B.Value);
                var option = new SudokuCellOption(node.Link.B.Cell, otherValue);
                yield return new SudokuChainNode(node, new SudokuLink(node.Link.B, option, true));
            }
        }

        protected override int GetComplexity(SudokuChainNode node) => GetComplexity(node.Length);

        private int GetComplexity(int len) => (int)Math.Max(MinComplexity,Math.Round(Math.Pow(len, 1.5) * MinComplexity/6));
    }
}
