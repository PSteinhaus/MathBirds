using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal class AddChallenge : MathChallenge
    {
        private static bool locked = false;
        internal override bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }
        private static int combo = 0;
        internal override int Combo
        {
            get { return combo; }
            set { combo = value; }
        }
        private const int STD_MIN_NUM = 1;
        private const int STD_MAX_NUM = 100;
        private protected int SummandCount { get; set; }
        private protected int MinNum { get; set; }
        private protected int MaxNum { get; set; }
        internal AddChallenge(int answerCount = 4, int summandCount = 2, int minNum = STD_MIN_NUM, int maxNum = STD_MAX_NUM, bool dummy=false)
        {
            if (dummy) return;
            CreateAnswerArrays(answerCount);
            SummandCount = summandCount;
            MinNum = minNum;
            MaxNum = maxNum;

            // prepare the RNG
            var rng = new Random();

            // create the challenge
            // the challenge consists of adding 'summandCount' many summands together
            int[] summands = new int[summandCount];
            for (int i = 0; i < summands.Length; i++)
                summands[i] = rng.Next(minNum, maxNum);
            ChallengeLaTeX = summands[0].ToString();
            for (int i = 1; i < summands.Length; i++)
                ChallengeLaTeX += "+" + summands[i].ToString();
            ChallengeInfix = ChallengeLaTeX;

            // create the solution
            SolutionInfix = summands.Sum().ToString();
            SolutionLaTeX = SolutionInfix;
            // map it to a random answer
            int solutionIndex = rng.Next(answerCount);
            AnswersInfix[solutionIndex] = SolutionInfix;
            AnswersLaTeX[solutionIndex] = SolutionLaTeX;

            // generate the false answers
            for (int i = 0; i < answerCount; i++)
            {
                // don't overwrite the solution
                if (i == solutionIndex) continue;
                // choose a random algorithm to create the next answer
                AnswersInfix[i] = WrongAnswer(rng, summands).ToString();
                AnswersLaTeX[i] = AnswersInfix[i];
            }
        }

        internal override MathChallenge CreateFromSelf()
        {
            return new AddChallenge(AnswersInfix.Length, SummandCount, MinNum, MaxNum);
        }

        private int WrongAnswer(Random rng, int[] summands)
        {
            int wrongAnswer = 0;
            int solution = (int)((SymbolicExpression)Infix.ParseOrThrow(SolutionInfix)).RealNumberValue;
        start:
            int algorithm = rng.Next(0, 3);
            switch (algorithm)
            {
                case 0:
                    // take the solution and add or substract 1 or 2
                    wrongAnswer = solution + rng.Next(-2, 3);
                    break;
                case 1:
                    // take the solution and add or substract 10 or 20
                    wrongAnswer = solution + rng.Next(-2, 3)*10;
                    break;
                default:
                    // just roll something in the range of the solution
                    wrongAnswer = solution + rng.Next(solution - (solution / 4), solution + (solution / 4));
                    break;
            }
            // check if the answer is identical to another
            foreach (string answer in AnswersInfix)
            {
                // if it is, begin anew
                if (answer != null && answer.Equals(wrongAnswer.ToString()))
                    goto start;
            }
            // check if the answer is actually a solution
            if (IsSolution(wrongAnswer.ToString()))
                goto start;
            // check if the answer is larger than each summand (it would be too simple if else)
            for (int i=0; i<summands.Length; i++)
                if (wrongAnswer <= summands[i])
                    goto start;
            return wrongAnswer;
        }

        internal override ScrapyardButton CreateScrapyardButton()
        {
            return new ScrapyardButton(this, "scrapyardAdd.png");
        }

        internal static AddChallenge GetDummy()
        {
            return new AddChallenge(dummy:true);
        }
    }
}
