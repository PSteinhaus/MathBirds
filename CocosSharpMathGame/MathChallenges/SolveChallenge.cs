using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Random;
using MathNet.Symbolics;

namespace CocosSharpMathGame
{
    // TODO: turn this class from a placeholder to an actual subtraction challenge
    internal class SolveChallenge : MathChallenge
    {
        private static bool locked = false;
        internal override bool Locked
        {
            get { return locked; }
            set { locked = value; }
        }
        private const int STD_MIN_NUM = 1;
        private const int STD_MAX_NUM = 9;
        internal SolveChallenge(int answerCount = 4, int minNum = STD_MIN_NUM, int maxNum = STD_MAX_NUM)
        {
            QuestionNodeHeight = 340f;
            NodeHeight = 320f;
            CreateAnswerArrays(answerCount);

            // prepare the RNG
            var rng = new Random();

            // choose the variables
            string[] signs = new string[3];
            string[] variables = new string[3];
            for (int i = 0; i < variables.Length; i++)
            {
                if (rng.NextBoolean())
                {
                    // choose a letter
                    switch(rng.Next(9))
                    {
                        case 0: variables[i] = "a"; break;
                        case 1: variables[i] = "b"; break;
                        case 2: variables[i] = "c"; break;
                        case 3: variables[i] = "y"; break;
                        case 4: variables[i] = "t"; break;
                        case 5: variables[i] = "k"; break;
                        case 6: variables[i] = "i"; break;
                        case 7: variables[i] = "s"; break;
                        case 8: variables[i] = "p"; break;
                        case 9: variables[i] = "r"; break;
                    }
                }
                else
                {
                    // choose a number
                    if (rng.NextBoolean())
                        variables[i] = rng.Next(minNum, maxNum).ToString();
                    else
                        variables[i] = 1.ToString();
                }
                if (rng.NextBoolean())
                {
                    signs[i] = "-";
                }
                else
                {
                    signs[i] = "";
                }
            }

            // create the challenge
            const string LA_MUL = @"\cdot";
            var challengeType = rng.Next(5);
            string side1Infix = "";
            string side1LaTeX = "";
            string side2Infix = "";
            string side2LaTeX = "";
            bool NegStr(string str) { return str.StartsWith("-"); }
            string SignedVar(int i)
            {
                return signs[i] + variables[i];
            }
            string MulFromLeft(int i)
            {
                return (variables[i].Equals("1") ? signs[i] : SignedVar(i) + " " + LA_MUL + " ");
            }
            string MulStrFromLeft(string str)
            {
                return (str.Equals("1") ? "" : (str.Equals("-1") ? "-" : str + " " + LA_MUL + " "));
            }
            string AddFromRight(string str)
            {
                return (NegStr(str) ? "" : " + ") + str;
            }
            string SubFromRight(string str)
            {
                return (NegStr(str) ? " + " + str.Substring(1) : str);
            }
            switch (challengeType)
            {
                case 0:
                    {
                        side1Infix = SignedVar(0);
                        side1LaTeX = SignedVar(0);
                        side2Infix = "x * " + SignedVar(1) + " + x * " + SignedVar(2);
                        side2LaTeX = MulFromLeft(1) + "x" + AddFromRight(MulFromLeft(2) + "x");
                    }
                    break;
                case 1:
                    {
                        side1Infix = SignedVar(0) + " * x";
                        side1LaTeX = MulFromLeft(0) + "x";
                        side2Infix = SignedVar(1) + " + " + SignedVar(2) + " * x";
                        side2LaTeX = SignedVar(1) + AddFromRight(MulFromLeft(2) + "x");
                    }
                    break;
                case 2:
                    {
                        side1Infix = "(" + SignedVar(0) + " + " + SignedVar(1) + ") / x";
                        side1LaTeX = @"\frac{" + SignedVar(0) + AddFromRight(SignedVar(1)) + "}{ x }";
                        side2Infix = SignedVar(2);
                        side2LaTeX = SignedVar(2);
                    }
                    break;
                case 3:
                    {
                        side1Infix = SignedVar(0) + " + " + SignedVar(1);
                        side1LaTeX = SignedVar(0) + AddFromRight(SignedVar(1));
                        side2Infix = "x / " + SignedVar(2);
                        side2LaTeX = @"\frac{ x }{" + SignedVar(2) + "}";
                    }
                    break;
                case 4:
                    {
                        side1Infix = SignedVar(0) + " / " + SignedVar(1);
                        side1LaTeX = @"\frac{" + SignedVar(0) + "}{" + SignedVar(1) + "}";
                        side2Infix = SignedVar(2) + " / x";
                        side2LaTeX = @"\frac{" + SignedVar(2) + "}{ x }";
                    }
                    break;
            }
            // shuffle the sides
            if (rng.NextBoolean())
            {
                ChallengeInfix = side1Infix + " = " + side2Infix;
                ChallengeLaTeX = side1LaTeX + " = " + side2LaTeX;
            }
            else
            {
                ChallengeInfix = side2Infix + " = " + side1Infix;
                ChallengeLaTeX = side2LaTeX + " = " + side1LaTeX;
            }

            // create the solution
            switch(challengeType)
            {
                case 0:
                    {
                        SolutionInfix = SignedVar(0) + " / (" + SignedVar(1) + " + " + SignedVar(2) + ")";
                        // the following SolutionLaTeX-commands are commented out, because it's simply better (more beautiful/ easier to read) to use Latex.Format
                        //SolutionLaTeX = @"x = \frac{" + SignedVar(0) + "}{" + SignedVar(1) + AddFromRight(SignedVar(2)) + "}";
                    }
                    break;
                case 1:
                    {
                        SolutionInfix = SignedVar(1) + " / (" + SignedVar(0) + " - " + SignedVar(2) + ")";
                        //SolutionLaTeX = @"x = \frac{" + SignedVar(1) + "}{" + SignedVar(0) + SubFromRight(SignedVar(2)) + "}";
                    }
                    break;
                case 2:
                    {
                        SolutionInfix = "(" + SignedVar(0) + " + " + SignedVar(1) + ") / " + SignedVar(2);
                        //SolutionLaTeX = @"x = \frac{" + SignedVar(0) + AddFromRight(SignedVar(1)) + "}{" + SignedVar(2) + "}";
                    }
                    break;
                case 3:
                    {
                        SolutionInfix = "(" + SignedVar(0) + " + " + SignedVar(1) + ") * " + SignedVar(2);
                        //SolutionLaTeX = "x = " + MulStrFromLeft("(" + SignedVar(0) + AddFromRight(SignedVar(1)) + ")") + SignedVar(2);
                    }
                    break;
                case 4:
                    {
                        SolutionInfix = "(" + SignedVar(2) + " * " + SignedVar(1) + ") / " + SignedVar(0);
                        //SolutionLaTeX = @"x = \frac{" + MulFromLeft(2) + SignedVar(1) + "}{" + SignedVar(0) + "}";
                    }
                    break;
            }
            SolutionLaTeX = "x = " + LaTeX.Format(Infix.ParseOrThrow(SolutionInfix));
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
                bool[] alreadyTried = new bool[7];
                string infix = "";
                int method = -1;
                bool ok = false;
                for (int tries = 0; tries < 30 && !ok; tries++)
                {
                    switch (challengeType)
                    {
                        case 0:
                            {
                                do
                                {
                                    if (method != -1)
                                        alreadyTried[method] = true;
                                    method = rng.Next(7);
                                    switch (method)
                                    {
                                        case 0:
                                            {
                                                infix = "(" + SignedVar(0) + " - " + SignedVar(2) + ") / " + SignedVar(1);
                                            }
                                            break;
                                        case 1:
                                            {
                                                infix = SignedVar(0) + " - " + SignedVar(1) + " - " + SignedVar(2);
                                            }
                                            break;
                                        case 2:
                                            {
                                                infix = SignedVar(0) + " - (" + SignedVar(1) + " + " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 3:
                                            {
                                                infix = SignedVar(0) + " / (" + SignedVar(1) + " * " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 4:
                                            {
                                                infix = SignedVar(1) + " + " + SignedVar(2);
                                            }
                                            break;
                                        case 5:
                                            {
                                                infix = SignedVar(1) + " / (" + SignedVar(0) + " + " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 6:
                                            {
                                                infix = SignedVar(2) + " / (" + SignedVar(0) + " + " + SignedVar(1) + ")";
                                            }
                                            break;
                                    }
                                }
                                while (alreadyTried[method]);
                            }
                            break;
                        case 1:
                            {
                                do
                                {
                                    if (method != -1)
                                        alreadyTried[method] = true;
                                    method = rng.Next(6);
                                    switch (method)
                                    {
                                        case 0:
                                            {
                                                infix = SignedVar(1) + " / (" + SignedVar(2) + " - " + SignedVar(0) + ")";
                                            }
                                            break;
                                        case 1:
                                            {
                                                infix = SignedVar(0) + " / (" + SignedVar(1) + " + " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 2:
                                            {
                                                infix = "(" + SignedVar(0) + " + " + SignedVar(1) + ") / " + SignedVar(2);
                                            }
                                            break;
                                        case 3:
                                            {
                                                infix = "(" + SignedVar(2) + " * " + SignedVar(1) + ") /" + SignedVar(0);
                                            }
                                            break;
                                        case 4:
                                            {
                                                infix = "(" + SignedVar(0) + " + " + SignedVar(1) + ") *" + SignedVar(2);
                                            }
                                            break;
                                        case 5:
                                            {
                                                infix = "(" + SignedVar(0) + " + " + SignedVar(2) + ") /" + SignedVar(1);
                                            }
                                            break;
                                    }
                                }
                                while (alreadyTried[method]);
                            }
                            break;
                        case 2:
                            {
                                do
                                {
                                    if (method != -1)
                                        alreadyTried[method] = true;
                                    method = rng.Next(6);
                                    switch (method)
                                    {
                                        case 0:
                                            {
                                                infix = SignedVar(0) + " / (" + SignedVar(1) + " + " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 1:
                                            {
                                                infix = SignedVar(1) + " / (" + SignedVar(0) + " - " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 2:
                                            {
                                                infix = "1 / (" + SignedVar(2) + " - " + SignedVar(0) + " - " + SignedVar(1) + ")";
                                            }
                                            break;
                                        case 3:
                                            {
                                                infix = "(" + SignedVar(0) + " + " + SignedVar(2) + ") /" + SignedVar(1);
                                            }
                                            break;
                                        case 4:
                                            {
                                                infix = "(" + SignedVar(1) + " + " + SignedVar(2) + ") /" + SignedVar(0);
                                            }
                                            break;
                                        case 5:
                                            {
                                                infix = "(" + SignedVar(1) + " - " + SignedVar(0) + ") /" + SignedVar(2);
                                            }
                                            break;
                                    }
                                }
                                while (alreadyTried[method]);
                            }
                            break;
                        case 3:
                            {
                                do
                                {
                                    if (method != -1)
                                        alreadyTried[method] = true;
                                    method = rng.Next(6);
                                    switch (method)
                                    {
                                        case 0:
                                            {
                                                infix = "(" + SignedVar(0) + " + " + SignedVar(1) + ") /" + SignedVar(2);
                                            }
                                            break;
                                        case 1:
                                            {
                                                infix = SignedVar(0) + " / (" + SignedVar(1) + " + " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 2:
                                            {
                                                infix = SignedVar(1) + " / (" + SignedVar(0) + " - " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 3:
                                            {
                                                infix = "(" + SignedVar(0) + " + " + SignedVar(2) + ") *" + SignedVar(1);
                                            }
                                            break;
                                        case 4:
                                            {
                                                infix = "(" + SignedVar(1) + " + " + SignedVar(2) + ") *" + SignedVar(0);
                                            }
                                            break;
                                        case 5:
                                            {
                                                infix = "(" + SignedVar(2) + " - " + SignedVar(1) + ") *" + SignedVar(0);
                                            }
                                            break;
                                    }
                                }
                                while (alreadyTried[method]);
                            }
                            break;
                        case 4:
                            {
                                do
                                {
                                    if (method != -1)
                                        alreadyTried[method] = true;
                                    method = rng.Next(6);
                                    switch (method)
                                    {
                                        case 0:
                                            {
                                                infix = SignedVar(0) + " / (" + SignedVar(1) + " + " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 1:
                                            {
                                                infix = SignedVar(1) + " / (" + SignedVar(0) + " - " + SignedVar(2) + ")";
                                            }
                                            break;
                                        case 2:
                                            {
                                                infix = "(" + SignedVar(0) + " * " + SignedVar(1) + ") /" + SignedVar(2);
                                            }
                                            break;
                                        case 3:
                                            {
                                                infix = "(" + SignedVar(0) + " * " + SignedVar(2) + ") /" + SignedVar(1);
                                            }
                                            break;
                                        case 4:
                                            {
                                                infix = SignedVar(2) + " / (" + SignedVar(0) + " * " + SignedVar(1) + ")";
                                            }
                                            break;
                                        case 5:
                                            {
                                                infix = SignedVar(0) + " / (" + SignedVar(2) + " * " + SignedVar(1) + ")";
                                            }
                                            break;
                                    }
                                }
                                while (alreadyTried[method]);
                            }
                            break;
                    }
                    // check if the answer is ok
                    ok = true;
                    // check if the answer is identical to another
                    foreach (string answer in AnswersInfix)
                    {
                        // if it is, begin anew
                        if (answer != null && answer.Equals(infix))
                            ok = false;
                    }
                    // check if the answer is actually a solution
                    if (IsSolution(infix))
                        ok = false;
                }
                AnswersInfix[i] = infix;
                AnswersLaTeX[i] = "x = " + LaTeX.Format(Infix.ParseOrThrow(AnswersInfix[i]));
            }
        }

        internal override MathChallenge CreateFromSelf()
        {
            return new SolveChallenge(AnswersInfix.Length);
        }

        internal override ScrapyardButton CreateScrapyardButton()
        {
            return new ScrapyardButton(this, "scrapyardSolve.png");
        }
    }
}
