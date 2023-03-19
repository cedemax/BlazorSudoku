using BlazorSudoku.Hints;
using System.Drawing;

namespace BlazorSudoku
{

    public enum SudokuActionType
    {
        SetOnlyPossible,
        RemoveOption,
        SetValue,
    }
    public class SudokuMove
    {
        public string Name { get; set; }

        public int Complexity { get; set; }

        public List<SudokuAction> Operations { get; set; } = new();

        public List<SudokuHint> Hints { get; set; } = new ();

        public bool IsEmpty => Operations.Count == 0;

        public SudokuMove(string name,int complexity)
        {
            Name = name;
            Complexity = complexity;
        }

        public void Perform(Sudoku sudoku)
        {
            foreach(var op in Operations)
            {
                switch (op.Action)
                {
                    case SudokuActionType.SetValue:
                        op.Cell.SetValue(op.Value);
                        break;
                    case SudokuActionType.SetOnlyPossible:
                        if(op.Cell.IsUnset)
                            op.Cell.SetOption(op.Value);
                        break;
                    case SudokuActionType.RemoveOption:
                        if(op.Cell.IsUnset)
                            op.Cell.RemoveOption(op.Value);
                        break;
                }
            }
        }

        public override string ToString() => string.Join(" -> ", Operations);
    }

    public class SudokuAction
    {
        public SudokuCell Cell { get; set; }
        public SudokuActionType Action { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }

        public SudokuAction(SudokuCell cell,SudokuActionType sudokuActionType, int value, string description) 
        {
            Cell = cell;
            Action = sudokuActionType;
            Value = value;
            Description = description;
        }
        public override string ToString() => $"{Action} {Value} on {Cell}";

    }
}
