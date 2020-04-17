using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal abstract class MathChallenge
    {
        internal abstract string Challenge();
        internal abstract string[] Answers();
        internal abstract string Solution();
        
        internal bool IsSolution(string answer)
        {
            var answerExpr = Infix.ParseOrUndefined(answer);
            if (answerExpr.IsUndefined) return false;
            else return answerExpr.Equals(Infix.ParseOrThrow(Solution()));
        }
    }
}
