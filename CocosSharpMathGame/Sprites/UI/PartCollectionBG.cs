using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosSharpMathGame
{
    internal class PartCollectionBG : HorizontalScalingButton
    {
        internal PartCollectionBG() : base("hangarOptionMid.png", "hangarOptionMid.png", "hangarOptionMid.png")
        {
            FitToWidth(Constants.COCOS_WORLD_WIDTH / Constants.STANDARD_SCALE);
        }
    }
}
