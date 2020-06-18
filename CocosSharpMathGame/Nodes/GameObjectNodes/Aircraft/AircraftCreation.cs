﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal partial class Aircraft
    {
        internal static Aircraft CreateTestAircraft(bool withWeapon=true)
        {
            Aircraft aircraft = new Aircraft();
            //IsManeuverPolygonDrawn = true;
            aircraft.Body = new TestBody();
            // mount the wings
            var wings = new TestDoubleWing();
            aircraft.Body.MountPart(wings);
            // mount the rotor
            aircraft.Body.MountPart(new TestRotor());
            // and the rudders
            aircraft.Body.MountPart(new TestRudder());
            aircraft.Body.MountPart(new TestRudder());
            //Body.MountPart(new TestEngineStrong());
            if (withWeapon)
            {
                // give both wings guns
                //wings.MountPart(new TestWeapon());
                wings.MountPart(new TestWeapon());
            }
            return aircraft;
            /*
            foreach (var part in TotalParts)
                Console.WriteLine("Part bounding box: "+part.BoundingBoxTransformedToParent);

            // lets create some maneuver polygon
            var d = ScaledContentSize.Width * 0.5f; // use some relative length as measure
            var reach = 3;
            var controlPoints = new CCPoint[7];
            controlPoints[0] = new CCPoint(3*d, 4*d);
            controlPoints[1] = new CCPoint(3*d *reach, 4*d * reach);
            var angle = controlPoints[1].Angle;
            controlPoints[2] = CCPoint.RotateByAngle(controlPoints[1], CCPoint.Zero, -angle*0.5f);
            controlPoints[3] = CCPoint.RotateByAngle(controlPoints[1], CCPoint.Zero, -angle);
            controlPoints[4] = CCPoint.RotateByAngle(controlPoints[1], CCPoint.Zero, -angle*1.5f);
            controlPoints[5] = new CCPoint(3*d * reach, -4 * d * reach);
            controlPoints[6] = new CCPoint(3*d, -4 * d);
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            maneuverPolygon.SpecifySpline(1, 5, 25);
            maneuverPolygon.ConstructPolygon();
            */
            /*
            var controlPoints = new CCPoint[]
            { new CCPoint(2*d, 3*d), new CCPoint(0, 3.5f*d), new CCPoint(-2*d, 3*d),
              new CCPoint(-d, d), new CCPoint(0,d*1.2f), new CCPoint(d, d) };
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            maneuverPolygon.SpecifySpline(0, 2, 15);
            maneuverPolygon.SpecifySpline(3, 5, 15);
            maneuverPolygon.ConstructPolygon();
            // turn it 90 degrees because 0° means EAST now...
            maneuverPolygon.RotateBy(90f);
            */
            /*
            // now let's try a MASSIVE polygon, allowing the plane to move almost anywhere
            var d = ScaledContentSize.Width * 1.5f; // use some relative length as measure
            var controlPoints = new CCPoint[]
            { new CCPoint(-10*d, -10*d), new CCPoint(-10*d, 10*d), new CCPoint(10*d, 10*d), new CCPoint(10*d, -10*d) };
            var maneuverPolygon = new PolygonWithSplines(controlPoints);
            */

            //UpdateManeuverPolygonToThis(maneuverPolygon);
            // this is a test plane so I want to see the polygon too
            //IsManeuverPolygonDrawn = true;

            // ok... since this doesn't work lets try to draw it like this:
            //var drawNode = maneuverPolygon.CreateDrawNode();
            //AddChild(drawNode);
            //drawNode.Scale = 1 / Constants.STANDARD_SCALE;
        }
    }
}