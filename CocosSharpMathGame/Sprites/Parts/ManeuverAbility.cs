using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Objects of this type describe the ability of a part to contribute to the maneuverability and propellment of an aircraft.
    /// </summary>
    internal class ManeuverAbility
    {
        /// <summary>
        /// How much propellment this part can generate (per second)
        /// </summary>
        internal float PowerMax { get; private protected set; }
        /// <summary>
        /// How much propellment this part HAS TO generate (for parts that cannot be tuned down more than a certain degree)
        /// </summary>
        internal float PowerMin { get; private protected set; }
        /// <summary>
        /// How much this part may contribute to rotation exclusively and on top of what is given as PowerMax/Min;
        /// this means that this power can only be used to calculate rotation;
        /// parts like ailerons for example may ONLY contribute to rotation, they cannot help propell the aircraft forward
        /// </summary>
        internal float RotationBonusMax { get; private protected set; }
        /// <summary>
        /// How much this part HAS TO contribute to rotation exclusively and on top (for parts that cannot be tuned down more than a certain degree)
        /// </summary>
        internal float RotationBonusMin { get; private protected set; }
        internal ManeuverAbility(float powerMin, float powerMax, float rotationBonusMin = 0, float rotationBonusMax = 0)
        {
            PowerMax = powerMax;
            PowerMin = powerMin;
            RotationBonusMax = rotationBonusMax;
            RotationBonusMin = rotationBonusMin;
        }
    }
}
