using BlazorSudoku.Techniques;
using BlazorSudoku.Techniques.Chain;

namespace BlazorSudoku.Generators
{
    public class GrandMaster : SimpleIteratorGenerator
    {
        public GrandMaster() : base(new SudokuTechnique[]
        {
            new NoMarksInferrence(),
            new EliminateDirect(),
            new DomainForcing(),
            new NakedGroup(),
            new OnlyOption(),
            new HiddenGroup(),
            new XYWing(),
            new XYZWing(),
            new SimpleColor(),
            new Fish(),
            new ContradictionChain(),
            new XYChain(),
            new MultiColor(),
        })
        {

        }

        public override int MinComplexity => 1000;

       

    }
}
