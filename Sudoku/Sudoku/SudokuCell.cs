using Sudoku.Sudoku.Events;

namespace BlazorSudoku
{
    public class SudokuCell
    {
        public int X { get; }
        public int Y { get; }

        public int? Value { get; private set; }
        public bool Error { get; set; }


        /// <summary>
        /// The cells possible values changed, but the cell did not become set
        /// </summary>
        public event EventHandler<SudokuCellEventArgs>? PossibleValuesChanged;
        /// <summary>
        /// The cells possible values were reduced to one
        /// </summary>
        public event EventHandler<SudokuCellEventArgs>? CellBecameSet;


        public bool IsSet => PossibleValues.Count == 1;
        public bool IsUnset => !IsSet;

        private Set32 possibleValues;

        public ref Set32 PossibleValues => ref possibleValues;

        /// <summary>
        /// a key describing the possible values
        /// </summary>
        public uint PID => PossibleValues.Flag;

        public HashSet<SudokuDomain> Domains { get; } = new HashSet<SudokuDomain>();

        /// <summary>
        /// Lazy loading
        /// </summary>
        private readonly int[] border = new int[4] { -1, -1, -1, -1 };
        private int GetBorder(int i)
        {
            if (border[i] < 0)
                border[i] = i switch
                {
                    0 => !Domains.Any(x => x.Cells.Any(x => x.X == X - 1 && x.Y == Y)) ? 2 : 0,
                    1 => Domains.Count - Domains.Count(x => x.Cells.Any(x => x.X == X + 1 && x.Y == Y)),
                    2 => !Domains.Any(x => x.Cells.Any(x => x.X == X && x.Y == Y - 1)) ? 2 : 0,
                    3 => Domains.Count - Domains.Count(x => x.Cells.Any(x => x.X == X && x.Y == Y + 1)),
                    _ => throw new NotImplementedException(),
                };
            return border[i];
        }

        public int LeftBorder => GetBorder(0);
        public int RightBorder => GetBorder(1);
        public int TopBorder => GetBorder(2);
        public int BottomBorder => GetBorder(3);

        public SudokuCell(int x,int y)
        {
            X = x;
            Y = y;

            PossibleValues = Set32.Empty;
        }
        /// <summary>
        /// Other Cells that are visible from this cell
        /// </summary>
        public HashSet<SudokuCell> Visible { get; private set; } = new HashSet<SudokuCell>();

        /// <summary>
        /// Unset Cells that are visible from this cell
        /// </summary>
        public HashSet<SudokuCell> VisibleUnset { get; private set; } = new HashSet<SudokuCell>();

        public void Init()
        {
            Visible = Domains.SelectMany(x => x.Cells).Where(x => x != this).ToHashSet();
            VisibleUnset = Visible.Where(x => x.IsUnset).ToHashSet();
            foreach (var cell in VisibleUnset)
                cell.CellBecameSet += (sender, args) => { VisibleUnset.Remove(args.Cell); };
        }

        public bool Sees(SudokuCell other) => Visible.Contains(other);


        public IEnumerable<SudokuCell> ConjugatePairs(int val)
        {
            if(IsSet || !PossibleValues.Contains(val))
                return Enumerable.Empty<SudokuCell>();

            return VisibleUnset.Where(x =>
                x.PossibleValues.Contains(val) &&
                x.Domains.Any(y => y.Cells.Contains(this) && y.Cells.Count(z => z.PossibleValues.Contains(val)) == 2));
        }
        public IEnumerable<SudokuCell> ConjugatePairs() => PossibleValues.SelectMany(x => ConjugatePairs(x));


        public void SetValue(int n,bool force = false)
        {
            if (force)
            {
                Value = n;
                PossibleValues.Clear();
                PossibleValues.Add(n);
            }

            if (!PossibleValues.Contains(n))
                throw new ArgumentException("Not possible");
            Value = n;

            if (PossibleValues.Count != 1)
            {
                PossibleValues.Clear();
                PossibleValues.Add(n);
                CellBecameSet?.Invoke(this, new SudokuCellEventArgs(this));
            }
        }

        public void SetOption(int n)
        {
            if (IsSet)
                throw new Exception("Already set");
            if (!PossibleValues.Contains(n))
                throw new ArgumentException("Not possible");
            PossibleValues.Clear();
            PossibleValues.Add(n);
            CellBecameSet?.Invoke(this, new SudokuCellEventArgs(this));
        }

        public void SetOptions(IEnumerable<int> ns,bool force = false)
        {
            if (force)
            {
                PossibleValues.Clear();
                foreach (var n in ns)
                    PossibleValues.Add(n);
                return;
            }

            if (IsSet)
                throw new Exception();


            var arr = ns.ToHashSet();
            if(arr.Count == 1)
            {
                SetOption(arr.First());
                return;
            }

            if (!arr.All(x => PossibleValues.Contains(x)))
                throw new Exception("Not possible");

            PossibleValues.Clear();
            foreach (var n in ns)
                PossibleValues.Add(n);
            PossibleValuesChanged?.Invoke(this, new SudokuCellEventArgs(this));
        }

        public void RemoveOption(int n)
        {
            if (IsSet)
                throw new Exception();
            if (PossibleValues.Contains(n))
            {
                PossibleValues.Remove(n);
                if (PossibleValues.Count == 1)
                    CellBecameSet?.Invoke(this, new SudokuCellEventArgs(this));
                else
                    PossibleValuesChanged?.Invoke(this, new SudokuCellEventArgs(this));
            }
        }

        public void RemoveOptions(IEnumerable<int> ns)
        {
            if (IsSet)
                throw new Exception();
            var arr = ns.ToHashSet();
            if (arr.Count == 1)
            {
                RemoveOption(arr.First());
                return;
            }
            var count = PossibleValues.Count;
            foreach (var n in ns)
                PossibleValues.Remove(n);
            if(PossibleValues.Count != count)
            {
                if (PossibleValues.Count == 1)
                    CellBecameSet?.Invoke(this, new SudokuCellEventArgs(this));
                else
                    PossibleValuesChanged?.Invoke(this, new SudokuCellEventArgs(this));
            }
           
        }
        public void RemoveOptions(Func<int,bool> filter)
        {
            if (IsSet)
                throw new Exception();
            PossibleValues.RemoveWhere(x => filter(x));
            var count = PossibleValues.Count;
            if (PossibleValues.Count != count)
            {
                if (PossibleValues.Count == 1)
                    CellBecameSet?.Invoke(this, new SudokuCellEventArgs(this));
                else
                    PossibleValuesChanged?.Invoke(this, new SudokuCellEventArgs(this));
            }
        }
        public override string ToString() => $"X:{X},Y:{Y} '{Convert.ToString(PID,2)}'";
    }
}
