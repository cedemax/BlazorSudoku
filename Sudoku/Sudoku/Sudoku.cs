using BlazorSudoku.Techniques;

namespace BlazorSudoku
{
    public class Sudoku
    {
        public string Name { get; set; }

        public SudokuCell[] Cells { get; }

        public SudokuDomain[] Domains { get; }

        public int N { get; }
        public int SqrtN { get; }


        public string PID => string.Join(";", Cells.Select(x => x.PID));


        public IEnumerable<SudokuCell> UnsetCells => Cells.Where(x => x.IsUnset);

        public string Serialize()
        {
            var index = Cells.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i);
            var cells = Cells.Select(x => $"Cell {x.X},{x.Y},{x.Value?.ToString() ?? "-1"},{string.Join(',',x.PossibleValues)}").ToArray();
            var domains = Domains.Select(x => $"Domain {string.Join(",", x.Cells.Select(y => index[y]))}");

            var sudoku = $"Sudoku {Name??"NoName"} {N} {Cells.Length} cells, {Domains.Length} domains\n" +
                $"{string.Join("\n",cells)}\n" +
                $"{string.Join("\n",domains)}";

            return sudoku;
        }

        public static Sudoku Parse(string saved)
        {
            var cells = new List<SudokuCell>();
            var domains = new List<SudokuDomain>();
            string name = "ParsedSudoku";
            var lines = saved.Split("\n");
            foreach(var line in lines)
            {
                var lineType = line[..line.IndexOf(' ')];
                switch (lineType.ToLowerInvariant())
                {
                    case "sudoku":
                        var things = line.Split(' ').Skip(1).ToArray();
                        name = things[0];
                        break;
                    case "cell":
                        var vals = line.Split(' ').Last().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
                        var cell = new SudokuCell(vals[0], vals[1]);
                        if (vals.Length >= 3 && vals[2] >= 0)
                            cell.SetValue(vals[2]);
                        if(vals.Length >= 4 && !cell.Value.HasValue)
                            cell.SetOptions(vals.Skip(3));
                        cells.Add(cell);
                        break;
                    case "domain":
                        var indices = line.Split(' ').Last().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
                        var domain = new SudokuDomain(cells.Select((x, i) => (x, i)).Where(x => indices.Contains(x.i)).Select(x => x.x).ToHashSet());
                        domains.Add(domain);
                        break;
                }
            }
            return new Sudoku(cells.ToArray(), domains.ToArray())
            {
                Name = name,
            };
        }


        public Sudoku(SudokuCell[] cells, SudokuDomain[] domains)
        {
            if (cells.Select(x => (x.X, x.Y)).Distinct().Count() != cells.Length)
                throw new ArgumentException("Invalid sudoku with overlapping cells");

            if (domains.Length == 0)
                throw new ArgumentException("At least one domain is required");

            N = domains[0].Cells.Count;
            SqrtN = (int)Math.Round(Math.Sqrt(N));

            if (domains.Any(x => x.Cells.Count != N))
                throw new ArgumentException("All domains must have same size");

            Cells = cells;

            Domains = domains;
            foreach (var cell in Cells)
                if(!cell.Value.HasValue && cell.PossibleValues.Count == 0)
                        cell.SetOptions(Enumerable.Range(0,N));
            foreach (var domain in Domains)
                foreach (var cell in domain.Cells)
                    cell.Domains.Add(domain);
        }


        public int IsValid()
        {
            foreach (var domain in Domains)
            {
                var dupes = domain.Cells
                    .Where(x => x.IsSet)
                    .GroupBy(x => x.PossibleValues.First())
                    .Where(x => x.Count() > 1)
                    .SelectMany(x => x).ToArray();
                if (dupes.Any())
                {
                    foreach (var dupe in dupes)
                        dupe.Error = true;
                    domain.Error = true;
                }
            }

            if (Cells.Any(x => !x.Value.HasValue))
                return 0; 

            if(Domains.Any(x => x.Cells.Select(x => x.Value!.Value).ToHashSet().Count != N))
                return -1;

            return 1;
        }


        public void SetValue(SudokuCell cell, int n)
        {
            if (!cell.PossibleValues.Contains(n))
                throw new ArgumentException("Not possible");
            cell.SetValue(n);

          

            var isValid = IsValid();
            //Solve();
        }

        public int Grade(out string moveList)
        {
            var sudoku = Clone();
            var hardestMove = 0;
            var moves = new List<SudokuMove>();
            do
            {
                var move = new Solver().GetMove(sudoku);
                if (move == null)
                    break;
                move.Perform(sudoku);
                hardestMove = Math.Max(hardestMove, move.Complexity);
                moves.Add(move);
            } while (true);

            var grade = hardestMove * (2 - Math.Exp(-moves.Sum(x => Math.Sqrt(x.Complexity-1))/10));

            var prev = "";
            var prevs = 0;
            moveList = "";
            foreach(var move in moves)
            {
                if(prev == move.Name)
                {
                    prevs++;
                }
                else
                {
                    if(prevs > 1)
                    {
                        moveList += $" x{prevs}";
                    }
                    prev = move.Name;
                    prevs = 1;
                    moveList += $" -> {move.Name}";
                }
            }
            if (prevs > 0)
                moveList += $" x{prevs}";

            return (int)Math.Round(grade);
        }


        public void SelectOnlies()
        {
            foreach(var cell in Cells.Where(x => !x.Value.HasValue && x.IsSet))
                cell.SetValue(cell.PossibleValues.First());

            var isValid = IsValid();
        }



    
      
        public IEnumerable<SudokuDomain[]> GetNonOverlappingSets(int size,IEnumerable<SudokuDomain> domains = null)
        {
            domains ??= Domains;
            foreach(var group in domains.GetCombinations(size))
            {
                if(SudokuDomain.NonOverlapping(group))
                    yield return group;
            }
        }



        public Sudoku Clone()
        {
            return Parse(Serialize());
        }

        public static Sudoku Hard9x9()
        {
            var sudoku = StandardNxN(3);

            for(var row = 0; row < 9; ++row)
            {
                for (var col = 0; col < 9; ++col)
                {
                    var cell = sudoku.Cells.First(x => x.X == col && x.Y == row);
                    if (HardVals[row][col].HasValue)
                        cell.SetValue(HardVals[row][col]!.Value-1);
                }
            }
            return sudoku;
        }


        public static Sudoku StandardNxN(int n = 3)
        {
            var N = n * n;

            var cells = Enumerable.Range(0, N).SelectMany(x => Enumerable.Range(0, N).Select(y => new SudokuCell(x, y))).ToArray();
            var domains = new List<SudokuDomain>();


            for (var i = 0; i < N; i++)
            {
                domains.Add(new SudokuDomain(cells.Where(x => x.X == i).ToHashSet()));  // columns
                domains.Add(new SudokuDomain(cells.Where(x => x.Y == i).ToHashSet()));  // rows
                domains.Add(new SudokuDomain(cells.Where(x => ((x.X / n) * n + (x.Y / n)) == i).ToHashSet()));  // squares
            }
            return new Sudoku(cells, domains.ToArray());
        }


        public static int?[][] HardVals = 
            new int?[9][]{
                new int?[9]{ null,8,4,null,null,null,null,7,null},
                new int?[9]{ 7,null,null,null,null,null,null,3,null},
                new int?[9]{ null,null,2,6,null,8,null,null,null},
                            
                new int?[9]{ 8,null,null,null,null,null,1,null,null},
                new int?[9]{ null,null,9,null,5,1,null,null,3},
                new int?[9]{ null,4,null,null,null,null,2,9,null},
                            
                new int?[9]{ null,null,1,null,null,7,9,null,null},
                new int?[9]{ null,null,null,null,null,4,null,null,null},
                new int?[9]{ null,null,6,3,2,null,null,null,null},
            };
    }
}
