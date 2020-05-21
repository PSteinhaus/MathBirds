using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestEngine : Part
    {
        internal TestEngine() : base("testEngine.png")
        {
            // set your types
            Types = new Type[] { Type.ENGINE };
            AnchorPoint = CCPoint.AnchorMiddle;

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint(ContentSize.Width / 2, ContentSize.Height / 2, 30) };

            // specify the collision polygon
            CollisionType = new CollisionTypePolygon(new Polygon(DiamondCollisionPoints()));

            // give the engine maneuver abilities
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.15f, (float)Math.Pow(10, 5) * 1.5f);//, (float)Math.Pow(10, 5)*0.1f, (float)Math.Pow(10, 5));
        }


    }
}
