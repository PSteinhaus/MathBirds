using System;
using System.Collections.Generic;
using System.IO;
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
        protected enum StreamEnum : byte
        {
            STOP = 0, NAME = 1, LOCKED = 2,
            COMBO = 3
        }
        internal void WriteToStream(BinaryWriter writer)
        {
            // write the string containing the full name of the challenge class
            writer.Write((byte)StreamEnum.NAME);
            string name = GetType().AssemblyQualifiedName;
            writer.Write(name);
            // write whether you're locked
            writer.Write((byte)StreamEnum.LOCKED);
            writer.Write(Locked);
            // write the current combo
            writer.Write((byte)StreamEnum.COMBO);
            writer.Write(Combo);
            //stop
            writer.Write((byte)StreamEnum.STOP);
        }

        internal void ReadFromStream(BinaryReader reader)
        {
            bool reading = true;
            MathChallenge dummy = null;
            while (reading)
            {
                StreamEnum nextEnum = (StreamEnum)reader.ReadByte();
                switch (nextEnum)
                {
                    case StreamEnum.NAME:
                        {
                            // read the string containing the full name of the challenge class
                            string className = reader.ReadString();
                            dummy = (MathChallenge)TypeHelper.CreateFromTypeName(className);
                        }
                        break;
                    case StreamEnum.LOCKED:
                        {
                            bool locked = reader.ReadBoolean();
                            if (dummy != null)
                                dummy.Locked = locked;
                        }
                        break;
                    case StreamEnum.COMBO:
                        {
                            int combo = reader.ReadInt32();
                            if (dummy != null)
                                dummy.Combo = combo;
                        }
                        break;
                    case StreamEnum.STOP:
                        reading = false;
                        break;
                }
            }
        }
    }
}
