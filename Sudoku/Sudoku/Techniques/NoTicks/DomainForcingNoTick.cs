using BlazorSudoku.Hints;

namespace BlazorSudoku.Techniques
{
    public class NoMarksInferrence : SudokuTechnique
    {

        public override string Name => $"No Marks";

        public override int MinComplexity => 0;

        public int Degree { get; }

        public NoMarksInferrence() : this(int.MaxValue) { }
        public NoMarksInferrence(int degree)
        {
            Degree = degree;
        }


        private class NoMarksStep
        {
            public string Name { get; set; } = "No Marks";
            public int Complexity { get; set; } = 0;
            public int Generation { get; set; } = 0;
            public bool IsSolved { get; set; } = false;
            public BARefSet<SudokuCell> Possible { get; set; }

            public NoMarksStep(BA<SudokuCell> initial)
            {
                Possible = new(new CBitArray(initial.Refs));
            }

            public List<SudokuHint> Hints { get; set; } = new();
        }

        public override List<SudokuMove> GetMoves(Sudoku sudoku, int limit, int complexityLimit, bool hint = true)
        {
            if (complexityLimit < MinComplexity)
                return new();

            var moves = new List<SudokuMove>();

            var unlockedValues = sudoku.GetPossibleValues(sudoku.UnlockedCellRefs);


            foreach (var unlockedValue in unlockedValues)
            {
                // one value at a time
                // these are the domains that can be affected
                var domains = sudoku.Domains.Where(x => !sudoku.GetPossibleValues(x.LockedCellRefs).Contains(unlockedValue)).ToArray();
                var steps = domains.ToDictionary(x => x, x => new NoMarksStep(x.UnlockedCellRefs));

                // check for already solved domains
                foreach (var (domain, step) in steps)
                {
                    if (step.Possible.CountTrue() == 1)
                    {
                        // final empty cell.
                        if (hint)
                            step.Hints.Add(new SudokuDomainHint(domain, SudokuHint.Direct));
                        step.Name = "Final Empty Cell";
                        step.Complexity = 0;
                        step.IsSolved = true;
                    }
                }

                // do the direct elimination step from pre-solved domains (only needs to be done once)
                foreach (var (domain, step) in steps.Where(x => !x.Value.IsSolved))
                {
                    int complexity = 0;
                    foreach (var intersectingDomain in domain.IntersectingDomains)
                    {
                        if (!steps.TryGetValue(intersectingDomain, out var intersectingStep))
                        {
                            // a pre-solved domain. Eliminates intersecting cells
                            var intersection = step.Possible.Intersect(intersectingDomain.Cells);
                            if (intersection.CountTrue() > 0)
                            {
                                // something was eliminated.
                                if (hint)
                                {
                                    step.Hints.Add(new SudokuDomainHint(intersectingDomain, SudokuHint.Direct));
                                    step.Hints.Add(new SudokuCellHint(intersectingDomain.Cells.Single(x => x.Value == unlockedValue), SudokuHint.Direct));
                                }
                                step.Complexity++;
                                step.Possible.ExceptWith(intersection);
                                if (step.Possible.CountTrue() == 1)
                                {
                                    // solved...
                                    step.IsSolved = true;
                                    step.Name = "Direct Elimination No Marks";
                                    step.Complexity = (int)Math.Round(Math.Sqrt(step.Generation)*step.Complexity * (domain.IsColOrRow ? 1.5 : 1));
                                    break;
                                }
                            }
                        }
                    }
                }

                // iteratively advance our eliminations
                var found = false;
                int generation = 0;
                do
                {
                    var gSteps = steps.ToDictionary(x => x.Key, x => new NoMarksStep(x.Value.Possible) { Hints = x.Value.Hints,Complexity = x.Value.Complexity });
                    found = false;
                    // first perform domain-locking
                    generation++;

                    // too complex
                    if (generation > Degree)
                        break;

                    foreach (var (domain, gStep) in gSteps.Where(x => !x.Value.IsSolved))
                    {
                        var remainingCells = gStep.Possible;
                        // all the intersecting domains that might cause eliminations
                        foreach (var line in domain.IntersectingDomains.Where(x => steps.ContainsKey(x)))
                        {
                            var lineStep = steps[line];
                            if (lineStep.IsSolved)
                                continue;
                            // this domain is locked in the line domain. Hence the line domain must be locked in this domain.
                            // also make sure that there is something to eliminate... 
                            if (lineStep.Possible.Contains(gStep.Possible) && !gStep.Possible.Contains(lineStep.Possible))
                            {
                                var eliminations = lineStep.Possible.Except(gStep.Possible);
                                lineStep.Possible.IntersectWith(gStep.Possible);
                                lineStep.Generation++;

                                if (hint)
                                {
                                    lineStep.Hints.Add(new SudokuStepHint(gStep.Hints, generation));
                                    lineStep.Hints.Add(new SudokuDomainHint(line, SudokuHint.Base));
                                    lineStep.Hints.Add(new SudokuDomainHint(domain, SudokuHint.Fin));
                                }
                                lineStep.Complexity+=gStep.Complexity;
                                found = true;
                            }
                        }
                    }

                    // then expand the lock values to intersecting domains
                    generation++;
                    foreach (var (domain, step) in steps.Where(x => !x.Value.IsSolved))
                    {
                        foreach (var line in domain.IntersectingDomains.Where(x => steps.ContainsKey(x)))
                        {
                            var lineStep = steps[line];

                            var intersection = sudoku.DomainIntersections[(domain, line)];
                            var possibleA = step.Possible.Intersect(intersection);
                            var possibleB = lineStep.Possible.Intersect(intersection);
                            var toEliminateFromStep = possibleA.Except(possibleB);
                            if (toEliminateFromStep.CountTrue() > 0)
                            {
                                step.Possible.ExceptWith(toEliminateFromStep);
                                step.Complexity += lineStep.Complexity;
                                step.Generation++;
                                if (hint)
                                    step.Hints.Add(new SudokuStepHint(lineStep.Hints, generation));
                                found = true;
                            }
                        }
                    }

                    // then check if we solved something
                    foreach (var (domain, step) in steps.Where(x => !x.Value.IsSolved))
                    {
                        if (step.Possible.CountTrue() == 1)
                        {
                            found = true;
                            step.IsSolved = true;
                            step.Name = $"{Cardinal(step.Generation)} degree inferrence No Marks";
                            step.Complexity = 5+(int)Math.Round(step.Complexity * (domain.IsColOrRow ? 1.5 : 1));
                        }
                    }
                    if (generation > sudoku.Domains.Length)
                        throw new InvalidOperationException("Too many generations");
                } while (found);

               
                moves.AddRange(steps.Where(x => x.Value.IsSolved).Select(x => new SudokuMove(x.Value.Name, x.Value.Complexity)
                {
                    Hints = x.Value.Hints.Concat(new SudokuHint[]
                    {
                        new SudokuCellHint(sudoku.GetCells(x.Value.Possible).Single(),SudokuHint.Positive,false),
                        new SudokuDomainHint(x.Key,SudokuHint.Positive,false),
                    }).ToList(),
                    Operations = new() { new SudokuAction(sudoku.GetCells(x.Value.Possible).Single(), SudokuActionType.SetValue, unlockedValue, "Set by inference") }
                }));
            }

            return moves.OrderBy(x => x.Complexity).Take(limit).ToList();
        }

        private static string Cardinal(int n) => n switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            _ => $"{n}th"
        };
    }
}
