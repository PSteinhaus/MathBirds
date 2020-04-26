using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestWings : Part
    {
        internal TestWings() : base("testWings.png")
        {
            // set your types
            Types = new Type[] { Type.WINGS };
            AnchorPoint = CCPoint.AnchorMiddle;
        }
    }
}
