using BlazorSudoku.Techniques;

namespace BlazorSudoku.Generators
{
    public class Beginner : SimpleIteratorGenerator
    {
        public Beginner() : base(new SudokuTechnique[]
        {
            new NoMarksInferrence(0),
        })
        {
        }

        public override int MinComplexity => 0;
    }
}
