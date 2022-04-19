namespace BlazorSudoku
{
    public class SudokuDomain
    {

        public HashSet<SudokuCell> Cells { get; }

        public bool Error { get; set; }

        public IEnumerable<SudokuCell> UnsetCells => Cells.Where(x => x.IsUnset);
        public IEnumerable<SudokuCell> SetCells => Cells.Where(x => x.IsSet);
        public HashSet<int> Unset => UnsetCells.SelectMany(x => x.PossibleValues).ToHashSet();
        public HashSet<int> Set => SetCells.Select(x => x.PossibleValues.Single()).ToHashSet();

        public SudokuDomain(HashSet<SudokuCell> cells)
        {
            Cells = cells;
            IsRow = cells.Select(x => x.Y).Distinct().Count() == 1;
            IsCol = cells.Select(x => x.X).Distinct().Count() == 1;
        }

        public bool Overlaps(params SudokuDomain[] others) => others.All(x => x.Cells.Intersect(Cells).Any());

        public bool IsRow { get; }
        public bool IsCol { get; }
        public bool IsBox { get; }

        public bool IsColOrRow => IsCol || IsRow;

        /// <summary>
        /// returns true if all domains given are non overlapping
        /// </summary>
        /// <param name="domains"></param>
        /// <returns></returns>
        public static bool NonOverlapping(params SudokuDomain[] domains)
        {
            return domains.SelectMany(x => x.Cells).ToHashSet().Count == domains.Sum(x => x.Cells.Count);
        }

        public override string ToString()
        {
            if (IsCol)
                return $"Col {Cells.First().X}";
            if (IsRow)
                return $"Row {Cells.First().X}";

            var xmin = Cells.Min(x => x.X);
            var ymin = Cells.Min(x => x.Y);
            var xmax = Cells.Max(x => x.X);
            var ymax = Cells.Max(x => x.Y);

            var x = xmin == xmax ? $"Col {xmin}" : (xmax - xmin == Cells.Count - 1) ? "" : $"Cols {xmin}-{xmax}";
            var y = ymin == ymax ? $"Row {ymin}" : (ymax - ymin == Cells.Count - 1) ? "" : $"Rows {ymin}-{ymax}";
            return $"{x} {y}";
        }

    }
}
