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
        public static IEnumerable<SudokuDomain> GetDomains(this Sudoku sudoku, BitArray refs)
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
        public static IEnumerable<SudokuCell> GetCells(this Sudoku sudoku, BitArray refs)
        {
            for (var i = 0; i < refs.Count; ++i)
                if (refs[i])
                    yield return sudoku.Cells[i];
        }

    }
}
