using BlazorSudoku.Techniques;

namespace BlazorSudoku
{
    public class Sudoku
    {
        public string Name { get; set; }

        public SudokuCell[] Cells { get; }
        public HashSet<SudokuCell> UnsetCells { get; }

        public SudokuDomain[] Domains { get; }

        public HashSet<SudokuDomain> UnsetDomains { get; }

        public Dictionary<(SudokuDomain A,SudokuDomain B),HashSet<SudokuCell>> DomainIntersections { get; }
        public Dictionary<(SudokuDomain A,SudokuDomain B), HashSet<SudokuCell>> DomainExceptions { get; }

        public int N { get; }
        public int SqrtN { get; }

        public string[] Characters => Enumerable.Range(0, N).Select(x => (x+1).ToString("X")).ToArray();


        public string PID => string.Join(";", Cells.Select(x => x.PID));


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
                            cell.SetValue(vals[2],true);
                        if(vals.Length >= 4 && !cell.Value.HasValue)
                            cell.SetOptions(vals.Skip(3),true);
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
                        cell.SetOptions(Enumerable.Range(0,N),true);
            foreach (var domain in Domains)
                foreach (var cell in domain.Cells)
                    cell.Domains.Add(domain);

            UnsetCells = Cells.Where(x => x.IsUnset).ToHashSet();
            UnsetDomains = Domains.Where(x => x.Cells.Any(x => x.IsUnset)).ToHashSet();

            foreach (var cell in Cells)
                cell.Init();
            foreach (var domain in Domains)
                domain.Init();

            DomainIntersections = new ();
            DomainExceptions = new ();
            foreach (var domainA in Domains)
            {
                foreach(var domainB in domainA.IntersectingDomains)
                {
                    if (domainA == domainB)
                        continue;
                    var keyA = (domainA, domainB);
                    if (!DomainIntersections.TryGetValue(keyA, out var intersection))
                    {
                        var keyB = (domainB, domainA);
                        intersection = domainA.Cells.Intersect(domainB.Cells).ToHashSet();
                        DomainIntersections[keyA] = intersection;
                        DomainIntersections[keyB] = intersection;
                    }
                    DomainExceptions[keyA] = domainA.Cells.Except(domainB.Cells).ToHashSet();
                }
            }

            foreach (var cell in Cells)
                cell.CellBecameSet += (sender, args) => { UnsetCells.Remove(args.Cell); };
            foreach (var domain in Domains)
                domain.DomainBecameSet += (sender, args) => { UnsetDomains.Remove(args.Domain); };
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
            var solver = new Solver(SudokuTechnique.GetAllTechiques().Where(x => x is not Solver && x is not SelectOnlies).OrderBy(x => x.MinComplexity).ToList());
            var sudoku = Clone();
            var hardestMove = 0;
            var moves = new List<SudokuMove>();
            var prevMove = "";
            var maxIters = N * N * N; // removing one option at a time...
            for (var i = 0; i < maxIters; ++i)  // avoid inf-loop
            {
                var move = solver.GetMove(sudoku);
                if (move == null)
                    break;

                var moveName = move.ToString();
                if (prevMove == moveName)
                    break;  // break away from inf-loop
                prevMove = moveName; 
                move.Perform(sudoku);
                hardestMove = Math.Max(hardestMove, move.Complexity);
                moves.Add(move);
            }

            var grade = hardestMove * (2 - Math.Exp(-moves.Sum(x => Math.Sqrt(Math.Max(1,x.Complexity-1)))/10));

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

        public void AdvanceUsing(IEnumerable<SudokuTechnique> techs)
        {
            var solver = new Solver(techs);
            var prevMove = "";
            var maxIters = N * N * N; // removing one option at a time...
            for(var i = 0;i< maxIters; ++i)  // avoid inf-loop
            {
                var move = solver.GetMove(this);
                if (move == null)
                    break;
                var moveName = move.ToString();
                if (prevMove == moveName)
                    break;  // break away from inf-loop
                prevMove = moveName;
                move.Perform(this);
            }
        }

        public IEnumerable<SudokuDomain[]> GetNonOverlappingSets(int size,IEnumerable<SudokuDomain>? domains = null)
        {
            domains ??= Domains.Where(x => x.Unset.Any());

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

        public Sudoku ParseOCRResult(OCRResult res)
        {
            return null;
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
    }
}
