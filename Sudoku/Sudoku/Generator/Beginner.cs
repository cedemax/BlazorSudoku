using BlazorSudoku.Techniques;

namespace BlazorSudoku.Generators
{
    public class Beginner : SimpleIteratorGenerator
    {
        public Beginner() : base(new SudokuTechnique[]
        {
            new DirectEliminationNoMarks(),
            new OnlyOptionNoTick(),
        })
        {
        }

        public override int MinComplexity => 0;
    }
}
