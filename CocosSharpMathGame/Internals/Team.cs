using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class Team
    {
        internal bool IsEnemy(Team team)
        {
            return team != this;
        }
    }
}
