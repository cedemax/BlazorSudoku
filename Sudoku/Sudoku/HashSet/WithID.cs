using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSudoku
{
    public interface WithID
    {
        /// <summary>
        /// A unique ID
        /// </summary>
        public int Key { get; }
    }
}
