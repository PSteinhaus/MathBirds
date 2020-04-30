using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Aircraft Artificial Intelligence
    /// </summary>
    internal abstract class AI
    {
        /// <summary>
        /// the aircraft that this AI belongs to
        /// </summary>
        internal Aircraft Aircraft { get; set; }
        internal AI()
        {
        }
        /// <summary>
        /// Do your move.
        /// </summary>
        /// <param name="aircrafts">The list of all aircrafts in the level</param>
        internal abstract void ActInPlanningPhase(IEnumerable<Aircraft> aircrafts);
    }
}
