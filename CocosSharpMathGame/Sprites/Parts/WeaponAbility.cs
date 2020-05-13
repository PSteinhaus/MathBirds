using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Describes how a part can aim and shoot.
    /// </summary>
    internal class WeaponAbility
    {
        internal float AttentionAngle { get; set; }
        internal float AttentionRange { get; set; }
        internal float ShootingAngle { get; set; }
        internal float ShootingRange { get; set; }
        internal bool FireAtWill { get; set; } = true;
        internal float RateOfFire { get; set; }
        internal float TurningAnglePerSecond { get; set; }
    }
}
