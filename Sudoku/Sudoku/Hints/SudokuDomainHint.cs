using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuDomainHint : SudokuHint
    {
        public SudokuDomain Domain { get; set; }
        public bool Fill { get; }

        public SudokuDomainHint(SudokuDomain domain, Color color, bool fill = true) : base(color)
        {
            Domain = domain;
            Fill = fill;
        }
    }
}
