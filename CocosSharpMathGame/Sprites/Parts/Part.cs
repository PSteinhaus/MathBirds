using System;
using System.Collections.Generic;
using System.IO;
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
    abstract internal class Part : GameObjectSprite, ICollidible, IStreamSaveable
    {
        internal bool Flipped { get; private protected set; } = false;
        internal float Health { get; private protected set; } = 10f; //float.PositiveInfinity; // standard behaviour is indestructability
        internal float MaxHealth { get; private protected set; } = 10f;
        internal enum State
        {
            ACTIVE, DESTROYED
        }
        internal State MyState { get; private protected set; } = State.ACTIVE;
        public CollisionType CollisionType { get; set; }
        internal List<DamageCloudTailNode> DamageCloudTailNodes { get; } = new List<DamageCloudTailNode>();
        /// <summary>
        /// The rotation this part starts at
        /// </summary>
        internal float NullRotation { get; set; }
        internal float RotationFromNull
        {
            get
            {
                return Constants.AngleFromToDeg(NullRotation, MyRotation);
            }
        }
        // returns the TotalRotation that this part would have if it was in NullRotation
        internal float TotalNullRotation
        {
            get
            {
                return TotalRotation - RotationFromNull;
            }
        }
        /// <summary>
        /// Returns the Aircraft that this part belongs to (if any else null)
        /// </summary>
        internal Aircraft Aircraft
        {
            get
            {
                return Parent as Aircraft;
            }
        }
        /// <summary>
        /// returns the position of this part relative to the aircraft;
        /// to be precise it is the difference between the left lower corner of the aircraft and the left lower corner of the part;
        /// </summary>
        internal CCPoint PosLeftLower
        {
            get
            {
                return Parent != null ?
                    new CCPoint(BoundingBoxTransformedToParent.MinX, Flipped ? BoundingBoxTransformedToParent.MaxY : BoundingBoxTransformedToParent.MinY) :
                    new CCPoint(BoundingBox.MinX, Flipped ? BoundingBox.MaxY : BoundingBox.MinY);
            }
        }
        /// <summary>
        /// whether and how this part may actively contribute to the maneuverability of the aircraft
        /// </summary>
        internal ManeuverAbility ManeuverAbility { get; private protected set; } = null;
        /// <summary>
        /// whether and how this part can act as a weapon
        /// </summary>
        internal WeaponAbility WeaponAbility { get; private protected set; }
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
            BODY, SINGLE_WING, WINGS, ENGINE, GUN,
            RUDDER,
            ROTOR
        }
        internal Type[] Types { get; set; }
        internal enum SizeType
        {
            TINY, SMALL, REGULAR, LARGE, HUGE, GIANT, GARGANTUAN
        }
        internal SizeType MySizeType { get; set; } = SizeType.REGULAR;
        /// <summary>
        /// Holds the SpriteFrames for all parts
        /// </summary>
        static internal CCSpriteSheet spriteSheet = new CCSpriteSheet("parts.plist");
        internal Part MountParent { get; set; } = null;

        internal void EnterWorkshopConfiguration()
        {
            const float BORDER = 8f;
            foreach (var part in MountedParts)
            {
                if (part == null) continue;
                CCPoint oldPos = part.Position;
                // move all mounted parts from their mount points
                // move them in the direction of the vector going from your position to the position of the part
                CCPoint vec = part.Position - this.Position;
                if (vec.Equals(CCPoint.Zero))
                    vec = new CCPoint(1, 0);
                CCPoint vecNormalized = CCPoint.Normalize(vec);
                // scale the vector, so that the bounding boxes no longer touch (+ a little bit of extra space)
                CCPoint finalVec = CCPoint.Zero;
                CCRect box = BoundingBoxTransformedToParent;
                CCRect boxPart = part.BoundingBoxTransformedToParent;
                if (vec.Y == 0)
                {
                    if (vec.X < 0)
                        finalVec = new CCPoint(box.MinX - boxPart.MaxX - BORDER, 0);
                    else
                        finalVec = new CCPoint(box.MaxX - boxPart.MinX + BORDER, 0);
                }
                else if (vec.X == 0)
                {
                    if (vec.Y < 0)
                        finalVec = new CCPoint(0, box.MinY - boxPart.MaxY - BORDER);
                    else
                        finalVec = new CCPoint(0, box.MaxY - boxPart.MinY + BORDER);
                }
                else if (vec.Y < 0)
                {
                    if (vec.X < 0)
                    {
                        var vec1 = new CCPoint(box.MinX - boxPart.MaxX - BORDER, vecNormalized.Y * Math.Abs(box.MinX - boxPart.MaxX - BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MinY - boxPart.MaxY - BORDER), box.MinY - boxPart.MaxY - BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                    else
                    {
                        var vec1 = new CCPoint(box.MaxX - boxPart.MinX + BORDER, vecNormalized.Y * Math.Abs(box.MaxX - boxPart.MinX + BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MinY - boxPart.MaxY - BORDER), box.MinY - boxPart.MaxY - BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                }
                else
                {
                    if (vec.X < 0)
                    {
                        var vec1 = new CCPoint(box.MinX - boxPart.MaxX - BORDER, vecNormalized.Y * Math.Abs(box.MinX - boxPart.MaxX - BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MaxY - boxPart.MinY + BORDER), box.MaxY - boxPart.MinY + BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                    else
                    {
                        var vec1 = new CCPoint(box.MaxX - boxPart.MinX + BORDER, vecNormalized.Y * Math.Abs(box.MaxX - boxPart.MinX + BORDER));
                        var vec2 = new CCPoint(vecNormalized.X * Math.Abs(box.MaxY - boxPart.MinY + BORDER), box.MaxY - boxPart.MinY + BORDER);
                        finalVec = vec1.Length < vec2.Length ? vec1 : vec2;
                    }
                }
                // move the part
                part.Position += finalVec;
                // to make sure that there are no intersections check for intersections with all other parts (exept the ones that belong to your hierarchy)
                var myParts = part.TotalParts;
                foreach (var aircraftPart in Aircraft.TotalParts)
                {
                    if (myParts.Contains(aircraftPart)) continue;
                    bool collisionDetected = false;
                    while (aircraftPart.BoundingBoxTransformedToParent.IntersectsRect(part.BoundingBoxTransformedToParent))
                    {
                        collisionDetected = true;
                        part.Position += vecNormalized;
                    }
                    if (collisionDetected)
                        part.Position += vecNormalized * BORDER;
                }
                // move all the parts that belong to you with you
                foreach (var mountedPart in part.TotalParts)
                    if (mountedPart != part)
                        mountedPart.Position += part.Position - oldPos;
                
                part.EnterWorkshopConfiguration();
            }
        }

        internal void EnterHangarConfiguration()
        {
            foreach (var part in MountedParts)
            {
                if (part == null) continue;
                // move all mounted parts back to their mount points
                foreach (var mount in PartMounts)
                    if (mount.MountedPart == part) mount.UpdateMountedPartPosition();
                part.EnterHangarConfiguration();
            }
        }

        internal PartMount[] PartMounts { get; private protected set; } = new PartMount[0];

        internal void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
                Die();
        }
        internal void Die(bool callPartsChanged=true)
        {
            Health = 0;
            MyState = State.DESTROYED;
            Color = CCColor3B.DarkGray; // TODO: switch to a "destroyed" version of the sprite instead of just darkening it
            // reduce your mass (half it for now)
            for (int i=0; i<MassPoints.Length; i++)
                MassPoints[i].Mass /= 2;
            // add a special destruction circle cloud that will follow you
            if (!DamageCloudTailNodes.Any())
            {
                var dctNode = new DamageCloudTailNode(0, CCPoint.Zero);
                dctNode.AutoAddClouds = false;
                DamageCloudTailNodes.Add(dctNode);
            }
            var destructionCircleCloud = new CircleCloud(PositionWorldspace, 0, CCColor4B.White, true, (ContentSize.Width + ContentSize.Height) * GetTotalScale() + 100f, 5f);
            destructionCircleCloud.FollowTarget = this;
            DamageCloudTailNodes.First().AddCloud(destructionCircleCloud);
            // also kill all parts that are mounted to you
            foreach (var part in MountedParts)
            {
                part.Die(false);
            }
            if (callPartsChanged)
                Aircraft.PartsChanged(deathPossible: true);
        }

        internal void ReactToHit(float damage, CCPoint collisionPos)
        {
            const float MAX_DISTANCE_FROM_COLLISION = 200f;
            float maxDistanceFromCollision = ((ContentSize.Width + ContentSize.Height) / 4) * GetTotalScale();
            if (maxDistanceFromCollision > MAX_DISTANCE_FROM_COLLISION) maxDistanceFromCollision = MAX_DISTANCE_FROM_COLLISION;
            // generate a random point near the collisionPos
            // and show an effect there
            Random rng = new Random();
            while(true)
            {
                CCPoint randomPoint = Constants.RandomPointNear(collisionPos, maxDistanceFromCollision, rng);
                //CCPoint randomPoint = collisionPos;
                if (Collisions.CollidePositionPolygon(randomPoint, this))   // check whether the random point is inside the collision polygon
                {
                    CCPoint relativePosition = randomPoint - PositionWorldspace;
                    relativePosition = CCPoint.RotateByAngle(relativePosition, CCPoint.Zero, -Constants.CCDegreesToMathRadians(TotalRotation));
                    // react differently depending upon how damaged you now are
                    var damageTail = new DamageCloudTailNode(DamageToReferenceSize(damage), relativePosition);
                    if (Health / MaxHealth > 0.7f)
                        damageTail.AutoAddClouds = false;
                    if (Health / MaxHealth < 0.5f)
                    {
                        byte value = (byte)rng.Next(105, 256);
                        damageTail.CloudColor = new CCColor4B((byte)rng.Next(value, 256), value, 0);
                        //damageTail.CloudColor = rng.Next(0, 2) == 0 ? CCColor4B.Yellow : CCColor4B.Red;
                        //damageTail.CloudColor = rng.Next(0,3) == 0 ? CCColor4B.Black : new CCColor4B(255, 255, 255);
                        //damageTail.CloudColor = new CCColor4B(255, 255, 255);
                    }
                    DamageCloudTailNodes.Add(damageTail);
                    break;
                }
            }
            // shake!
            if (Aircraft.ControlledByPlayer)
            {
                // the shake amount depends on the damage, but mostly on how the damage relates to the max health
                float baseRefSize = DamageToReferenceSize(damage, 10f);
                float specialBonus = 0f;
                float healthFactor = damage / MaxHealth;
                if (healthFactor > 1) healthFactor = 1;
                if (Health <= 0)
                {
                    baseRefSize *= (1 + TotalParts.Count()) * 0.8f;
                    specialBonus += TotalMaxHealth;
                }
                float shakeAmount = baseRefSize * 16 * healthFactor + specialBonus;
                ((PlayLayer)Layer).AddScreenShake(shakeAmount, shakeAmount);
            }
        }

        /// <summary>
        /// Also includes the health of mounted parts (all mounted parts, i.e. all found by recursion)
        /// </summary>
        internal float TotalHealth
        {
            get
            {
                float health = 0;
                foreach (var part in TotalParts)
                    health += part.Health;
                return health;
            }
        }

        /// <summary>
        /// Also includes the max health of mounted parts (all mounted parts, i.e. all found by recursion)
        /// </summary>
        internal float TotalMaxHealth
        {
            get
            {
                float maxHealth = 0;
                foreach (var part in TotalParts)
                    maxHealth += part.MaxHealth;
                return maxHealth;
            }
        }

        /// <summary>
        /// Returns how many mount connections can be found from here to the body
        /// </summary>
        /// <returns></returns>
        internal int NumOfMountParents()
        {
            if (this == Aircraft.Body)
                return 0;
            else
                return MountParent.NumOfMountParents() + 1;
        }

        private float DamageToReferenceSize(float damage, float maxRefSize = 50f)
        {
            float refSize = damage * 6;
            return refSize <= maxRefSize ? refSize : maxRefSize;
        }

        /// <summary>
        /// searches recursively and returns all parts that are mounted on this part including itself
        /// </summary>
        internal List<Part> TotalParts
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
        /// <summary>
        /// Returns only the directly mounted parts (no recursion)
        /// </summary>
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
        /// Mirror the part on its x axis
        /// </summary>
        internal void Flip()
        {
            Flipped = !Flipped;
            RotationX = Flipped ? 180f : 0; 
            for (int i = 0; i < MassPoints.Length; i++)
                MassPoints[i].Y *= -1;
            // flip the collision polygon
            ((CollisionTypePolygon)CollisionType).collisionPolygon.MirrorOnXAxis();
            for (int i = 0; i < PartMounts.Length; i++)
            {
                var pm = PartMounts[i];
                pm.Position = new CCPoint(pm.Position.X, -pm.Position.Y);
                pm.NullRotation = -pm.NullRotation;
                // don't forget to update the mounted part and flip it as well
                if (pm.MountedPart != null)
                {
                    pm.UpdateMountedPartPosition();
                    pm.MountedPart.Flip();
                }
            }
        }

        /// <summary>
        /// tries to mount a given part on a PartMount of this Part
        /// </summary>
        /// <param name="partMount"></param>
        /// <param name="part"></param>
        /// <param name="dz">the difference in ZOrder between the mounted part and this part</param>
        /// <returns>whether it was successfully mounted</returns>
        internal bool MountPart (PartMount partMount, Part part)
        {
            // check whether the PartMount is actually one of yours and proceed if true
            for (int i=0; i<PartMounts.Length; i++)
                if (PartMounts[i].Equals(partMount))
                    return MountPart(i, part);
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
        internal bool MountPart (int mountIndex, Part part)
        {
            bool mounted = false;
            // check the index
            if (mountIndex < PartMounts.Length)
                mounted = PartMounts[mountIndex].MountPart(part);
            return mounted;
        }

        internal int CalcZOrder()
        {
            // calculate the ZOrder recursively
            int dz = 0;
            Part part = this;
            while (part!=null && part.MountPoint != null)
            {
                dz += part.MountPoint.Dz;
                part = part.MountParent;
            }
            return dz;
        }

        internal PartMount MountPoint
        {
            get
            {
                PartMount mp = null;
                if (MountParent == null) return null;
                foreach (var mountPoint in MountParent.PartMounts)
                    if (mountPoint.MountedPart == this)
                    {
                        mp = mountPoint;
                        break;
                    }
                return mp;
            }
        }

        internal bool MountPart (Part part)
        {
            bool mounted = false;
            // try to mount the part somewhere and stop if you succeed
            for (int i=0; i<PartMounts.Length && mounted==false; i++)
                mounted = MountPart(i, part);
            return mounted;
        }

        internal void ExecuteOrders(float dt)
        {
            switch (MyState)
            {
                case State.ACTIVE:
                    if (WeaponAbility != null && Aircraft.MyState != Aircraft.State.SHOT_DOWN)
                        WeaponAbility.ExecuteOrders(dt);
                    if (ManeuverAbility != null)
                        ManeuverAbility.ExecuteOrders(dt, PositionWorldspace, TotalRotation);
                    break;
                case State.DESTROYED:
                    if (ManeuverAbility != null)
                        ManeuverAbility.ExecuteOrders(dt, PositionWorldspace, TotalRotation, decayOnly: true);
                    break;
            }
            foreach (var damageTail in DamageCloudTailNodes)
                damageTail.Advance(dt, PositionWorldspace, TotalRotation);
        }

        protected MassPoint[] CreateDiamondMassPoints(float massPerPoint)
        {
            var points = DiamondCollisionPoints();
            var massPoints = new MassPoint[4];
            for (int i = 0; i < massPoints.Length; i++)
                massPoints[i] = new MassPoint(points[i].X, points[i].Y, massPerPoint);
            return massPoints;
        }

        /// <summary>
        /// unmounts all mounted parts including parts mounted on monuted parts etc.
        /// </summary>
        /// <returns></returns>
        internal void Disassemble()
        {
            foreach (var part in TotalParts)
                part.UnmountAll();
        }

        /// <summary>
        /// unmounts all directly mounted parts
        /// </summary>
        internal void UnmountAll()
        {
            foreach (PartMount partMount in PartMounts)
                partMount.UnmountPart();
        }

        internal void Unmount(Part part)
        {
            foreach (PartMount partMount in PartMounts)
                if (partMount.MountedPart == part)
                {
                    partMount.UnmountPart();
                    break;
                }
        }

        protected enum StreamEnum : byte
        {
            STOP = 0, NAME = 1, MOUNT = 2
        }
        public void WriteToStream(BinaryWriter writer)
        {
            // write the type name
            writer.Write((byte)StreamEnum.NAME);
            writer.Write(GetType().AssemblyQualifiedName);
            // write down your mount children
            for(int i=0; i<PartMounts.Length; i++)
            {
                var mountedPart = PartMounts[i].MountedPart;
                if (mountedPart != null)
                {
                    writer.Write((byte)StreamEnum.MOUNT);
                    writer.Write(i);    // save the mount index, so that the part can be correctly remounted later
                    mountedPart.WriteToStream(writer);
                }
            }
            writer.Write((byte)StreamEnum.STOP);
        }

        public static Part CreateFromStream(BinaryReader reader, Aircraft associatedAircraft=null, bool isBody=false)
        {
            Part createdPart = null;
            bool reading = true;
            while (reading)
            {
                StreamEnum nextEnum = (StreamEnum)reader.ReadByte();
                switch (nextEnum)
                {
                    case StreamEnum.NAME:
                        {
                            // parse the type name
                            createdPart = (Part)TypeHelper.CreateFromTypeName(reader.ReadString());
                            // if this part is the body of an aircraft assign it right away
                            if (associatedAircraft != null)
                            {
                                if (isBody) associatedAircraft.Body = createdPart;
                                else        associatedAircraft.AddChild(createdPart);
                            }
                        }
                        break;
                    case StreamEnum.MOUNT:
                        {
                            // construct and mount a part
                            int mountIndex = reader.ReadInt32();
                            createdPart.MountPart(mountIndex, CreateFromStream(reader, associatedAircraft));
                        }
                        break;
                    case StreamEnum.STOP:
                    default:
                        reading = false;
                        break;
                }
            }
            return createdPart;
        }
    }
}
