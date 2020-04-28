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
        internal TestAircraft() : base()
        {
            Body = new TestBody();
            // mount the wings
            wings = new TestWings();
            Body.MountPart(wings);

            // lets create some maneuver polygon
            var d = ScaledContentSize.Width; // use some relative length as measure
            var controlPoints = new CCPoint[]
            { new CCPoint(2*d, 4*d), new CCPoint(0, 5*d), new CCPoint(-2*d, 4*d),
              new CCPoint(-d/2, d/4), new CCPoint(0,d/2), new CCPoint(d/2, d/4) };
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            maneuverPolygon.SpecifySpline(0, 2, 15);
            maneuverPolygon.SpecifySpline(3, 5, 15);
            maneuverPolygon.ConstructPolygon();
            // turn it 90 degrees because 0° means EAST now...
            maneuverPolygon.RotateBy(90f);
            UpdateManeuverPolygonToThis(maneuverPolygon);
            // this is a test plane so I want to see the polygon too
            IsManeuverPolygonDrawn = true;

            // ok... since this doesn't work lets try to draw it like this:
            //var drawNode = maneuverPolygon.CreateDrawNode();
            //AddChild(drawNode);
            //drawNode.Scale = 1 / Constants.STANDARD_SCALE;
        }
    }
}
