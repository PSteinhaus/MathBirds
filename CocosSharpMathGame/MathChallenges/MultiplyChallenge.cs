using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    internal class MultiplyChallenge : MathChallenge
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
        private const int STD_MIN_NUM = -5;
        private const int STD_MAX_NUM = 20;
        private protected int NumbersCount { get; set; }
        private protected int MinNum { get; set; }
        private protected int MaxNum { get; set; }
        internal MultiplyChallenge(int answerCount = 4, int numbersCount = 2, int minNum = STD_MIN_NUM, int maxNum = STD_MAX_NUM)
        {
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
            // make sure that no more than one number is greater than 10 (because that's just annoying)
            bool greater10Found = false;
            for (int i = 0; i < numbers.Length; i++)
            {
                if (Math.Abs(numbers[i]) > 10)
                {
                    if (greater10Found)
                        while (Math.Abs(numbers[i])>10)
                            numbers[i] = rng.Next(minNum, maxNum);
                    greater10Found = true;
                }
            }
            ChallengeLaTeX = numbers[0].ToString();
            for (int i = 1; i < numbers.Length; i++)
                ChallengeLaTeX += @"\cdot" + numbers[i].ToString();
            ChallengeInfix = numbers[0].ToString();
            for (int i = 1; i < numbers.Length; i++)
                ChallengeInfix += "*" + numbers[i].ToString();

            // create the solution
            int solution = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
                solution *= numbers[i];
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
            return new MultiplyChallenge(AnswersInfix.Length, NumbersCount, MinNum, MaxNum);
        }
        
        private int WrongAnswer(int[] numbers)
        {
            int wrongAnswer = 0;
            int solution = (int)((SymbolicExpression)Infix.ParseOrThrow(SolutionInfix)).RealNumberValue;
            var rng = new Random();
            bool ok = false;
            for (int tries = 0; tries < 30 && !ok; tries++)
            {
                int method = rng.Next(3);
                switch (method)
                {
                    case 0:
                        // multiply with slightly changed numbers
                        wrongAnswer = numbers[1];
                        for (int i = 0; i < numbers.Length; i++)
                        {
                            if (i == 1) continue;
                            wrongAnswer *= (numbers[i] + (rng.NextBoolean() ? 1 : -1));
                        }
                        break;
                    case 1:
                        // multiply with slightly changed numbers
                        wrongAnswer = numbers[0];
                        for (int i = 1; i < numbers.Length; i++)
                            wrongAnswer *= (numbers[i] + (rng.NextBoolean() ? 1 : -1));
                        break;
                    default:
                        // just roll something in the range of the solution
                        int range = Math.Abs(solution/4);
                        wrongAnswer = solution + rng.Next(solution - range, solution + range);
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
                    ok = false;
            }
            return wrongAnswer;
        }

        internal override ScrapyardButton CreateScrapyardButton()
        {
            return new ScrapyardButton(this, "scrapyardMultiply.png");
        }
    }
}
