using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class MathChallengeNode : GameObjectNode
    {
        private bool pressable = true;
        internal bool Pressable
        {
            get { return pressable; }
            set
            {
                pressable = value;
                foreach (var answerNode in AnswerNodes)
                    answerNode.Pressable = pressable;
            }
        }
        internal float NodeHeight { get; set; } = 220f;
        internal float QuestionNodeHeight { get; set; } = 240f;
        internal int Columns { get; private protected set; }
        internal MathChallenge MathChallenge { get; private protected set; }
        internal MathNode[] AnswerNodes { get; private protected set; }
        internal MathNode QuestionNode { get; private protected set; }
        internal MathChallengeNode(MathChallenge mathChallenge, int columns = 2)
        {
            Scale = 1f;
            AnchorPoint = CCPoint.AnchorLowerLeft;
            MathChallenge = mathChallenge;
            Columns = columns;

            QuestionNode = new MathNode(mathChallenge.ChallengeInfix, mathChallenge.ChallengeLaTeX, this);
            AddChild(QuestionNode);
            QuestionNode.Pressable = false; // there is no reason the question node should be allowed to be pressed
            AnswerNodes = new MathNode[mathChallenge.AnswersLaTeX.Length];
            for (int i=0; i<AnswerNodes.Length; i++)
            {
                AnswerNodes[i] = new MathNode(mathChallenge.AnswersInfix[i], mathChallenge.AnswersLaTeX[i], this);
                AddChild(AnswerNodes[i]);
            }
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            var bounds = VisibleBoundsWorldspace;
            var answerSize = new CCSize(bounds.Size.Width / Columns, NodeHeight);
            int rows = (AnswerNodes.Length - 1) / Columns + 1;
            // position and scale everything correctly
            QuestionNode.SetSize(new CCSize(bounds.Size.Width, QuestionNodeHeight));
            QuestionNode.Position = new CCPoint(0, rows * answerSize.Height) + (CCPoint)QuestionNode.ContentSize/2;
            for (int i=0; i<AnswerNodes.Length; i++)
            {
                AnswerNodes[i].SetSize(answerSize);
                AnswerNodes[i].Position = new CCPoint((i % Columns) * answerSize.Width, (rows - (i / Columns) - 1) * answerSize.Height) + (CCPoint)AnswerNodes[i].ContentSize/2;
            }
            ContentSize = new CCSize(bounds.Size.Width, rows * answerSize.Height + QuestionNode.ContentSize.Height);
        }

        internal event EventHandler<bool> AnswerChosenEvent;
        internal void AnswerChosen(string mathInfix)
        {
            AnswerChosenEvent?.Invoke(this, MathChallenge.IsSolution(mathInfix));
        }
    }
}
