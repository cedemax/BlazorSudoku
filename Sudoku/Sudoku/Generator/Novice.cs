using BlazorSudoku.Techniques;

namespace BlazorSudoku.Generators
{
    public class Novice : SimpleIteratorGenerator
    {
        public Novice() : base(new SudokuTechnique[]
        {
            new DirectEliminationNoMarks(),
            new OnlyOptionNoTick(),
            new EliminateDirect(),
            new DomainForcing(),
            new NakedGroup(2),
        })
        {

        }

        public override int MinComplexity => 12;

       

    }
}
