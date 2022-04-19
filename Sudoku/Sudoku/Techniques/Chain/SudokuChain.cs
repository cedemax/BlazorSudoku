namespace BlazorSudoku.Techniques.Chain
{
    public class SudokuChainNode
    {
        public SudokuChainNode? Previous { get; }
        public SudokuLink Link { get; }

        public SudokuChainNode(SudokuChainNode? previous, SudokuLink link)
        {
            Previous = previous;
            Link = link;
        }

        public int Length => 1 + (Previous?.Length ?? 0);

        public SudokuCellOption First => Previous?.First ?? Link.A;
        public SudokuCellOption Last => Link.B;

        public IEnumerable<SudokuLink> NodesReversed()
        {
            yield return Link;
            if(Previous != null)
                foreach(var node in Previous.NodesReversed())
                    yield return node;
        }
        public IEnumerable<SudokuLink> Nodes() => NodesReversed().Reverse();

        public override string ToString()
        {
            return $"{First.Value} {string.Join("", Nodes().Select(x => $" {(x.Strong?"=>":"->")} {x.B.Value}"))}";
        }
    }


}
