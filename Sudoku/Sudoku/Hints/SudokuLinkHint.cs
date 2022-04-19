using BlazorSudoku.Techniques.Chain;
using System.Drawing;

namespace BlazorSudoku.Hints
{
    public class SudokuLinkHint : SudokuHint
    {
        public SudokuLink Link { get; set; }
        public Color ColorB { get; }
        public Color ColorA => Color;
        public bool First { get; set; }
        public bool Last { get; set; }

        public IEnumerable<(SudokuCellOption Option, Color Color)> AB => new[] { (Link.A, ColorA), (Link.B, ColorB) };

        public SudokuLinkHint(SudokuLink link, Color colorA, Color colorB) : base(colorA)
        {
            Link = link;
            ColorB = colorB;
        }
    }
}
