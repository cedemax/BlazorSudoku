namespace BlazorSudoku
{
    public class SudokuDomain : WithID
    {
        /// <summary>
        /// The cells that make up this domain
        /// </summary>
        public BASet<SudokuCell> Cells { get; }
        /// <summary>
        /// Cells in intersecting domains
        /// </summary>
        public BASet<SudokuCell> SeenCells { get; }
        /// <summary>
        /// Intersecting domains
        /// </summary>
        public BASet<SudokuDomain> IntersectingDomains { get; }

        public bool Error { get; set; }

        /// <summary>
        /// Temporary field for use in computations to avoid creation of dictionaries
        /// </summary>
        public int TempField { get; set;}  

        /// <summary>
        /// A unique ID
        /// </summary>
        public int Key { get; }
        /// <summary>
        /// A reference to the parent sudoku
        /// </summary>
        public Sudoku Sudoku { get; }

        /// <summary>
        /// References the cells that have more than one option remaining (marks)
        /// </summary>
        public BARefSet<SudokuCell> UnsetCellRefs { get; }
        /// <summary>
        /// References the cells that are not yet locked by the player
        /// </summary>
        public BARefSet<SudokuCell> UnlockedCellRefs { get; }

        /// <summary>
        /// References the cells that have only one option remaining (marks)
        /// </summary>
        public BARefSet<SudokuCell> SetCellRefs { get; }
        /// <summary>
        /// References the cells that have been locked by the player
        /// </summary>
        public BARefSet<SudokuCell> LockedCellRefs { get; }
        public BARefSet<SudokuCell>[] PossibleValueRefs{ get; }

        public IEnumerable<SudokuCell> UnsetCells => Sudoku.GetCells(UnsetCellRefs);
        public IEnumerable<SudokuCell> SetCells => Sudoku.GetCells(SetCellRefs);
        public IEnumerable<SudokuCell> UnlockedCells => Sudoku.GetCells(UnlockedCellRefs);
        public IEnumerable<SudokuCell> LockedCells => Sudoku.GetCells(LockedCellRefs);

        private Set32 unset;
        private Set32 set;
        public ref Set32 Unset => ref unset;
        public ref Set32 Set => ref set;


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
            UnlockedCellRefs = new BARefSet<SudokuCell>(cellCount);
            LockedCellRefs = new BARefSet<SudokuCell>(cellCount);

            PossibleValueRefs = Enumerable.Range(0,sudoku.N).Select(x => new BARefSet<SudokuCell>(cellCount)).ToArray();

            IntersectingDomains = new BASet<SudokuDomain>(domainCount);
            SeenCells = new BASet<SudokuCell>(cellCount);

            foreach (var cell in cells)
                Cells.Add(cell);

            Key = key;
            Sudoku = sudoku;
            IsRow = cells.Select(x => x.Y).Distinct().Count() == 1;
            IsCol = cells.Select(x => x.X).Distinct().Count() == 1;

            foreach (var cell in cells)
            {
                cell.CellBecameSet += OnCellBecameSet;
                cell.CellBecameLocked += OnCellBecameLocked;
                cell.CellBecameUnlocked += OnCellBecameUnlocked;
                cell.CellBecameUnSet += OnCellBecameUnSet;
                cell.PossibleValuesChanged += OnPossibleValuesChanged;
            }

            Unset = Set32.All(sudoku.N);
            Set = Set32.Empty;
        }

        public void Init()
        {
            foreach (var domain in Sudoku.GetDomains(Cells.Select(x => x.Domains).Union().Without(this)))
            {
                IntersectingDomains.Add(domain);
                foreach(var cell in domain.Cells.Where(x => !Cells.Contains(x)))
                    SeenCells.Add(cell);
            }


            foreach (var cell in Cells)
            {
                if (cell.IsSet)
                    SetCellRefs.Add(cell);
                else
                    UnsetCellRefs.Add(cell);

                if (cell.Value.HasValue)
                    LockedCellRefs.Add(cell);
                else
                    UnlockedCellRefs.Add(cell);
            }
            UpdateSetUnset(null);
        }

        private void UpdateSetUnset(SudokuCell? cell)
        {
            Unset = UnsetCells.Select(x => x.PossibleValues).Union();
            Set = SetCells.Select(x => x.PossibleValues).Union();
            Unset.ExceptWith(Set);

            if(cell is not null)
            {
                for (var i = 0; i < PossibleValueRefs.Length; ++i)
                    if (cell.PossibleValues.Contains(i))
                        PossibleValueRefs[i].Add(cell);
                    else
                        PossibleValueRefs[i].Remove(cell);
            }
            else
            {
                for (var i = 0; i < PossibleValueRefs.Length; ++i)
                    foreach(var c in Cells)
                        if (c.PossibleValues.Contains(i))
                            PossibleValueRefs[i].Add(c);
                        else
                            PossibleValueRefs[i].Remove(c);
            }
        }

        private void OnCellBecameLocked(object? sender, SudokuCellEventArgs args)
        {
            UnlockedCellRefs.Remove(args.Cell);
            LockedCellRefs.Add(args.Cell);
        }
        private void OnCellBecameUnlocked(object? sender, SudokuCellEventArgs args)
        {
            UnlockedCellRefs.Add(args.Cell);
            LockedCellRefs.Remove(args.Cell);
        }
        private void OnCellBecameSet(object? sender, SudokuCellEventArgs args)
        {
            UnsetCellRefs.Remove(args.Cell);
            SetCellRefs.Add(args.Cell);
            UpdateSetUnset(args.Cell);


            if (Unset.Count == 0)
                DomainBecameSet?.Invoke(this, new SudokuDomainEventArgs(this));
        }

        private void OnCellBecameUnSet(object? sender, SudokuCellEventArgs args)
        {
            UnsetCellRefs.Add(args.Cell);
            SetCellRefs.Remove(args.Cell);
            UpdateSetUnset(args.Cell);

            DomainBecameUnSet?.Invoke(this, new SudokuDomainEventArgs(this));
        }

        private void OnPossibleValuesChanged(object? sender, SudokuCellEventArgs args)
        {
            UpdateSetUnset(args.Cell);
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
            var refs = BAPool.Get<SudokuCell>(domains[0].Sudoku.Cells.Length);
            var cntr = 0;
            for (var i = 0; i < domains.Length; i++) 
            {
                refs.UnionWith(domains[i].Cells);
                cntr += domains[i].Cells.Count;
            }
            return refs.CountTrue() == cntr;

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
                return $"Row {Cells.First().Y}";

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
