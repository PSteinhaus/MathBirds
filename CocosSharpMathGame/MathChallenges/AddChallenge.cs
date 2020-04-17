using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal class AddChallenge : MathChallenge
    {
        private const int STD_MIN_NUM = 1;
        private const int STD_MAX_NUM = 100;
        AddChallenge(int answerCount, int summandCount, int minNum = STD_MIN_NUM, int maxNum = STD_MAX_NUM)
        {
            CreateAnswerArrays(answerCount);

            // prepare the RNG
            var rng = new Random();

            // create the challenge
            // the challenge consists of adding 'summandCount' many summands together
            int[] summands = new int[summandCount];
            for (int i=0; i<summands.Length; i++)
                summands[i] = rng.Next(minNum, maxNum);
            ChallengeLaTeX = summands[0].ToString();
            for (int i = 1; i < summands.Length; i++)
                ChallengeLaTeX += "+" + summands[0].ToString();

            // create the solution
            SolutionInfix = summands.Sum().ToString();
            SolutionLaTeX = SolutionInfix;
            // map it to a random answer
            int solutionIndex = rng.Next(0, AnswersInfix.Length);
            AnswersInfix[solutionIndex] = SolutionInfix;
            AnswersLaTeX[solutionIndex] = SolutionLaTeX;

            // generate the false answers
            for (int i=0; i<answerCount; i++)
            {
                // don't overwrite the solution
                if (i == solutionIndex) continue;
                // choose a random algorithm to create the next answer
                AnswersInfix[i] = WrongAnswer(rng.Next(0, 2)).ToString();
                AnswersLaTeX[i] = AnswersInfix[i];
            }
        }

        private int WrongAnswer(int algorithm)
        {
            int wrongAnswer = 0;
            int solution = (int)((SymbolicExpression)Infix.ParseOrThrow(SolutionInfix)).RealNumberValue;
            start:
            switch(algorithm)
            {
                case 0:
                    // take the solution and add or substract 1 or 2
                    wrongAnswer = solution + new Random().Next(-2, 2);
                    break;
                case 1:
                    // take the solution and add or substract 10 or 20
                    wrongAnswer = solution + new Random().Next(-2, 2)*10;
                    break;
                default:
                    // just roll something in the range of the solution
                    wrongAnswer = solution + new Random().Next(solution - (solution / 2), solution + (solution / 2));
                    break;
            }
            // check if the answer is identical to another
            foreach (string answer in AnswersInfix)
            {
                // if it is, begin anew
                if (answer.Equals(wrongAnswer.ToString()))
                    goto start;
            }
            // check if the answer is actually a solution
            if (IsSolution(wrongAnswer.ToString()))
                goto start;
            // check if the answer is positive (negative or 0 doesn't make much sense)
            if (wrongAnswer<=1)
                goto start;
            return wrongAnswer;
        }
    }
}
