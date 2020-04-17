using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Math Challenges hold a question, the solution, and wrong answers
    /// both in infix and in LaTeX form.
    /// The infix forms may be simplified because of the automatic simplification in MathNet.Symbolics
    /// </summary>
    internal abstract class MathChallenge
    {
        internal string ChallengeLaTeX { get; private protected set; }
        internal string[] AnswersLaTeX { get; private protected set; }
        internal string[] AnswersInfix { get; private protected set; }
        internal string SolutionLaTeX { get; private protected set; }
        internal string SolutionInfix { get; private protected set; }

        internal void CreateAnswerArrays(int answerCount)
        {
            AnswersInfix = new string[answerCount > 0 ? answerCount : 1];
            AnswersLaTeX = new string[answerCount > 0 ? answerCount : 1];
        }

        internal bool IsSolution(string answerInfix)
        {
            var answerExpr = Infix.ParseOrUndefined(answerInfix);
            if (answerExpr.IsUndefined) return false;
            else return answerExpr.Equals(Infix.ParseOrThrow(SolutionInfix));
        }
    }
}
