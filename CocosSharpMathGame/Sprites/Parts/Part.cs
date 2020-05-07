using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Parts are what Aircrafts are made of.
    /// They are visible.
    /// </summary>
    abstract internal class Part : GameObjectSprite
    {
        /// <summary>
        /// returns the position of this part relative to the aircraft;
        /// to be precise it is the difference between the left lower corner of the aircraft and the left lower corner of the part;
        /// </summary>
        internal CCPoint PosLeftLower
        {
            get
            {
                return new CCPoint(BoundingBoxTransformedToParent.MinX, BoundingBoxTransformedToParent.MinY);
            }
        }
        /// <summary>
        /// whether and how this part may actively contribute to the maneuverability of the aircraft
        /// </summary>
        internal ManeuverAbility ManeuverAbility { get; private protected set; } = null;
        /// <summary>
        /// Parts are physical objects. They have mass, distributed in space. This mass (and its distribution) is modeled here.
        /// The positions given here start at (0,0) (lower left corner of the part) and end at (ContentSize.Width, ContentSize.Height).
        /// </summary>
        internal MassPoint[] MassPoints { get; private protected set; }
        /// <summary>
        /// The mass of just this single part (without mounted parts)
        /// </summary>
        internal float MassSingle
        {
            get
            {
                float mass = 0;
                foreach (var massPoint in MassPoints)
                    mass += massPoint.Mass;
                return mass;
            }
        }
        /// <summary>
        /// The mass of just this part including mounted parts
        /// </summary>
        internal float TotalMass
        {
            get
            {
                float mass = MassSingle;
                foreach (var part in MountedParts)
                    mass += part.TotalMass;
                return mass;
            }
        }
        /// <summary>
        /// Parts can have multiples Types.
        /// THEY MUST HAVE AT LEAST ONE TYPE
        /// </summary>
        internal enum Type
        {
            BODY, SINGLE_WING, WINGS, WEAPON, ENGINE
        }
        internal Type[] Types { get; set; }
        /// <summary>
        /// Holds the SpriteFrames for all parts
        /// </summary>
        static protected CCSpriteSheet spriteSheet = new CCSpriteSheet("parts.plist");
        internal Part MountParent { get; set; } = null;
        protected PartMount[] PartMounts { get; set; }
        /// <summary>
        /// searches recursively and returns all parts that are mounted on this part including itself
        /// </summary>
        internal IEnumerable<Part> TotalParts
        {
            get
            {
                List<Part> totalParts = new List<Part> { this };
                if (PartMounts != null)
                foreach (PartMount partMount in PartMounts)
                {
                    if (partMount.MountedPart != null)
                        totalParts.AddRange(partMount.MountedPart.TotalParts);
                }
                return totalParts;
            }
        }
        internal IEnumerable<Part> MountedParts
        {
            get
            {
                List<Part> mountedParts = new List<Part>();
                if (PartMounts != null)
                    foreach (PartMount partMount in PartMounts)
                    {
                        if (partMount.MountedPart != null)
                            mountedParts.Add(partMount.MountedPart);
                    }
                return mountedParts;
            }
        }
        /// <summary>
        /// The center of mass when calculated only considering the masspoints and mass of this single part (without mounted parts)
        /// </summary>
        internal CCPoint CenterOfMassSingle
        {
            get
            {
                float myMass = MassSingle;
                float x = PosLeftLower.X;
                float y = PosLeftLower.Y;
                foreach (var massPoint in MassPoints)
                {
                    x += (massPoint.Mass / myMass) * massPoint.X;
                    y += (massPoint.Mass / myMass) * massPoint.Y;
                }
                return new CCPoint(x, y);
            }
        }
        /// <summary>
        /// The center of mass with consideration of all mounted parts.
        /// (0,0) is the left lower corner of the part.
        /// </summary>
        internal CCPoint CenterOfMass
        {
            get
            {
                CCPoint centerOfMassSingle = CenterOfMassSingle;
                if (MountedParts.Any())
                {
                    float totalMass = TotalMass;
                    float x = (MassSingle / totalMass) * centerOfMassSingle.X;
                    float y = (MassSingle / totalMass) * centerOfMassSingle.Y;
                    // calculate the center of mass as weighted sum of the centers of mass from the parts
                    foreach (var part in MountedParts)
                    {
                        CCPoint centerOfMass = part.CenterOfMass;
                        CCPoint leftLowerCornerOfPart = part.PosLeftLower;
                        // remember to factor in the relative position of the part
                        x += (part.TotalMass / totalMass) * (centerOfMass.X);
                        y += (part.TotalMass / totalMass) * (centerOfMass.Y);
                    }
                    return new CCPoint(x, y);
                }
                else
                    return centerOfMassSingle;
            }
        }
        /// <summary>
        /// Aircrafts can be rotated by force. How much force is necessary for a certain rotation is defined by the moment of intertia.
        /// This property can be calculated for any part, but it is mainly supposed to be called for the body of an aircraft.
        /// </summary>
        internal float MomentOfInertia
        {
            get
            {
                // calculate the moment of inertia as the sum over all mass points multiplied with the square of their distance to the center of mass
                CCPoint centerOfMass = CenterOfMass;
                float momentOfInertia = 0;
                //IEnumerable<Part> totalParts = TotalParts;
                foreach (var part in TotalParts)
                {
                    CCPoint leftLowerCornerOfPart = part.PosLeftLower;
                    foreach (var massPoint in part.MassPoints)
                    {
                        CCPoint totalMassPosition = new CCPoint(leftLowerCornerOfPart.X + massPoint.X, leftLowerCornerOfPart.Y + massPoint.Y);
                        float distance = CCPoint.Distance(centerOfMass, totalMassPosition) * Constants.STANDARD_SCALE;
                        momentOfInertia += massPoint.Mass * distance*distance;
                    }
                }
                return momentOfInertia;
            }
        }
        protected Part(string textureName) : base(spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(textureName)))
        {
            Scale = 1; // Parts are usually scaled by the aircrafts owning them
            AnchorPoint = CCPoint.AnchorLowerLeft;
            //Console.WriteLine("Part ContentSize: " + ContentSize);
        }

        /// <summary>
        /// tries to mount a given part on a PartMount of this Part
        /// </summary>
        /// <param name="partMount"></param>
        /// <param name="part"></param>
        /// <param name="dz">the difference in ZOrder between the mounted part and this part</param>
        /// <returns>whether it was successfully mounted</returns>
        internal bool MountPart (PartMount partMount, Part part, int dz = -1)
        {
            // check whether the PartMount is actually one of yours and proceed if true
            for (int i=0; i<PartMounts.Length; i++)
                if (PartMounts[i].Equals(partMount))
                    return MountPart(i, part, dz);
            return false;
        }
        /// <summary>
        /// tries to mount a given part on a PartMount of this Part
        /// (this one actually does the work)
        /// </summary>
        /// <param name="mountIndex"></param>
        /// <param name="part"></param>
        /// <param name="dz"></param>
        /// <returns></returns>
        internal bool MountPart (int mountIndex, Part part, int dz = -1)
        {
            bool mounted = false;
            // check the index
            if (mountIndex < PartMounts.Length)
                mounted = PartMounts[mountIndex].MountPart(part);
            // if it was sucessfully mounted add it as a child of your parent (the aircraft)
            // and tell it that you're its mount-parent now
            if (mounted)
            {
                Parent.AddChild(part, zOrder: this.ZOrder + dz);
                part.MountParent = this;
                (Parent as Aircraft).PartsChanged();
            }
            return mounted;
        }

        internal bool MountPart (Part part, int dz = -1)
        {
            bool mounted = false;
            // try to mount the part somewhere and stop if you succeed
            for (int i=0; i<PartMounts.Length && mounted==false; i++)
                mounted = MountPart(i, part, dz);
            return mounted;
        }
    }
}
