using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Can be "advanced" only given the time to advance by.
    /// </summary>
    interface IAdvancable
    {
        /// <summary>
        /// Advance by time dt and return whether it is done (can now be removed).
        /// </summary>
        /// <param name="dt">time to advance by</param>
        /// <returns>whether it is now finished (and can be removed)</returns>
        bool Advance(float dt);
    }
}
