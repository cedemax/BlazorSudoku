using Sudoku.Sudoku.Events;
using Sudoku.Sudoku.HashSet;

namespace BlazorSudoku
{
    public class SudokuDomain : WithID
    {
        /// <summary>
        /// The cells that make up this domain
        /// </summary>
        public BASet<SudokuCell> Cells { get; }

        public bool Error { get; set; }

        /// <summary>
        /// A unique ID
        /// </summary>
        public int Key { get; }
        /// <summary>
        /// A reference to the parent sudoku
        /// </summary>
        public Sudoku Sudoku { get; }

        public BARefSet<SudokuCell> UnsetCellRefs { get; }
        public BARefSet<SudokuCell> SetCellRefs { get; }

        public IEnumerable<SudokuCell> UnsetCells => Sudoku.GetCells(UnsetCellRefs);
        public IEnumerable<SudokuCell> SetCells => Sudoku.GetCells(SetCellRefs);

        public Set32 Unset { get; private set; }
        public Set32 Set { get; private set; }

        public BASet<SudokuDomain> IntersectingDomains { get; }
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

        public SudokuDomain(IEnumerable<SudokuCell> cells, int key, Sudoku sudoku, int domainCount, int cellCount)
        {
            Cells = new BASet<SudokuCell>(cellCount);
            UnsetCellRefs = new BARefSet<SudokuCell>(cellCount);
            SetCellRefs = new BARefSet<SudokuCell>(cellCount);
            IntersectingDomains = new BASet<SudokuDomain>(domainCount);

            foreach (var cell in cells)
                Cells.Add(cell);

            Key = key;
            Sudoku = sudoku;
            IsRow = cells.Select(x => x.Y).Distinct().Count() == 1;
            IsCol = cells.Select(x => x.X).Distinct().Count() == 1;

            foreach (var cell in cells)
            {
                cell.CellBecameSet += OnCellBecameSet;
                cell.CellBecameUnSet += OnCellBecameUnSet;
                cell.PossibleValuesChanged += OnPossibleValuesChanged;
            }

            Unset = Set32.All(sudoku.N);
            Set = Set32.Empty;
        }

        public void Init()
        {
            foreach (var domain in Sudoku.GetDomains(Cells.Select(x => x.Domains).Union().Without(this)))
                IntersectingDomains.Add(domain);

            IntersectingUnsetDomains = IntersectingDomains.Where(x => x.Cells.Any(x => x.IsUnset)).ToHashSet();

            foreach (var intersectingDomain in IntersectingDomains)
            {
                intersectingDomain.DomainBecameSet += (sender, args) => { IntersectingUnsetDomains.Remove(intersectingDomain); };
                intersectingDomain.DomainBecameUnSet += (sender, args) => { IntersectingUnsetDomains.Add(intersectingDomain); };
            }

            Unset = Set32.All(Sudoku.N);
            Set = Set32.Empty;
            foreach (var cell in Cells)
            {
                if (cell.IsSet)
                {
                    SetCellRefs.Add(cell);
                    Unset.ExceptWith(cell.PossibleValues);
                    Set.UnionWith(cell.PossibleValues);
                }
                else
                {
                    UnsetCellRefs.Add(cell);
                    Unset.UnionWith(cell.PossibleValues);
                }
            }
        }

        private void OnCellBecameSet(object? sender, SudokuCellEventArgs args)
        {
            UnsetCellRefs.Remove(args.Cell);
            SetCellRefs.Add(args.Cell);
            Unset = UnsetCells.Select(x => x.PossibleValues).Union();
            Set = SetCells.Select(x => x.PossibleValues).Union();

            if (Unset.Count == 0)
                DomainBecameSet?.Invoke(this, new SudokuDomainEventArgs(this));
        }

        private void OnCellBecameUnSet(object? sender, SudokuCellEventArgs args)
        {
            UnsetCellRefs.Add(args.Cell);
            SetCellRefs.Remove(args.Cell);
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
