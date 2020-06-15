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
        internal GOButton() : base("goButton.png", false)
        {
            Scale = Constants.STANDARD_SCALE * 2;
        }

        private protected override void ButtonEnded(CCTouch touch)
        {
            // TODO: switch to the PlayLayer (i.e. start the game!)
        }
    }
}
