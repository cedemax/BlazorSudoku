using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques.Chain
{
    public abstract class ChainTechnique : SudokuTechnique
    {
        protected abstract IEnumerable<SudokuChainNode> Starts(Sudoku sudoku); 

        protected abstract IEnumerable<SudokuChainNode> Propagate(SudokuChainNode node);

        protected abstract SudokuMove? EvaluateChain(SudokuChainNode node, HashSet<(SudokuCell cell, int n)> done, Sudoku sudoku,out bool terminateChain);

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit = int.MaxValue)
        {
            var done = new HashSet<(SudokuCell cell, int n)>();
            var moves = new List<SudokuMove>();

            foreach (var start in Starts(sudoku))
            {
                var nodes = new List<SudokuChainNode> { start };

                do
                {
                    // breadth first search
                    var newNodes = new List<SudokuChainNode>();

                    foreach (var node in nodes)
                    {
                        var newNodes2 = Propagate(node).ToArray();
                        foreach (var newNode in newNodes2)
                        {
                            var move = EvaluateChain(newNode, done, sudoku,out bool terminateChain);

                            if (move != null && !move.IsEmpty)
                            {
                                foreach (var link in newNode.Nodes())
                                    move.Hints.Add(new SudokuLinkHint(link, SudokuHint.Coloring(!link.Strong), SudokuHint.Coloring(link.Strong)));
                                moves.Add(move);
                                if (moves.Count >= limit)
                                    return moves;
                            }
                            else if(!terminateChain)
                            {
                                newNodes.Add(newNode); // try again with a longer chain
                            }
                        }
                    }
                    nodes = newNodes;
                } while (nodes.Count > 0);
            }
            return moves;
        }

    }
}
