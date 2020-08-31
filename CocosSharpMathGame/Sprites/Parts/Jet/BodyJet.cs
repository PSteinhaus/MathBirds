using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class BodyJet : Part
    {
        public BodyJet() : base("bodyJet.png")
        {
            SetHealthAndMaxHealth(40);
            NormalAnchorPoint = CCPoint.AnchorMiddle;
            // set your types
            Types = new Type[] { Type.BODY };

            // add mount points for two single wings
            var wingMount1 = new PartMount(this, new CCPoint(ContentSize.Width * 0.4f, (ContentSize.Height / 2) + 5), Type.SINGLE_WING);
            var wingMount2 = new PartMount(this, new CCPoint(ContentSize.Width * 0.4f, (ContentSize.Height / 2) - 5), Type.SINGLE_WING);
            // add mount points for two rudders
            var rudderMount1 = new PartMount(this, new CCPoint(6f, (ContentSize.Height / 2) + 2), Type.RUDDER);
            var rudderMount2 = new PartMount(this, new CCPoint(6f, (ContentSize.Height / 2) - 2), Type.RUDDER);

            PartMounts = new PartMount[] { wingMount1, wingMount2, rudderMount1, rudderMount2 };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(80);

            // add a special maneuver ability! (jet propulsion :) )
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 2.75f, (float)Math.Pow(10, 5) * 35.0f);
            ManeuverAbility.CloudTailNode.ReferenceSize *= 2f;
        }
    }
}
