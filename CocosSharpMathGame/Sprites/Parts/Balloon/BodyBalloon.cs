using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class BodyBalloon : Part
    {
        public BodyBalloon() : base("bodyBalloon.png")
        {
                SetHealthAndMaxHealth(25);
                NormalAnchorPoint = CCPoint.AnchorMiddle;
                // set your types
                Types = new Type[] { Type.BODY };

                // add a central mount point for a weapon
                var weaponMount = new PartMount(this, (CCPoint)ContentSize / 2, Type.GUN);
                weaponMount.MaxTurningAngle = 115f;
                weaponMount.Dz = -1;

                // add mount points for two rotors
                var rotorMount1 = new PartMount(this, new CCPoint(ContentSize.Width / 2, 4), Type.ROTOR);
                var rotorMount2 = new PartMount(this, new CCPoint(ContentSize.Width / 2, ContentSize.Height - 4), Type.ROTOR);
                rotorMount1.Dz = 1;
                rotorMount2.Dz = 1;

                PartMounts = new PartMount[] { weaponMount, rotorMount1, rotorMount2 };

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(400);
        }
    }

    internal class BodyBalloonShiny : Part
    {
        public BodyBalloonShiny() : base("bodyBalloonShiny.png")
        {
                SetHealthAndMaxHealth(40);
                NormalAnchorPoint = CCPoint.AnchorMiddle;
                // set your types
                Types = new Type[] { Type.BODY };

                // add a central mount point for a weapon
                var weaponMount = new PartMount(this, (CCPoint)ContentSize / 2, Type.GUN);
                weaponMount.MaxTurningAngle = 115f;
                weaponMount.Dz = -1;

                // add mount points for two rotors
                var rotorMount1 = new PartMount(this, new CCPoint(ContentSize.Width / 2, 4), Type.ROTOR);
                var rotorMount2 = new PartMount(this, new CCPoint(ContentSize.Width / 2, ContentSize.Height - 4), Type.ROTOR);
                rotorMount1.Dz = 1;
                rotorMount2.Dz = 1;

                PartMounts = new PartMount[] { weaponMount, rotorMount1, rotorMount2 };

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(400);
        }
    }
}
