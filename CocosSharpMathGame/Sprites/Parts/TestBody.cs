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
        internal TestBody() : base("testBody")
        {
            // add a mount point for wings at your center
            PartMounts = new PartMount[] { new PartMount(CCPoint.Zero, Type.WINGS) };
        }
    }
}
