using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal class DivideChallenge : MathChallenge
    {
        private static bool locked = true;
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
        private const int STD_MIN_NUM = -50;
        private const int STD_MAX_NUM = 200;
        private protected int MinNum { get; set; }
        private protected int MaxNum { get; set; }
        public DivideChallenge() : this(dummy: false)
        { }
        internal DivideChallenge(int answerCount = 4, int minNum = STD_MIN_NUM, int maxNum = STD_MAX_NUM, bool dummy=false)
        {
            QuestionNodeHeight = 340f;
            CreateAnswerArrays(answerCount);
            MinNum = minNum;
            MaxNum = maxNum;
            if (dummy) return;
            int numbersCount = 2;

            // prepare the RNG
            var rng = new Random();

            // create the challenge
            int[] numbers = new int[numbersCount];
            while (numbers[1] == 0 || numbers[0] % numbers[1] != 0 ||
                  ((numbers[0] == numbers[1] || numbers[0] == 0 || numbers[0] == -numbers[1] || numbers[1] == 1 || numbers[1] == -1) && rng.Next(20) != 0))
                for (int i=0; i< numbers.Length; i++)
                    numbers[i] = rng.Next(minNum, maxNum);

            ChallengeLaTeX = @"\frac{" + numbers[0] + "}{" + numbers[1] + "}";
            ChallengeInfix = numbers[0] + "/" + numbers[1];

            // create the solution
            int solution = numbers[0] / numbers[1];
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
            return new DivideChallenge(AnswersInfix.Length, MinNum, MaxNum);
        }
        
        private int WrongAnswer(int[] numbers)
        {
            int wrongAnswer = 0;
            int solution = (int)((SymbolicExpression)Infix.ParseOrThrow(SolutionInfix)).RealNumberValue;
            var rng = new Random();
            bool ok = false;
            for (int tries = 0; tries < 30 && !ok; tries++)
            {
                int method = rng.Next(2);
                switch (method)
                {
                    case 0:
                        // take the solution and add or substract 1 or 2
                        wrongAnswer = solution + new Random().Next(-3, 4);
                        break;
                    default:
                        // just roll something in the range of the solution
                        int range = Math.Abs(solution/2) + 5;
                        wrongAnswer = solution + rng.Next(solution - range, solution + range + 1);
                        break;
                }
                // check if the answer is ok
                ok = true;
                // check if the answer is identical to another
                foreach (string answer in AnswersInfix)
                {
                    // if it is, begin anew
                    if (answer != null && answer.Equals(wrongAnswer.ToString()))
                        ok = false;
                }
                // check if the answer is actually a solution
                if (IsSolution(wrongAnswer.ToString()))
                {
                    ok = false;
                    tries--;
                }
            }
            return wrongAnswer;
        }

        internal override ScrapyardButton CreateScrapyardButton()
        {
            return new ScrapyardButton(this, "scrapyardDivide.png");
        }

    }
}
