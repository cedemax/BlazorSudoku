﻿namespace BlazorSudoku
{
    public class SudokuDomainEventArgs : EventArgs
    {
        /// <summary>
        /// The domain that was affected
        /// </summary>
        public SudokuDomain Domain { get; set; }


        public SudokuDomainEventArgs(SudokuDomain domain)
        {
            Domain = domain;
        }
    }
}
