using BlazorSudoku.Techniques;
using BlazorSudoku.Techniques.Chain;

namespace BlazorSudoku.Generators
{
    public class Master : SimpleIteratorGenerator
    {
        public Master() : base(new SudokuTechnique[]
        {
            new NoMarksInferrence(),
            new EliminateDirect(),
            new DomainForcing(),
            new NakedGroup(4),
            new OnlyOption(),
            new HiddenGroup(3),
            new XYWing(),
            new XYZWing(),
            new SimpleColor(12),
            new Fish(3,true,true),
            new ContradictionChain(),
            new XYChain(),
            new MultiColor(),
        })
        {

        }

        public override int MinComplexity => 110;

       

    }
}
