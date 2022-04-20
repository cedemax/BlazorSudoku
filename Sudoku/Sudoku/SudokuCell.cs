namespace BlazorSudoku
{
    public class SudokuCell
    {
        public int X { get; }
        public int Y { get; }

        public int? Value { get; private set; }
        public bool Error { get; set; }

        public bool IsSet => PossibleValues.Count == 1;
        public bool IsUnset => !IsSet;

        private HashSet<int> possibleValues = new ();

        public IReadOnlySet<int> PossibleValues => possibleValues;


        public string PID => string.Join("", PossibleValues);

        public HashSet<SudokuDomain> Domains { get; } = new HashSet<SudokuDomain>();

        public int LeftBorder => Domains.Count(x => x.Cells.Any(x => x.X == X - 1 && x.Y == Y )) == 0 ? 2 : 0;
        public int RightBorder => Domains.Count - Domains.Count(x => x.Cells.Any(x => x.X == X+1 && x.Y == Y));
        public int TopBorder => Domains.Count(x => x.Cells.Any(x => x.X == X && x.Y == Y-1)) == 0?2:0;
        public int BottomBorder => Domains.Count - Domains.Count(x => x.Cells.Any(x => x.X == X && x.Y == Y + 1));

        public SudokuCell(int x,int y)
        {
            X = x;
            Y = y;
        }

        public bool Sees(SudokuCell other) => Domains.Intersect(other.Domains).Any();

        public IEnumerable<SudokuCell> Visible => Domains.SelectMany(x => x.Cells).Where(x => x != this).Distinct();

        public IEnumerable<SudokuCell> VisibleUnset => Visible.Where(x => x.IsUnset);

        public IEnumerable<SudokuCell> ConjugatePairs(int val)
        {
            if(IsSet || !PossibleValues.Contains(val))
                return Enumerable.Empty<SudokuCell>();

            return VisibleUnset.Where(x =>
                x.PossibleValues.Contains(val) &&
                x.Domains.Any(y => y.Cells.Contains(this) && y.Cells.Count(z => z.PossibleValues.Contains(val)) == 2));
        }
        public IEnumerable<SudokuCell> ConjugatePairs() => PossibleValues.SelectMany(x => ConjugatePairs(x));




        public void SetValue(int n)
        {

            possibleValues.Clear();
            possibleValues.Add(n);
            Value = n;
        }

        public void SetOption(int n)
        {
            if (IsSet)
                throw new Exception();
            possibleValues.Clear();
            possibleValues.Add(n);
        }

        public void SetOptions(IEnumerable<int> ns)
        {
            if (IsSet)
                throw new Exception();
            possibleValues.Clear();
            foreach (var n in ns)
                possibleValues.Add(n);
        }

        public void RemoveOption(int n)
        {
            if (IsSet)
                throw new Exception();
            possibleValues.Remove(n);
        }

        public void RemoveOptions(IEnumerable<int> ns)
        {
            if (IsSet)
                throw new Exception();
            foreach(var n in ns)
                possibleValues.Remove(n);
        }

        public void RemoveOptions(Func<int,bool> filter)
        {
            if (IsSet)
                throw new Exception();
            possibleValues.RemoveWhere(x => filter(x));
        }

        public override string ToString() => $"X:{X},Y:{Y} {PID}";
    }
}
