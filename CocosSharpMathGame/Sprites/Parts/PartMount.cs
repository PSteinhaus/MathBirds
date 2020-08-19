using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Xamarin.Forms.Internals;

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
        internal float MaxTurningAngle { get; set; } = 10f;
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
            Dz = 1;
        }

        internal void UpdateMountedPartPosition()
        {
            if (MountedPart == null) return;
            MountedPart.Position = AircraftPosition;
            MountedPart.NullRotation = NullRotation;
            foreach (var mountPoint in MountedPart.PartMounts)
                mountPoint.UpdateMountedPartPosition();
        }
        internal CCPoint AircraftPosition
        {
            get { return MyPart.PosLeftLower + Position; }
        }
        internal CCPoint PositionWorldspace
        {
            get { return MyPart.Aircraft.ConvertToWorldspace(AircraftPosition); }
        }        
        internal CCPoint PositionModifyAircraft
        {
            get
            {
                float BORDER = 32f;
                CCPoint pos = PositionWorldspace;
                CCPoint vec = pos - MyPart.PositionWorldspace;
                if (vec.Equals(CCPoint.Zero))
                    vec = new CCPoint(1, 0);
                CCPoint vecNormalized = CCPoint.Normalize(vec);
                // scale the vector, so that the bounding boxes no longer touch (+ a little bit of extra space)
                CCPoint finalVec = CCPoint.Zero;
                CCRect box = MyPart.BoundingBoxTransformedToWorld;
                if (vec.Y == 0)
                {
                    if (vec.X < 0)
                        finalVec = new CCPoint(box.MinX - pos.X - BORDER, 0);
                    else
                        finalVec = new CCPoint(box.MaxX - pos.X + BORDER, 0);
                }
                else if (vec.X == 0)
                {
                    if (vec.Y < 0)
                        finalVec = new CCPoint(0, box.MinY - pos.Y - BORDER);
                    else
                        finalVec = new CCPoint(0, box.MaxY - pos.Y + BORDER);
                }
                else if (vec.Y < 0)
                {
                    if (vec.X < 0)
                    {
                        var vec1 = new CCPoint(box.MinX - pos.X - BORDER, vecNormalized.Y * Math.Abs(box.MinX - pos.X - BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MinY - pos.Y - BORDER), box.MinY - pos.Y - BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                    else
                    {
                        var vec1 = new CCPoint(box.MaxX - pos.X + BORDER, vecNormalized.Y * Math.Abs(box.MaxX - pos.X + BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MinY - pos.Y - BORDER), box.MinY - pos.Y - BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                }
                else
                {
                    if (vec.X < 0)
                    {
                        var vec1 = new CCPoint(box.MinX - pos.X - BORDER, vecNormalized.Y * Math.Abs(box.MinX - pos.X - BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MaxY - pos.Y + BORDER), box.MaxY - pos.Y + BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                    else
                    {
                        var vec1 = new CCPoint(box.MaxX - pos.X + BORDER, vecNormalized.Y * Math.Abs(box.MaxX - pos.X + BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MaxY - pos.Y + BORDER), box.MaxY - pos.Y + BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                }
                CCPoint returnVec = pos + finalVec;
                bool passed = false;
                while (!passed)
                {
                    passed = true;
                    foreach (var aircraftPart in MyPart.Aircraft.TotalParts)
                    {
                        if (Collisions.CollideBoundingBoxCircle(aircraftPart.BoundingBoxTransformedToWorld, returnVec, BORDER))
                        {
                            passed = false;
                            returnVec += finalVec;
                        }
                    }
                    foreach (var mountPoint in MyPart.PartMounts)
                    {
                        if (mountPoint == this) continue;
                        else if (MyPart.PartMounts.IndexOf(this) > MyPart.PartMounts.IndexOf(mountPoint) && mountPoint.Available && mountPoint.PositionModifyAircraft.IsNear(returnVec, BORDER))
                        {
                            passed = false;
                            returnVec += vecNormalized * BORDER;
                        }
                    }
                }
                return returnVec;
            }
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
            else if (CanMount(part))
            {
                MountedPart = part;
                part.MountParent = MyPart;
                // make sure the part is actually at the position of the mount
                UpdateMountedPartPosition();
                //Console.WriteLine("MyPart Position: " + MyPart.Position);
                //Console.WriteLine("Mount: " + part.Position);
                // if it was sucessfully mounted add it as a child of your parts parent (the aircraft)
                // and tell it that your part is its mount-parent now
                // if the mount point is lower than the anchor of the body the part has to be flipped (mirror on x axis)
                if ((MyPart.PosLeftLower.Y + Position.Y <  MyPart.Aircraft.Body.PositionY && part.Flipped == false) ||
                    (MyPart.PosLeftLower.Y + Position.Y >= MyPart.Aircraft.Body.PositionY && part.Flipped == true))
                    part.Flip();
                foreach (var singlePart in part.TotalParts)
                    MyPart.Parent.AddChild(singlePart, zOrder: singlePart.CalcZOrder());
                (MyPart.Parent as Aircraft).PartsChanged();
                return true;
            };
            return false;
        }

        internal bool CanMount(Part part)
        {
            return AllowedTypes.Intersect(part.Types).Any() && AllowedSizes.Contains(part.MySizeType);
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
                // also remove all parts that are mounted on the removed part
                foreach (Part part in mountedPart.TotalParts)
                    aircraft.RemoveChild(part);
                aircraft.PartsChanged();
            }
            // return the formerly mounted part
            return mountedPart;
        }
    }
}
