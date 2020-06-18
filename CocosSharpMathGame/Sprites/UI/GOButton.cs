using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class GOButton : Button
    {
        private CCLabel Label { get; set; }
        internal GOButton() : base("goButton.png", false)
        {
            Scale = Constants.STANDARD_SCALE * 2;
            // add text
            Label = new CCLabel("GO", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
            Label.Position = (CCPoint)ContentSize / 2;
            Label.PositionX += 1f;
            Label.Color = CCColor3B.White;
            Label.Scale = 0.75f;
            Label.IsAntialiased = false;
            AddChild(Label);
        }

        internal override void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesBegan(touches, touchEvent);
            if (touches.Count > 0)
            {
                // turn darker when pressed
                Label.Color = CCColor3B.Gray;
            }
        }

        internal override void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesEnded(touches, touchEvent);
            if (touches.Count > 0)
            {
                // turn back to original color when released
                Label.Color = CCColor3B.White;
            }
        }

        internal override void OnTouchesCancelled(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesCancelled(touches, touchEvent);
            if (touches.Count > 0)
            {
                // turn back to original color when released
                Label.Color = CCColor3B.White;
            }
        }

        private protected override void ButtonEnded(CCTouch touch)
        {
            // TODO: switch to the PlayLayer (i.e. start the game!)
        }
    }
}
