using BlazorSudoku.Techniques;

namespace BlazorSudoku.Generators
{
    public class Intermediate : SimpleIteratorGenerator
    {
        public Intermediate() : base(new SudokuTechnique[]
        {
            new NoMarksInferrence(),
            new EliminateDirect(),
            new DomainForcing(),
            new NakedGroup(3),
            new OnlyOption(),
            new HiddenGroup(2),
        })
        {

        }

        public override int MinComplexity => 25;

       

    }
}
