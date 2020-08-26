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
        internal float NodeHeight { get { return MathChallenge.NodeHeight; } }
        internal float QuestionNodeHeight { get { return MathChallenge.QuestionNodeHeight; } }
        internal int Columns { get; private protected set; }
        internal static bool UnlockedAddSubSlot { get; set; } = false;
        internal static bool UnlockedMulDivSlot { get; set; } = false;
        internal static bool UnlockedSolveSlot { get; set; } = false;
        internal MathChallenge MathChallenge { get; private protected set; }
        internal MathNode[] AnswerNodes { get; private protected set; }
        internal MathNode QuestionNode { get; private protected set; }
        internal CCDrawNode DrawNode { get; private protected set; } = new CCDrawNode();
        internal CCLabel MultiplLabel { get; private protected set; } = new CCLabel("x1", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
        internal float MultiplVisible { get; private protected set; }
        internal float MultiplProgressVisible { get { return MultiplVisible % 1; } }
        internal float MultiplGoal { get { return MathChallenge.Multiplier + MathChallenge.MultiplProgress; } }
        static readonly CCSize MULTIPL_BOX_SIZE = new CCSize(200f, 200f);
        internal int Multiplier
        {
            get { return (int)MultiplVisible; }
        }
        internal MathChallengeNode(MathChallenge mathChallenge, float multiplVisible = 1, int columns = 2)
        {
            Scale = 1f;
            MultiplVisible = multiplVisible;
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
            MultiplLabel.AnchorPoint = CCPoint.AnchorMiddle;
            MultiplLabel.IsAntialiased = false;
            AddChild(DrawNode);
            AddChild(MultiplLabel);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            Schedule();
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
            // update the drawNode
            DrawNode.Position = new CCPoint(0, QuestionNode.BoundingBoxTransformedToWorld.UpperRight.Y);
            UpdateDrawNode();
            // place the Multiplier-Label
            MultiplLabel.Position = new CCPoint(bounds.Size.Width - MULTIPL_BOX_SIZE.Width / 2 + 3f, QuestionNode.BoundingBoxTransformedToWorld.UpperRight.Y + MULTIPL_BOX_SIZE.Height / 2);
            UpdateMultiplierLabel();
            // update the content size
            ContentSize = new CCSize(bounds.Size.Width, rows * answerSize.Height + QuestionNode.ContentSize.Height + MULTIPL_BOX_SIZE.Height);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            // update the visible progress of the combo bar
            if (MultiplVisible != MultiplGoal)
            {
                int multiplBefore = Multiplier;
                const float PROGRESS_INCREASE_RATE = 0.05f; // per 1/60 s
                const float BREAKOFF = 0.0001f;
                float diff = MultiplGoal - MultiplVisible;
                if (Math.Abs(diff) > BREAKOFF)
                    MultiplVisible += PROGRESS_INCREASE_RATE * diff;
                else
                    MultiplVisible = MultiplGoal;
                // if the visible combo changed update the combo label
                if (Multiplier != multiplBefore)
                {
                    UpdateMultiplierLabel();
                    // also check if a plane-slot-unlock has just been triggered
                    switch (MathChallenge)
                    {
                        case AddChallenge ac:
                        case SubChallenge sc:
                            {
                                if (!UnlockedAddSubSlot && (new AddChallenge(dummy: true)).Multiplier >= 3 && (new SubChallenge(dummy: true)).Multiplier >= 3)
                                    UnlockAddSubSlot();
                            }
                            break;
                        case MultiplyChallenge mc:
                        case DivideChallenge dc:
                            {
                                if (!UnlockedMulDivSlot && (new MultiplyChallenge(dummy: true)).Multiplier >= 4 && (new DivideChallenge(dummy: true)).Multiplier >= 4)
                                    UnlockMulDivSlot();
                            }
                            break;
                        case SolveChallenge sc:
                            {
                                if (!UnlockedSolveSlot && (new SolveChallenge(dummy: true)).Multiplier >= 4)
                                    UnlockSolveSlot();
                            }
                            break;
                    }
                }
                // visualize (update the DrawNode)
                UpdateDrawNode();
            }
        }

        internal static event EventHandler UnlockedAddSubSlotEvent;

        private void UnlockAddSubSlot()
        {
            UnlockedAddSubSlot = true;
            UnlockedAddSubSlotEvent?.Invoke(this, EventArgs.Empty);
        }

        internal static event EventHandler UnlockedMulDivSlotEvent;

        private void UnlockMulDivSlot()
        {
            UnlockedMulDivSlot = true;
            UnlockedMulDivSlotEvent?.Invoke(this, EventArgs.Empty);
        }

        internal static event EventHandler UnlockedSolveSlotEvent;

        private void UnlockSolveSlot()
        {
            UnlockedSolveSlot = true;
            UnlockedSolveSlotEvent?.Invoke(this, EventArgs.Empty);
        }

        internal static CCColor4B MultiplierBarColor(int multiplier)
        {
            CCColor4B barColor;
            switch (multiplier)
            {
                case 0:
                    {
                        barColor = CCColor4B.Black;
                    }
                    break;
                case 1:
                    {
                        barColor = CCColor4B.Red;
                    }
                    break;
                case 2:
                    {
                        barColor = CCColor4B.Yellow;
                    }
                    break;
                case 3:
                    {
                        barColor = CCColor4B.Green;
                    }
                    break;
                case 4:
                    {
                        barColor = new CCColor4B(0, 0.525f, 1f, 1f);
                    }
                    break;
                case 5:
                default:
                    {
                        barColor = new CCColor4B(0.225f, 0, 0.8f, 1f);
                    }
                    break;
                case 30:
                    {
                        barColor = CCColor4B.Transparent;
                    }
                    break;
            }
            return barColor;
        }

        internal void UpdateMultiplierLabel()
        {
            if (Multiplier != 30)
            {
                MultiplLabel.Text = "*" + Multiplier;
                MultiplLabel.Scale = 4f;
            }
            else
            {
                MultiplLabel.Text = "[";
                MultiplLabel.Scale = 6f;
            }
        }

        internal void UpdateDrawNode()
        {
            var bounds = VisibleBoundsWorldspace;
            const float LINE_WIDTH = 20f;
            const float BAR_WIDTH = 40f;
            const float BAR_BOX_HEIGHT = BAR_WIDTH + LINE_WIDTH * 2;
            float barBoxWidth = bounds.Size.Width - MULTIPL_BOX_SIZE.Width;
            DrawNode.Clear();
            // draw the combo bar box and the multiplier box
            DrawNode.DrawRect(new CCRect(0, 0, barBoxWidth, BAR_BOX_HEIGHT), CCColor4B.White);
            DrawNode.DrawRect(new CCRect(barBoxWidth, 0, MULTIPL_BOX_SIZE.Width, MULTIPL_BOX_SIZE.Height), CCColor4B.White);
            DrawNode.DrawRect(new CCRect(barBoxWidth + LINE_WIDTH, LINE_WIDTH, MULTIPL_BOX_SIZE.Width - 2*LINE_WIDTH, MULTIPL_BOX_SIZE.Height - 2*LINE_WIDTH), CCColor4B.Black);
            // draw the combo bar
            CCRect barFull = new CCRect(LINE_WIDTH, LINE_WIDTH, barBoxWidth - 2 * LINE_WIDTH, BAR_WIDTH);
            CCRect bar = new CCRect(LINE_WIDTH, LINE_WIDTH, barFull.Size.Width * MultiplProgressVisible, BAR_WIDTH);
            // choose the color
            var barColor = MultiplierBarColor(Multiplier);
            var barColorBehind = MultiplierBarColor(Multiplier - 1);
            DrawNode.DrawRect(barFull, barColorBehind);
            DrawNode.DrawRect(bar, barColor);
            //DrawNode.DrawPolygon(new CCPoint[] { CCPoint.Zero, new CCPoint(barBoxWidth, 0), new CCPoint(barBoxWidth, 0), new CCPoint(barBoxWidth, BAR_BOX_HEIGHT), new CCPoint(0, BAR_BOX_HEIGHT) }, 4, CCColor4B.Transparent, LINE_WIDTH, CCColor4B.White);    // bar box
            //DrawNode.DrawPolygon(new CCPoint[] { new CCPoint(barBoxWidth, 0), new CCPoint(bounds.Size.Width, 0), new CCPoint(bounds.Size.Width, MULTIPL_BOX_SIZE.Height), new CCPoint(barBoxWidth, MULTIPL_BOX_SIZE.Height) }, 4, CCColor4B.Transparent, LINE_WIDTH, CCColor4B.White);    // multiplier box
        }

        internal event EventHandler<bool> AnswerChosenEvent;
        internal void AnswerChosen(string mathInfix)
        {
            // as soon as the player first answers a math challenge of this type unlock it
            if (MathChallenge.Locked)
                MathChallenge.Locked = false;
            AnswerChosenEvent?.Invoke(this, MathChallenge.TrySolveWith(mathInfix));
        }
    }
}
