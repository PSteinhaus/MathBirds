using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// PartMounts are positions where parts can be added (screwed onto) the part owning the PartMount.
    /// </summary>
    internal struct PartMount
    {
        /// <summary>
        /// where it is in relation to the part it belongs to;
        /// the position is not yet scaled!
        /// </summary>
        internal CCPoint Position { get; set; }
        internal Part MyPart { get; private set; }
        internal Part.Type[] AllowedTypes { get; set; }
        /// <summary>
        /// the part that is mounted on this PartMount (if there is one)
        /// </summary>
        internal Part MountedPart { get; private set; }
        internal PartMount(Part myPart, CCPoint position, params Part.Type[] allowedTypes)
        {
            MyPart = myPart;
            Position = position;
            AllowedTypes = allowedTypes;
            MountedPart = null;
        }

        /// <summary>
        /// tries to mount a given part
        /// </summary>
        /// <param name="part"></param>
        /// <returns>whether it was sucessfully mounted</returns>
        internal bool MountPart(Part part)
        {
            // if it's null abort
            if (part == null)
                return false;
            // if there is already something mounted on you abort
            if (MountedPart != null)
                return false;
            // else check if the part may be mounted here
            else if (AllowedTypes.Intersect(part.Types).Any())
            {
                MountedPart = part;
                // make sure the part is actually at the position of the mount
                // so also factor in the scaling
                part.Position = MyPart.PosLeftLower + Position;
                Console.WriteLine("MyPart Position: " + MyPart.Position);
                Console.WriteLine("Mount: " + part.Position);
                return true;
            };
            return false;
        }
    }
}
