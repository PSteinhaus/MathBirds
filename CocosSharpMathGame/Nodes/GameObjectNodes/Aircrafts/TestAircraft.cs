using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestAircraft : Aircraft
    {
        // DEBUG
        internal TestWings wings;
        internal TestAircraft(CCPoint position, float rotation=0f) : base()
        {
            Position = position;
            Rotation = rotation;
            Console.WriteLine("Aircraft: " + Position);
            Body = new TestBody();
            // mount the wings
            wings = new TestWings();
            Body.MountPart(wings);
        }
    }
}
