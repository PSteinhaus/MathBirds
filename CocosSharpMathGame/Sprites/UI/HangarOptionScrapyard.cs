using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class HangarOptionScrapyard : HangarOptionNode
    {
        internal HangarOptionScrapyard() : base()
        {
            FitToWidth(220f);
            // add text
            var label = new CCLabel("Scrapyard", "EarlyGameBoy", 12, CCLabelFormat.SpriteFont);
            label.Position = (CCPoint)ContentSize / 2;
            label.Color = CCColor3B.White;
            label.Scale = 1.125f;
            label.IsAntialiased = false;
            AddChild(label);
        }
    }
}
