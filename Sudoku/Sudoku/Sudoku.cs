using BlazorSudoku.Data;
using BlazorSudoku.Generators;
using BlazorSudoku.Techniques;
using System.Collections;

namespace BlazorSudoku
{
    public class Sudoku
    {
        public string Name { get; set; }

        public SudokuCell[] Cells { get; }
        public BARefSet<SudokuCell> UnsetCells { get; }

        public SudokuDomain[] Domains { get; }

        public HashSet<SudokuDomain> UnsetDomains { get; }

        public Dictionary<(SudokuDomain A,SudokuDomain B),HashSet<SudokuCell>> DomainIntersections { get; }
        public Dictionary<(SudokuDomain A,SudokuDomain B), HashSet<SudokuCell>> DomainExceptions { get; }

        public int N { get; }
        public int SqrtN { get; }


        public string PID => string.Join(";", Cells.Select(x => x.PID));


        public string Serialize(bool options = true)
        {
            var index = Cells.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => x.i);
            var cells = options
                ? Cells.Select(x => $"Cell {x.X},{x.Y},{x.Value?.ToString() ?? "-1"},{string.Join(',', x.PossibleValues)}").ToArray()
                : Cells.Select(x => $"Cell {x.X},{x.Y},{x.Value?.ToString() ?? "-1"}").ToArray();
            var domains = Domains.Select(x => $"Domain {string.Join(",", x.Cells.Select(y => index[y]))}");

            var sudoku = $"Sudoku {Name?.Replace(' ','_')??"NoName"} {N} {Cells.Length} {Domains.Length}\n" +
                $"{string.Join("\n",cells)}\n" +
                $"{string.Join("\n",domains)}";

            return sudoku;
        }

        public static Sudoku Parse(string saved)
        {
            var lines = saved.Split("\n");
            var header = lines[0].Split(' ');
            if (header[0] != "Sudoku")
                throw new InvalidDataException("Not a sudoku");

            var name = header[1];
            var N = int.Parse(header[2]);
            var nCells = int.Parse(header[3]);
            var nDomains = int.Parse(header[4]);

            var cellDatas = new SudokuCellData[nCells];
            var domainDatas = new SudokuDomainData[nDomains];
            var cellCounter = 0;
            var domainCounter = 0;

            foreach (var line in lines.Skip(1))
            {
                var lineType = line[..line.IndexOf(' ')];
                switch (lineType.ToLowerInvariant())
                {
                    case "cell":
                        var vals = line.Split(' ').Last().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
                        var cell = new SudokuCellData(vals[0], vals[1],
                            vals.Length >= 3 && vals[2] >= 0 ? vals[2]:null,
                            vals.Length >= 4?vals.Skip(3).ToArray():null);
                        cellDatas[cellCounter++]=cell;
                        break;
                    case "domain":
                        var indices = line.Split(' ').Last().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
                        var domain = new SudokuDomainData(indices);
                        domainDatas[domainCounter++]= domain;
                        break;
                }
            }

            if (domainCounter != nDomains)
                throw new InvalidDataException("File did not contain enough domains");
            if (cellCounter != nCells)
                throw new InvalidDataException("File did not contain enough cells");

            return new Sudoku(cellDatas, domainDatas, name);
        }

        public Sudoku(SudokuCellData[] cellDatas, SudokuDomainData[] domainDatas, string? name = null)
        {
            var cells = cellDatas.Select((x, i) => new SudokuCell(x.X, x.Y, i, this,domainDatas.Length,cellDatas.Length)).ToArray();
            var domains = domainDatas.Select((x, i) => new SudokuDomain(x.CellIndices.Select(y => cells[y]).ToHashSet(), i,this)).ToArray();

            for(var i = 0; i < cells.Length; ++i)
            {
                if (cellDatas[i].Value != null)
                    cells[i].SetValue(cellDatas[i].Value!.Value, true);

                if (cellDatas[i].Options != null && cells[i].Value == null)
                    cells[i].SetOptions(cellDatas[i].Options!, true);
            }

            Name = name ?? "Sudoku";

            if (cells.Select(x => (x.X, x.Y)).Distinct().Count() != cells.Length)
                throw new ArgumentException("Invalid sudoku with overlapping cells");

            if (domains.Length == 0)
                throw new ArgumentException("At least one domain is required");

            N = domains[0].Cells.Count;
            SqrtN = (int)Math.Round(Math.Sqrt(N));

            if (domains.Any(x => x.Cells.Count != N))
                throw new ArgumentException("All domains must have same size");

            Cells = cells;
            UnsetCells = new BARefSet<SudokuCell>(cellDatas.Length);

            Domains = domains;
            foreach (var cell in Cells)
            {
                if (!cell.Value.HasValue && cell.PossibleValues.Count == 0)
                    cell.SetOptions(Enumerable.Range(0, N), true);

                if (cell.IsUnset)
                    UnsetCells.Add(cell);
            }

            UnsetDomains = Domains.Where(x => x.Cells.Any(x => x.IsUnset)).ToHashSet();

            foreach (var cell in Cells)
                cell.Init();
            foreach (var domain in Domains)
                domain.Init();

            DomainIntersections = new();
            DomainExceptions = new();
            foreach (var domainA in Domains)
            {
                foreach (var domainB in domainA.IntersectingDomains)
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
            {
                cell.CellBecameSet += (sender, args) => { UnsetCells.Remove(args.Cell); };
                cell.CellBecameUnSet += (sender, args) => { UnsetCells.Add(args.Cell); };
            }
            foreach (var domain in Domains)
            {
                domain.DomainBecameSet += (sender, args) => { UnsetDomains.Remove(args.Domain); };
                domain.DomainBecameUnSet += (sender, args) => { UnsetDomains.Add(args.Domain); };
            }
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

        public int Grade(out string moveList,SudokuTechnique[]? techniques,out Sudoku solution)
        {
            techniques ??= SudokuTechnique.GetAllTechiques();
            var solver = new Solver(techniques
                .Where(x => x is not Solver && x is not SelectOnlies && x is not SudokuGenerator)
                .OrderBy(x => x.MinComplexity).ToList());
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

            var grade = moves.OrderByDescending(x => x.Complexity).Select((x,i) => x.Complexity/Math.Pow(i+1,1.5)).Sum();

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

            solution = sudoku;
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

        public bool IsSolved => UnsetCells.IsAllFalse();
        public Sudoku Clone(bool options = true)
        {
            return Parse(Serialize(options));
        }

        public static Sudoku StandardNxN(int n = 3)
        {
            var N = n * n;
            var cellDatas = Enumerable.Range(0, N).SelectMany(x => Enumerable.Range(0, N).Select(y => new SudokuCellData(x, y,null,null))).ToArray();
            var domainDatas = new List<SudokuDomainData>();

            for (var i = 0; i < N; i++)
            {
                domainDatas.Add(new SudokuDomainData(cellDatas.Select((x,i) => (x,i)).Where(x => x.x.X == i).Select(x => x.i).ToArray()));  // columns
                domainDatas.Add(new SudokuDomainData(cellDatas.Select((x, i) => (x, i)).Where(x => x.x.Y == i).Select(x => x.i).ToArray()));  // rows
                domainDatas.Add(new SudokuDomainData(cellDatas.Select((x, i) => (x, i)).Where(x => ((x.x.X / n) * n + (x.x.Y / n)) == i).Select(x => x.i).ToArray()));  // rows
            }
            return new Sudoku(cellDatas, domainDatas.ToArray());
        }
    }
}
