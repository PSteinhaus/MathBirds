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
        internal abstract bool Locked { get; set; }
        internal abstract int Combo { get; set; }
        internal int Multiplier
        {
            get
            {
                int baseValue = 1 + (int)((float)Math.Sqrt(Combo * 0.05f) / 0.4f);
                return baseValue < 6 ? baseValue : 30; 
            }
        }
        /// <summary>
        /// returns the progress towards the next higher multiplier in percent
        /// </summary>
        internal float MultiplProgress
        {
            get
            {
                return ((float)Math.Sqrt(Combo * 0.05f) / 0.4f) % 1;
            }
        }
        internal string ChallengeLaTeX { get; private protected set; }
        internal string ChallengeInfix { get; private protected set; }
        internal string[] AnswersLaTeX { get; private protected set; }
        internal string[] AnswersInfix { get; private protected set; }
        internal string SolutionLaTeX { get; private protected set; }
        internal string SolutionInfix { get; private protected set; }
        internal float QuestionNodeHeight { get; private protected set; } = 240f;
        internal float NodeHeight { get; private protected set; } = 220f;

        /// <summary>
        /// Returns a MathChallenge that is generated based on the parameters of the calling MathChallenge.
        /// </summary>
        /// <returns></returns>
        internal abstract MathChallenge CreateFromSelf();

        internal void CreateAnswerArrays(int answerCount)
        {
            AnswersInfix = new string[answerCount];
            AnswersLaTeX = new string[answerCount];
        }

        internal bool IsSolution(string answerInfix)
        {
            var answerExpr = Infix.ParseOrUndefined(answerInfix);
            if (answerExpr.IsUndefined) return false;
            else return answerExpr.Equals(Infix.ParseOrThrow(SolutionInfix));
        }

        internal bool TrySolveWith(string answerInfix)
        {
            if (IsSolution(answerInfix))
            {
                Combo++;
                return true;
            }
            else
            {
                Combo = 0;
                return false;
            }
        }

        internal static MathChallenge[] GetAllChallengeModels()
        {
            // keep this list updated when adding new major challenge types
            return new MathChallenge[] { new AddChallenge(), new SubChallenge(), new MultiplyChallenge(), new DivideChallenge(), new SolveChallenge() };
        }

        internal abstract ScrapyardButton CreateScrapyardButton();
    }
}
