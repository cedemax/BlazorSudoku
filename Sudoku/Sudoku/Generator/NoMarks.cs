using BlazorSudoku.Techniques;

namespace BlazorSudoku.Generators
{
    public class NoMarks : SimpleIteratorGenerator
    {
        public NoMarks() : base(new SudokuTechnique[]
        {
            new NoMarksInferrence(),
        })
        {

        }

        public override int MinComplexity => 20;
    }
    public class NoMarks3 : SimpleIteratorGenerator
    {
        public NoMarks3() : base(new SudokuTechnique[]
        {
            new NoMarksInferrence(),
        })
        {

        }

        public override int MinComplexity => 30;
    }
}
