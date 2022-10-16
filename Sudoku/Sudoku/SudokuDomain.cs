using Sudoku.Sudoku.Events;

namespace BlazorSudoku
{
    public class SudokuDomain
    {
        public HashSet<SudokuCell> Cells { get; }

        public bool Error { get; set; }

        public HashSet<SudokuCell> UnsetCells { get; private set; } = new HashSet<SudokuCell>();
        public HashSet<SudokuCell> SetCells { get; private set; } = new HashSet<SudokuCell>();
        public HashSet<int> Unset { get; private set; } = new HashSet<int>();
        public HashSet<int> Set { get; private set; } = new HashSet<int>();

        public HashSet<SudokuDomain> IntersectingDomains { get; private set; } = new HashSet<SudokuDomain>();
        public HashSet<SudokuDomain> IntersectingUnsetDomains { get; private set; } = new HashSet<SudokuDomain>();

        public string Name { get; }

        /// <summary>
        /// The domains possible values changed, but the cell did not become set
        /// </summary>
        public event EventHandler<SudokuDomainEventArgs> UnsetChanged;
        /// <summary>
        /// The domains possible values were reduced to zero
        /// </summary>
        public event EventHandler<SudokuDomainEventArgs> DomainBecameSet;

        public SudokuDomain(HashSet<SudokuCell> cells,string? name = null)
        {
            Cells = cells;
            IsRow = cells.Select(x => x.Y).Distinct().Count() == 1;
            IsCol = cells.Select(x => x.X).Distinct().Count() == 1;

            foreach(var cell in cells)
            {
                cell.CellBecameSet += OnCellBecameSet;
                cell.PossibleValuesChanged += OnPossibleValuesChanged;
            }

            Name = name??ToString();
        }

        public void Init()
        {
            IntersectingDomains = Cells.SelectMany(x => x.Domains).Where(x => x != this && x.Cells.Intersect(Cells).Any()).ToHashSet();
            IntersectingUnsetDomains = IntersectingDomains.Where(x => x.Cells.Any(x => x.IsUnset)).ToHashSet();

            foreach(var intersectingUnsetDomain in IntersectingUnsetDomains)
                intersectingUnsetDomain.DomainBecameSet += (sender,args) => { IntersectingUnsetDomains.Remove(intersectingUnsetDomain); };

            UnsetCells = Cells.Where(x => x.IsUnset).ToHashSet();
            SetCells = Cells.Where(x => x.IsSet).ToHashSet();
            Unset = UnsetCells.SelectMany(x => x.PossibleValues).ToHashSet();
            Set = SetCells.Select(x => x.PossibleValues.First()).ToHashSet();
        }

        private void OnCellBecameSet(object? sender,SudokuCellEventArgs args)
        {
            UnsetCells.Remove(args.Cell);
            SetCells.Add(args.Cell);
            Unset.Remove(args.Cell.PossibleValues.First());
            Set.Add(args.Cell.PossibleValues.First());

            if (Unset.Count == 0)
                DomainBecameSet?.Invoke(this, new SudokuDomainEventArgs(this));
        }

        private void OnPossibleValuesChanged(object? sender, SudokuCellEventArgs args)
        {
            // no use yet
        }

        public bool Overlaps(params SudokuDomain[] others) => others.All(x => x.IntersectingDomains.Contains(this));

        public bool IsRow { get; }
        public bool IsCol { get; }
        public bool IsBox => !IsColOrRow;

        public bool IsColOrRow => IsCol || IsRow;

        public bool IsSet => Unset.Count == 0;
        public bool IsUnset => Unset.Count > 0;

        /// <summary>
        /// returns true if none of the given domains intersect
        /// </summary>
        /// <param name="domains"></param>
        /// <returns></returns>
        public static bool NonOverlapping(params SudokuDomain[] domains)
        {
            for (var i = 0; i < domains.Length; i++)
                for (var j = i + 1; j < domains.Length; ++j)
                    if (domains[i].IntersectingDomains.Contains(domains[j]))
                        return false;
            return true;
        }

        public override string ToString()
        {
            if(Name != null)
                return Name;
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
