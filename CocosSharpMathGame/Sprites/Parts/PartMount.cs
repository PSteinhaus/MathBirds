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
    internal class PartMount
    {
        /// <summary>
        /// where it is in relation to the PosLeftLower of the part it belongs to;
        /// the position is not yet scaled!
        /// </summary>
        internal CCPoint Position { get; set; }
        internal int Dz { get; set; }
        internal Part MyPart { get; private set; }
        internal bool Available
        {
            get
            {
                if (MountedPart == null)
                {
                    foreach (var partMount in PossiblyBlockingPartMounts)
                        if (partMount.MountedPart != null)
                            return false;
                    return true;
                }
                return false;
            }
        }
        internal float NullRotation { get; set; }
        internal Part.Type[] AllowedTypes { get; set; }
        internal Part.SizeType[] AllowedSizes { get; set; } = new Part.SizeType[1] { Part.SizeType.REGULAR };
        /// <summary>
        /// Sometimes PartMounts can block each other, if a part is mounted on one of them. Add all PartMounts here that should block you when something is mounted on them.
        /// </summary>
        internal List<PartMount> PossiblyBlockingPartMounts { get; set; } = new List<PartMount>();
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
            NullRotation = 0;
            Dz = -1;
        }

        internal void UpdateMountedPartPosition()
        {
            MountedPart.Position = MyPart.PosLeftLower + Position;
            MountedPart.NullRotation = NullRotation;
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
            // only continue if you are really available
            if (!Available)
                return false;
            // else check if the part may be mounted here
            else if (AllowedTypes.Intersect(part.Types).Any() && AllowedSizes.Contains(part.MySizeType))
            {
                MountedPart = part;
                // make sure the part is actually at the position of the mount
                UpdateMountedPartPosition();
                //Console.WriteLine("MyPart Position: " + MyPart.Position);
                //Console.WriteLine("Mount: " + part.Position);
                return true;
            };
            return false;
        }

        internal Part UnmountPart()
        {
            var mountedPart = MountedPart;
            if (mountedPart == null)
                return null;
            // first unflip the part if it is flipped
            if (mountedPart.Flipped)
                mountedPart.Flip();
            mountedPart.MountParent = null;
            // actually remove the part from this mount point
            MountedPart = null;
            if (MyPart.Parent is Aircraft aircraft)
            {
                // let the aircraft that the part was mounted on react to the loss
                aircraft.RemoveChild(mountedPart);
                aircraft.PartsChanged();
            }
            // return the formerly mounted part
            return mountedPart;
        }
    }
}
