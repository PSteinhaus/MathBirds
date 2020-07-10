using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Xamarin.Forms;

namespace CocosSharpMathGame
{
    internal class MathNode : UIElementNode
    {
        private protected const float BORDER = 12f;
        internal CCDrawNode DrawNode { get; private protected set; } = new CCDrawNode();
        internal MathSprite MathSprite { get; private protected set; }
        internal MathChallengeNode MathChallengeNode { get; private protected set; }
        internal MathNode(string mathInfix, string mathLaTeX, MathChallengeNode mathChallengeParent)
        {
            MathChallengeNode = mathChallengeParent;
            Scale = 1f;
            AnchorPoint = CCPoint.AnchorMiddle;
            AddChild(DrawNode);
            MathSprite = new MathSprite(mathInfix, mathLaTeX);
            AddChild(MathSprite, 1);
            MakeClickable(touchMustEndOnIt: false);
        }
        internal void SetSize(CCSize size)
        {
            ContentSize = size;
            DrawNode.Clear();
            DrawNode.DrawRect(new CCRect(0, 0, size.Width, size.Height), CCColor4B.White);
            DrawNode.DrawRect(new CCRect(BORDER, BORDER, size.Width-2*BORDER, size.Height-2*BORDER), CCColor4B.Black);
            var mathSize = new CCSize(size.Width - 12 * BORDER, size.Height - 12 * BORDER);
            MathSprite.FitToBox(mathSize);
            MathSprite.Position = (CCPoint)ContentSize/2;
        }

        private protected override void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            // make the button a bit smaller
            AddAction(new CCScaleTo(0.05f, 0.8f));
        }

        private protected override void OnTouchesEndedUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            // reset the scale
            AddAction(new CCScaleTo(0.05f, 1f));
            if (TouchIsOnIt(touches[0]))
                MathChallengeNode.AnswerChosen(MathSprite.MathInfix);
        }
    }
}
