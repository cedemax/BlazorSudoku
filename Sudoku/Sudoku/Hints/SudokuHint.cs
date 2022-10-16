using System.Drawing;

namespace BlazorSudoku.Hints
{
    public abstract class SudokuHint
    {
        public Color Color { get; set; }

        protected SudokuHint(Color color)
        {
            Color = color;
        }

        public static Color Fin => Color.FromArgb(66, 100, 100,0);
        public static Color Direct => Color.FromArgb(66, 0, 100, 100);
        public static Color Elimination => Color.FromArgb(66, 100, 0, 0);
        public static Color Base => Color.FromArgb(66, 0, 0, 100);
        public static Color Cover => Color.FromArgb(66, 0, 100, 0);
        public static Color Coloring(bool v) => Color.FromArgb(66, v ? 100 : 200, v?200:100, 0);
        public static Color Coloring(int v)
        {
            var vb = v > 0;
            return Math.Abs(v) switch
            {
                1 => Color.FromArgb(66, 100, vb ? 200 : 50, 0),
                2 => Color.FromArgb(66, 0, 100, vb ? 200 : 50),
                3 => Color.FromArgb(66, 100, 0, vb ? 200 : 50),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
