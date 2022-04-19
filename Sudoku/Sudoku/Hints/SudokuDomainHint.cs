using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuDomainHint : SudokuHint
    {
        public SudokuDomain Domain { get; set; }

        public SudokuDomainHint(SudokuDomain domain, Color color) : base(color)
        {
            Domain = domain;
        }
    }
}
