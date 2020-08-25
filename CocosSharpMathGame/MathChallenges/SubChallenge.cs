using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal class SubChallenge : MathChallenge
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
        private protected int NumbersCount { get; set; }
        private protected int MinNum { get; set; }
        private protected int MaxNum { get; set; }
        internal SubChallenge(int answerCount = 4, int numbersCount = 2, int minNum = STD_MIN_NUM, int maxNum = STD_MAX_NUM, bool dummy=false)
        {
            if (dummy) return;
            CreateAnswerArrays(answerCount);
            NumbersCount = numbersCount;
            MinNum = minNum;
            MaxNum = maxNum;

            // prepare the RNG
            var rng = new Random();

            // create the challenge
            int[] numbers = new int[numbersCount];
            for (int i=0; i< numbers.Length; i++)
                numbers[i] = rng.Next(minNum, maxNum);
            ChallengeLaTeX = numbers[0].ToString();
            for (int i = 1; i < numbers.Length; i++)
                ChallengeLaTeX += "-" + numbers[i].ToString();
            ChallengeInfix = ChallengeLaTeX;

            // create the solution
            int solution = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
                solution -= numbers[i];
            SolutionInfix = solution.ToString();
            SolutionLaTeX = SolutionInfix;
            // map it to a random answer
            int solutionIndex = rng.Next(answerCount);
            AnswersInfix[solutionIndex] = SolutionInfix;
            AnswersLaTeX[solutionIndex] = SolutionLaTeX;

            // generate the false answers
            for (int i=0; i<answerCount; i++)
            {
                // don't overwrite the solution
                if (i == solutionIndex) continue;
                // choose a random algorithm to create the next answer
                AnswersInfix[i] = WrongAnswer(numbers).ToString();
                AnswersLaTeX[i] = AnswersInfix[i];
            }
        }

        internal override MathChallenge CreateFromSelf()
        {
            return new SubChallenge(AnswersInfix.Length, NumbersCount, MinNum, MaxNum);
        }
        
        private int WrongAnswer(int[] numbers)
        {
            int wrongAnswer = 0;
            int solution = (int)((SymbolicExpression)Infix.ParseOrThrow(SolutionInfix)).RealNumberValue;
            var rng = new Random();
        start:
            int method = rng.Next(3);
            switch(method)
            {
                case 0:
                    // take the solution and add or substract 1 or 2
                    wrongAnswer = solution + rng.Next(-3, 4);
                    break;
                case 1:
                    // take the solution and add or substract 10 or 20
                    wrongAnswer = solution + rng.Next(-2, 3)*10;
                    break;
                default:
                    // just roll something in the range of the solution
                    int range = Math.Abs(solution/4);
                    wrongAnswer = solution + rng.Next(solution - range, solution + range);
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
            // check if the answer is smaller than the first number
            if (wrongAnswer >= numbers[0])
                goto start;
            return wrongAnswer;
        }

        internal override ScrapyardButton CreateScrapyardButton()
        {
            return new ScrapyardButton(this, "scrapyardSub.png");
        }
    }
}
