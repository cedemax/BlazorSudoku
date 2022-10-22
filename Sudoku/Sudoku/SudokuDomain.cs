using Sudoku.Sudoku.Events;

namespace BlazorSudoku
{
    public class SudokuDomain
    {
        public HashSet<SudokuCell> Cells { get; }

        public bool Error { get; set; }

        /// <summary>
        /// A unique ID
        /// </summary>
        public int Key { get; }


        public HashSet<SudokuCell> UnsetCells { get; private set; } = new HashSet<SudokuCell>();
        public HashSet<SudokuCell> SetCells { get; private set; } = new HashSet<SudokuCell>();
        public Set32 Unset { get; private set; }
        public Set32 Set { get; private set; }

        public HashSet<SudokuDomain> IntersectingDomains { get; private set; } = new HashSet<SudokuDomain>();
        public HashSet<SudokuDomain> IntersectingUnsetDomains { get; private set; } = new HashSet<SudokuDomain>();

        /// <summary>
        /// The domains possible values changed, but the cell did not become set
        /// </summary>
        public event EventHandler<SudokuDomainEventArgs>? UnsetChanged;
        /// <summary>
        /// The domains possible values were reduced to zero
        /// </summary>
        public event EventHandler<SudokuDomainEventArgs>? DomainBecameSet;

        /// <summary>
        /// The domains possible values were changed
        /// </summary>
        public event EventHandler<SudokuDomainEventArgs>? DomainBecameUnSet;

        public SudokuDomain(HashSet<SudokuCell> cells,int key)
        {
            Cells = cells;
            Key = key;
            IsRow = cells.Select(x => x.Y).Distinct().Count() == 1;
            IsCol = cells.Select(x => x.X).Distinct().Count() == 1;

            foreach(var cell in cells)
            {
                cell.CellBecameSet += OnCellBecameSet;
                cell.CellBecameUnSet += OnCellBecameUnSet;
                cell.PossibleValuesChanged += OnPossibleValuesChanged;
            }

            Unset = Set32.Empty;
            Set = Set32.Empty;
        }

        public void Init()
        {
            IntersectingDomains = Cells.SelectMany(x => x.Domains).Where(x => x != this && x.Cells.Intersect(Cells).Any()).ToHashSet();
            IntersectingUnsetDomains = IntersectingDomains.Where(x => x.Cells.Any(x => x.IsUnset)).ToHashSet();

            foreach(var intersectingUnsetDomain in IntersectingUnsetDomains)
                intersectingUnsetDomain.DomainBecameSet += (sender,args) => { IntersectingUnsetDomains.Remove(intersectingUnsetDomain); };

            UnsetCells = Cells.Where(x => x.IsUnset).ToHashSet();
            SetCells = Cells.Where(x => x.IsSet).ToHashSet();
            Unset = UnsetCells.Select(x => x.PossibleValues).Union();
            Set = SetCells.Select(x => x.PossibleValues).Union();
        }

        private void OnCellBecameSet(object? sender,SudokuCellEventArgs args)
        {
            UnsetCells.Remove(args.Cell);
            SetCells.Add(args.Cell);
            Unset = UnsetCells.Select(x => x.PossibleValues).Union();
            Set = SetCells.Select(x => x.PossibleValues).Union();

            if (Unset.Count == 0)
                DomainBecameSet?.Invoke(this, new SudokuDomainEventArgs(this));
        }

        private void OnCellBecameUnSet(object? sender, SudokuCellEventArgs args)
        {
            UnsetCells.Add(args.Cell);
            SetCells.Remove(args.Cell);
            Unset = UnsetCells.Select(x => x.PossibleValues).Union();
            Set = SetCells.Select(x => x.PossibleValues).Union();

            DomainBecameUnSet?.Invoke(this, new SudokuDomainEventArgs(this));
        }

        private void OnPossibleValuesChanged(object? sender, SudokuCellEventArgs args)
        {
            Unset = UnsetCells.Select(x => x.PossibleValues).Union();
            Set = SetCells.Select(x => x.PossibleValues).Union();
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
