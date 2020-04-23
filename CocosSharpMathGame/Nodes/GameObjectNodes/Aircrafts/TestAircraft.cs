using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    class TestAircraft : Aircraft
    {
        TestAircraft(CCPoint position, float rotation)
        {
            Body = new TestBody();
            // mount the wings
            Body.MountPart(new TestWings());
        }
    }
}
