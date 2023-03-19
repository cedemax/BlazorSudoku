using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuStepHint : SudokuHint
    {
        public int Generation { get; set; }
        public List<SudokuHint> Hints { get; set; }

        public override IEnumerable<SudokuHint> AllHints => Hints.SelectMany(x => x.AllHints);

        public SudokuStepHint(List<SudokuHint> hints,int generation) : base(Direct)
        {
            Hints = hints.ToList();
            Generation = generation;
        }
    }
}
