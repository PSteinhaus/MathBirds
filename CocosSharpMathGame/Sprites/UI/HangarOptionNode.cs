using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class HangarOptionNode : HorizontalScalingButton
    {
        internal HangarOptionNode() : base("hangarOptionStart.png", "hangarOptionMid.png", "hangarOptionEnd.png")
        {
            FitToWidth(200f);
        }
    }
}
