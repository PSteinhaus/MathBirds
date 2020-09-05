using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class BodyBat : Part
    {
        public BodyBat() : base("bodyBat.png")
        {
                SetHealthAndMaxHealth(4);
                NormalAnchorPoint = CCPoint.AnchorMiddle;
                // set your types
                Types = new Type[] { Type.BODY };

                // add mount points for two single wings
                var wingMount1 = new PartMount(this, new CCPoint((ContentSize.Width * 0.75f), (ContentSize.Height / 2) + 3), Type.SINGLE_WING);
                var wingMount2 = new PartMount(this, new CCPoint((ContentSize.Width * 0.75f), (ContentSize.Height / 2) - 3), Type.SINGLE_WING);
                var doubleWingMount = new PartMount(this, new CCPoint((ContentSize.Width * 0.62f), ContentSize.Height / 2), Type.WINGS);
                doubleWingMount.Dz = 2;
                doubleWingMount.PossiblyBlockingPartMounts.Add(wingMount1);
                doubleWingMount.PossiblyBlockingPartMounts.Add(wingMount2);
                wingMount1.PossiblyBlockingPartMounts.Add(doubleWingMount);
                wingMount2.PossiblyBlockingPartMounts.Add(doubleWingMount);
                // add mount points for two rudders
                var rudderMount1 = new PartMount(this, new CCPoint(1.5f, (ContentSize.Height / 2) + 1), Type.RUDDER);
                var rudderMount2 = new PartMount(this, new CCPoint(1.5f, (ContentSize.Height / 2) - 1), Type.RUDDER);
                // add a mount for a rotor and one for a weapon
                var rotorMount = new PartMount(this, new CCPoint(ContentSize.Width - 3, ContentSize.Height / 2), Type.ROTOR);
                var gunMount = new PartMount(this, new CCPoint(ContentSize.Width - 3, ContentSize.Height / 2), Type.GUN);
                gunMount.Dz = -1;

                PartMounts = new PartMount[] { wingMount1, wingMount2, doubleWingMount, rudderMount1, rudderMount2, rotorMount, gunMount };

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(25);
        }
    }

    internal class BodyBatShiny : Part
    {
        public BodyBatShiny() : base("bodyBatShiny.png")
        {
                SetHealthAndMaxHealth(7);
                NormalAnchorPoint = CCPoint.AnchorMiddle;
                // set your types
                Types = new Type[] { Type.BODY };

                // add mount points for two single wings
                var wingMount1 = new PartMount(this, new CCPoint((ContentSize.Width * 0.75f), (ContentSize.Height / 2) + 3), Type.SINGLE_WING);
                var wingMount2 = new PartMount(this, new CCPoint((ContentSize.Width * 0.75f), (ContentSize.Height / 2) - 3), Type.SINGLE_WING);
                var doubleWingMount = new PartMount(this, new CCPoint((ContentSize.Width * 0.72f), ContentSize.Height / 2), Type.WINGS);
                doubleWingMount.PossiblyBlockingPartMounts.Add(wingMount1);
                doubleWingMount.PossiblyBlockingPartMounts.Add(wingMount2);
                wingMount1.PossiblyBlockingPartMounts.Add(doubleWingMount);
                wingMount2.PossiblyBlockingPartMounts.Add(doubleWingMount);
                // add mount points for two rudders
                var rudderMount1 = new PartMount(this, new CCPoint(1.5f, (ContentSize.Height / 2) + 1), Type.RUDDER);
                var rudderMount2 = new PartMount(this, new CCPoint(1.5f, (ContentSize.Height / 2) - 1), Type.RUDDER);
                // add a mount for a rotor and one for a weapon
                var rotorMount = new PartMount(this, new CCPoint(ContentSize.Width - 3, ContentSize.Height / 2), Type.ROTOR);
                var gunMount = new PartMount(this, new CCPoint(ContentSize.Width - 3, ContentSize.Height / 2), Type.GUN);
                gunMount.Dz = 1;

                PartMounts = new PartMount[] { wingMount1, wingMount2, doubleWingMount, rudderMount1, rudderMount2, rotorMount, gunMount };

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(15);
        }
    }
}
