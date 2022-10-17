using BlazorSudoku.Techniques;

namespace BlazorSudoku.Generators
{
    public class Advanced : SimpleIteratorGenerator
    {
        public Advanced() : base(new SudokuTechnique[]
        {
            new DirectEliminationNoMarks(),
            new OnlyOptionNoTick(),
            new EliminateDirect(),
            new DomainForcing(),
            new NakedGroup(3),
            new OnlyOption(),
            new HiddenGroup(2),
            new XYWing(),
            new XYZWing(),
            new SimpleColor(6),
            new Fish(2,false,false),
        })
        {

        }

        public override int MinComplexity => 45;

       

    }
}
