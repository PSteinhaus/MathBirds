using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestBody : Part
    {
        internal TestBody() : base("testBody.png")
        {
            // set your types
            Types = new Type[] { Type.BODY };
            // add a mount point for wings at your center
            PartMounts = new PartMount[] { new PartMount(new CCPoint((ContentSize.Width / 2), (ContentSize.Height / 2)), Type.WINGS) };
        }
    }
}
