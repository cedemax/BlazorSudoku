namespace BlazorSudoku.Techniques.Chain
{
    public class ContradictionChain : ChainTechnique
    {
        public override int MinComplexity => 36;


        protected override IEnumerable<SudokuChainNode> Starts(Sudoku sudoku)
        {
            foreach (var start in sudoku.UnsetCells)
            {
                var options = start.PossibleValues.Select(x => new SudokuCellOption(start, x)).ToArray();
                // internal strong link
                if(start.PossibleValues.Count == 2)
                {
                    foreach (var value in start.PossibleValues)
                    {
                        var startOption = options.First(x => x.Value == value);
                        var nextOption = options.First(x => x.Value != value);

                        var node = new SudokuChainNode(null, new SudokuLink(startOption, nextOption, true));
                        yield return node;
                    }
                }
                else if(start.PossibleValues.Count > 2)
                {
                    // find external weak or strong
                    foreach (var value in start.PossibleValues)
                    {
                        var startOption = options.First(x => x.Value == value);
                        foreach(var cell in start.VisibleUnset.Where(x => x.PossibleValues.Count == 2 && x.PossibleValues.Contains(value)))
                        {
                            // external weak
                            var nextOption = new SudokuCellOption(cell, value);
                            var node = new SudokuChainNode(null, new SudokuLink(startOption, nextOption, false));
                            yield return node;
                        }
                        foreach (var domain in start.Domains)
                        {
                            var others = domain.UnsetCells.Where(x => x != start && x.PossibleValues.Contains(value)).ToArray();

                            if (others.Length == 1)
                            {
                                // strong external link
                                var onlyOther = others[0];
                                var nextOption = new SudokuCellOption(onlyOther, value);
                                var node = new SudokuChainNode(null, new SudokuLink(startOption, nextOption, true));
                                yield return node;
                            }
                        }
                    }
                }
            }
        }

        protected override SudokuMove? EvaluateChain(SudokuChainNode node, HashSet<(SudokuCell cell, int n)> done, Sudoku sudoku,out bool terminateChain)
        {
            var len = node.Length;
            terminateChain = false;
            // a chain
            if (len < 3)
                return null;   // too short
            var start = node.First;
            var last = node.Last;

            var selfReferences = node.NodesReversed().Skip(1)
                .Where(x =>
                    x.A.Cell == node.Link.B.Cell  // affect same cell
                ).ToArray();

            terminateChain = selfReferences.Length > 0;
            if (selfReferences.Any(x => x.A.Value == node.Link.B.Value))
                return null;    // no loops allowed

            var contradiction = selfReferences
                .FirstOrDefault(x => 
                    (
                        x.A.Value == node.Link.B.Value &&  // affects same option
                        x.Strong == node.Link.Strong        // same strength -> contradiction
                    ) 
                    ||       
                    (
                        x.A.Value != node.Link.B.Value &&   // affects different option
                        x.Weak && node.Link.Strong         // The node would have two different values
                    )
                    ); 

            if (contradiction != null)
            {
                if (done.Contains((node.Link.B.Cell, node.Link.B.Value)))
                    return null;

                var name = contradiction.A.Equals(node.First)
                    ? "Discontinuous Nice Loop"
                    : "Contradiction Chain";
                var move = new SudokuMove(name, (int)Math.Round(Math.Pow(len, 1.5) * 8));

                move.Operations.Add(new SudokuAction(node.Link.B.Cell, SudokuActionType.RemoveOption, start.Value, "Removed by contradiction"));
                done.Add((node.Link.B.Cell, node.Link.B.Value));

                terminateChain = true;
                return move;
            }
            else
            {
                // try for AIC
                if (start.Value != last.Value || node.NodesReversed().Last().Weak || node.Link.Weak)
                    return null;   // requirement for AIC
                var eliminate = sudoku.Cells
                    .Where(x =>
                    x.IsUnset &&
                    x.PossibleValues.Contains(start.Value) &&
                    x.Sees(start.Cell) &&
                    x.Sees(last.Cell) && !node.NodesReversed().Any(y => y.A.Cell == x || y.B.Cell == x))
                    .ToArray();
                var move = new SudokuMove($"Alternate Inference Chain on {start.Value + 1}", (int)Math.Round(Math.Pow(len, 1.5) * 8));

                foreach (var elimCell in eliminate)
                {
                    // todo done and op
                    if (done.Contains((elimCell, start.Value)))
                        continue;
                    move.Operations.Add(new SudokuAction(elimCell, SudokuActionType.RemoveOption, start.Value, "Removed by XY-chain"));
                    done.Add((elimCell, start.Value));
                }
                return move;
            }



            return null;    // not contradictory
        }

        protected override IEnumerable<SudokuChainNode> Propagate(SudokuChainNode node)
        {
            if (node.Link.Strong)
            {
                // find weak link
                foreach (var cell in node.Link.B.Cell.VisibleUnset
                    .Where(x => x.PossibleValues.Count == 2 && x.PossibleValues.Contains(node.Link.B.Value) && !x.PossibleValues.Contains(node.Link.A.Value)))
                {
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

                foreach(var domain in node.Link.B.Cell.Domains)
                {
                    var others = domain.UnsetCells.Where(x => x != node.Link.B.Cell && x.PossibleValues.Contains(node.Link.B.Value)).ToArray();

                    if (others.Length == 1)
                    {
                        var onlyOther = others[0];
                        // strong external link
                        var externalOption = new SudokuCellOption(onlyOther, node.Link.B.Value);
                        yield return new SudokuChainNode(node, new SudokuLink(node.Link.B, externalOption, true));
                    }
                }
            }
        }
    }
}
