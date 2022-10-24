using BlazorSudoku;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSudoku
{
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Returns the domains based on the references
        /// </summary>
        /// <param name="sudoku"></param>
        /// <param name="refs"></param>
        /// <returns></returns>
        public static IEnumerable<SudokuDomain> GetDomains(this Sudoku sudoku, BA<SudokuDomain> refs) => GetDomains(sudoku, refs.Refs);

        /// <summary>
        /// Returns the domains based on the references
        /// </summary>
        /// <param name="sudoku"></param>
        /// <param name="refs"></param>
        /// <returns></returns>
        public static IEnumerable<SudokuDomain> GetDomains(this Sudoku sudoku, CBitArray refs)
        {
            for (var i = 0; i < refs.Count; ++i)
                if (refs[i])
                    yield return sudoku.Domains[i];
        }

        /// <summary>
        /// Returns the domains based on the references
        /// </summary>
        /// <param name="sudoku"></param>
        /// <param name="refs"></param>
        /// <returns></returns>
        public static IEnumerable<SudokuCell> GetCells(this Sudoku sudoku, BA<SudokuCell> refs) => GetCells(sudoku, refs.Refs);

        /// <summary>
        /// Returns the domains based on the references
        /// </summary>
        /// <param name="sudoku"></param>
        /// <param name="refs"></param>
        /// <returns></returns>
        public static IEnumerable<SudokuCell> GetCells(this Sudoku sudoku, CBitArray refs)
        {
            for (var i = 0; i < refs.Count; ++i)
                if (refs[i])
                    yield return sudoku.Cells[i];
        }
        /// <summary>
        /// Returns the possible values for a set of cells
        /// </summary>
        /// <param name="sudoku"></param>
        /// <param name="refs"></param>
        /// <returns></returns>
        public static Set32 GetPossibleValues(this Sudoku sudoku, BA<SudokuCell> refs)
        {
            var ret = Set32.Empty;
            for (var i = 0; i < refs.Refs.Count; ++i)
                if (refs.Refs[i])
                    ret.UnionWith(sudoku.Cells[i].PossibleValues);
            return ret;
        }

        /// <summary>
        /// Returns the counts of possible values for a set of cells
        /// </summary>
        /// <param name="sudoku"></param>
        /// <param name="refs"></param>
        /// <returns></returns>
        public static void GetPossibleValueCounts(this Sudoku sudoku, BA<SudokuCell> refs,ref int[] ret)
        {
            Array.Fill(ret, 0);
            for (var i = 0; i < refs.Refs.Count; ++i)
                if (refs.Refs[i])
                    foreach (var v in sudoku.Cells[i].PossibleValues)
                        ++ret[v];
        }

    }
}
