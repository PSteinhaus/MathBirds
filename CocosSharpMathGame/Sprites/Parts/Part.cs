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
    abstract internal class Part : CCSprite
    {
        /// <summary>
        /// Parts can have multiples Types.
        /// THEY MUST HAVE AT LEAST ONE TYPE
        /// </summary>
        internal enum Type
        {
            BODY, SINGLE_WING, WINGS, WEAPON 
        }
        internal Type[] Types { get; set; }
        /// <summary>
        /// Holds the SpriteFrames for all parts
        /// </summary>
        static protected CCSpriteSheet spriteSheet = new CCSpriteSheet("parts.plist");
        protected PartMount[] PartMounts { get; set; }
        /// <summary>
        /// searches recursively and returns all parts that are mounted on this part including itself
        /// </summary>
        internal IEnumerable<Part> TotalParts
        {
            get
            {
                List<Part> totalParts = new List<Part> { this };
                foreach (PartMount partMount in PartMounts)
                {
                    if (partMount.MountedPart != null)
                        totalParts.AddRange(partMount.MountedPart.TotalParts);
                }
                return totalParts;
            }
        }
        protected Part(string textureName) : base(spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(textureName)))
        {
            IsAntialiased = false;
            AnchorPoint = CCPoint.AnchorLowerLeft;
            Console.WriteLine("Part ContentSize: " + ContentSize);
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

        internal bool MountPart (int mountIndex, Part part, int dz = -1)
        {
            bool mounted = false;
            // check the index
            if (mountIndex < PartMounts.Length)
                mounted = PartMounts[mountIndex].MountPart(part);
            // if it was sucessfully mounted add it as a child of your parent (the aircraft)
            if (mounted)
            {
                Parent.AddChild(part, zOrder: this.ZOrder + dz);
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
